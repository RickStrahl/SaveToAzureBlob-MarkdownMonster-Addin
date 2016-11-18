#region License
/*
 **************************************************************
 *  Author: Rick Strahl 
 *          © West Wind Technologies, 2016
 *          http://www.west-wind.com/
 * 
 * Created: 11/17/2016
 *
 * Permission is hereby granted, free of charge, to any person
 * obtaining a copy of this software and associated documentation
 * files (the "Software"), to deal in the Software without
 * restriction, including without limitation the rights to use,
 * copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the
 * Software is furnished to do so, subject to the following
 * conditions:
 * 
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
 * OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
 * HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
 * WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
 * OTHER DEALINGS IN THE SOFTWARE.
 **************************************************************  
*/
#endregion


using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using FontAwesome.WPF;
using MarkdownMonster.AddIns;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Westwind.Utilities;

namespace SaveToAzureBlobStorage
{


    /// <summary>
    /// Markdown Monster Add-in that allows opening or pasting of local images
    /// to Azure Blob Storage and retrieve a URL that is automatically embedded
    /// into the current document.
    /// </summary>
    public class SaveToAzureBlobStorageAddin : MarkdownMonsterAddin
    {
        public string ErrorMessage { get; set; }

        
        

        public override void OnApplicationStart()
        {
            base.OnApplicationStart();

            Id = "SaveToAzureBlogStorage";

            // by passing in the add in you automatically
            // hook up OnExecute/OnExecuteConfiguration/OnCanExecute
            var menuItem = new AddInMenuItem(this)
            {
                Caption = "Save Image to Azure Blob Storage",

                // if an icon is specified it shows on the toolbar
                // if not the add-in only shows in the add-ins menu
                FontawesomeIcon = FontAwesomeIcon.CloudUpload,
            };

            // if you don't want to display main or config menu items clear handler
            menuItem.ExecuteConfiguration = null;

            // Must add the menu to the collection to display menu and toolbar items            
            MenuItems.Add(menuItem);            
        }

        public override void OnExecute(object sender)
        {
            var form = new PasteImageToAzureWindow(this);
            form.ShowDialog();

            if (!string.IsNullOrEmpty(form.ImageUrl))
            {
                var editor = Model.ActiveEditor;
                editor.SetSelection("![](" + form.ImageUrl + ")");
                Model.Window.Activate();                
                editor.SetEditorFocus();

                Model.Window.PreviewMarkdownAsync(keepScrollPosition: true);
            }
        }

    

        public override void OnExecuteConfiguration(object sender)
        {
            MessageBox.Show("Configuration for our sample Addin", "Markdown Addin Sample",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public override bool OnCanExecute(object sender)
        {
            return Model.IsEditorActive;
        }


        public string SaveFileToAzureBlobStorage(string filename, string connectionStringName, string blobName = null)
        {
            if (filename == null || !File.Exists(filename))
            {
                ErrorMessage = "Invalid file name. No file specified or file doesn't exist.";
                return null;
            }

            if (string.IsNullOrEmpty(blobName))
                blobName = GetBlobFilename(filename);

            var blobConnection = AzureConfiguration.Current.ConnectionStrings
              .FirstOrDefault(cs => cs.Name.ToLower() == connectionStringName.ToLower());

            if (blobConnection == null)
            {
                ErrorMessage = "Invalid configuration string.";
                return null;
            }

            try
            {
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(blobConnection.ConnectionString);

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

        private bool UploadStream(Stream stream, string blobName, CloudBlobContainer container)
        {
            // Retrieve reference to a blob named "myblob".
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(blobName);

            // Create or overwrite the "myblob" blob with contents from a local file.
            blockBlob.UploadFromStream(stream);

            return true;
        }

        /// <summary>
        /// Returns date prefixed filename ideal for blob storage
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="date"></param>
        /// <returns></returns>
        public string GetBlobFilename(string filename = null, DateTime? date = null)
        {
            if (date == null)
                date = DateTime.UtcNow;

            string file;
            if (string.IsNullOrEmpty(filename))
                file = StringUtils.NewStringId() + ".png";
            else
                file = Path.GetFileName(filename);

            return date.Value.ToString("yyyy/MM/dd/") + file;
        }


        public string SaveBitmapSourceToAzureBlobStorage(BitmapSource image,  string connectionStringName, string blobName = null)
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
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(blobConnection.ConnectionString);

                // Create the blob client.
                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

                // Retrieve a reference to a container.
                CloudBlobContainer container = blobClient.GetContainerReference(blobConnection.ContainerName);

                // Create the container if it doesn't already exist.
                container.CreateIfNotExists();

                container.SetPermissions(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Blob });


                
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
    }
}
