using System;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace SaveImageToAzureBlobStorageAddin
{
    public class AzureBlobUploader
    {
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Saves a file from local disk to Azure Blob storage and a given blob name.
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="connectionStringName"></param>
        /// <param name="blobName"></param>
        /// <returns></returns>
        public string SaveFileToAzureBlobStorage(string filename, string connectionStringName, string blobName = null)
        {
            if (filename == null || !File.Exists(filename))
            {
                ErrorMessage = "Invalid file name. No file specified or file doesn't exist.";
                return null;
            }

            var blobConnection = AzureConfiguration.Current.ConnectionStrings
                        .FirstOrDefault(cs => cs.Name.ToLower() == connectionStringName.ToLower());

            if (blobConnection == null)
            {
                ErrorMessage = "Invalid configuration string.";
                return null;
            }

            try
            {
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(blobConnection.DecryptConnectionString());

                // Create the blob client.
                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

                // Retrieve a reference to a container.
                CloudBlobContainer container = blobClient.GetContainerReference(blobConnection.ContainerName);

                // Create the container if it doesn't already exist.
                container.CreateIfNotExists();

                container.SetPermissions(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Blob });

                bool result = false;
                using (var fileStream = File.OpenRead(filename))
                {
                    result = UploadStream(fileStream, blobName, container);
                }

                if (!result)
                    return null;

                var blob = container.GetBlockBlobReference(blobName);

                return blob.Uri.ToString();
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.GetBaseException().Message;
                return null;
            }
        }

        /// <summary>
        /// Uploads an image directly from a BitmapSource. Used for uploading 
        /// images loaded into image control from clipboard so no intermediary
        /// file is required.
        /// </summary>
        /// <param name="image"></param>
        /// <param name="connectionStringName"></param>
        /// <param name="blobName"></param>
        /// <returns></returns>
        public string SaveBitmapSourceToAzureBlobStorage(BitmapSource image, string connectionStringName, string blobName)
        {

            var blobConnection = AzureConfiguration.Current.ConnectionStrings
                .FirstOrDefault(cs => cs.Name.ToLower() == connectionStringName.ToLower());

            if (blobConnection == null)
            {
                ErrorMessage = "Invalid configuration string.";
                return null;
            }

            try
            {
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(blobConnection.DecryptConnectionString());


                // Create the blob client.
                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

                // Retrieve a reference to a container.
                CloudBlobContainer container = blobClient.GetContainerReference(blobConnection.ContainerName);

                // Create the container if it doesn't already exist.
                container.CreateIfNotExists();

                container.SetPermissions(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Blob });

                // strip leading slashes - Azure will provide the trailing dash
                // on the domain.
                if (blobName.StartsWith("/") && blobName.Length > 1)
                    blobName = blobName.Substring(1);

                var extension = Path.GetExtension(blobName).Replace(".", "").ToLower();
                BitmapEncoder encoder;

                if (extension == "jpg" || extension == "jpeg")
                    encoder = new JpegBitmapEncoder();
                else if (extension == "gif")
                    encoder = new GifBitmapEncoder();
                else if (extension == ".bmp")
                    encoder = new BmpBitmapEncoder();
                else
                    encoder = new PngBitmapEncoder();

                encoder.Frames.Add(BitmapFrame.Create(image));

                bool result;
                using (var ms = new MemoryStream())
                {
                    encoder.Save(ms);
                    ms.Flush();
                    ms.Position = 0;

                    result = UploadStream(ms, blobName, container);
                }

                if (!result)
                    return null;

                var blob = container.GetBlockBlobReference(blobName);

                return blob.Uri.ToString();
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.GetBaseException().Message;
            }

            return null;
        }

        /// <summary>
        /// Uploads actual stream to Azure
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="blobName"></param>
        /// <param name="container"></param>
        /// <returns></returns>
        private bool UploadStream(Stream stream, string blobName, CloudBlobContainer container)
        {
            // Retrieve reference to a blob named "myblob".
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(blobName);

            // Create or overwrite the "myblob" blob with contents from a local file.
            blockBlob.UploadFromStream(stream);

            try
            {
                // set the content type of the image uploaded to be image/[png,jpg,gif,etc]
                // This fixes #8
                blockBlob.Properties.ContentType = $@"image/{Path.GetExtension(blobName).Substring(1)}";
                blockBlob.SetProperties();
            }
            catch (Exception ex)
            {
                ErrorMessage = $@"Error setting content type of the blob. Defaulted to 'application/octet-stream': {ex}";
            }

            return true;
        }
    }
}
