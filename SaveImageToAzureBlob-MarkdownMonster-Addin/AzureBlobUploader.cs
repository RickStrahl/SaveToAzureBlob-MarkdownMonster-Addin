using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Azure.Storage.Blobs;

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
        public async Task<string> SaveFileToAzureBlobStorage(string filename, string connectionStringName, string blobName = null)
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
                // Get a reference to a container named "sample-container" and then create it
                BlobContainerClient container = new BlobContainerClient(blobConnection.DecryptConnectionString(), blobConnection.ContainerName);
                await container.CreateIfNotExistsAsync();

                // Get a reference to a blob named "sample-file" in a container named "sample-container"
                BlobClient blob = container.GetBlobClient(blobName);

                // Upload local file
                var response =  await blob.UploadAsync(filename);

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
        public async Task<string> SaveBitmapSourceToAzureBlobStorage(BitmapSource image, string connectionStringName, string blobName)
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

                // Get a reference to a container named "sample-container" and then create it
                BlobContainerClient container = new BlobContainerClient(blobConnection.DecryptConnectionString(), blobConnection.ContainerName);
                await container.CreateIfNotExistsAsync();

                
                // Get a reference to a blob named "sample-file" in a container named "sample-container"
                BlobClient blob = container.GetBlobClient(blobName);


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
                MemoryStream ms = new MemoryStream();
                using (ms)
                {
                    encoder.Save(ms);
                    ms.Flush();
                    ms.Position = 0; 
                    
                    var response = await blob.UploadAsync(ms);
                    return blob.Uri.ToString();
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.GetBaseException().Message;
            }

            return null;
        }
    }
}
