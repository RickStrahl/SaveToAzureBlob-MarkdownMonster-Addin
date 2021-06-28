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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using FontAwesome.WPF;
using MarkdownMonster;
using MarkdownMonster.AddIns;
using Westwind.Utilities;

namespace SaveImageToAzureBlobStorageAddin
{


    /// <summary>
    /// Markdown Monster Add-in that allows opening or pasting of local images
    /// to Azure Blob Storage and retrieve a URL that is automatically embedded
    /// into the current document.
    /// </summary>
    public class SaveToAzureBlobStorageAddin : MarkdownMonsterAddin
    {
        public string ErrorMessage { get; set; }
        

        public override Task OnApplicationStart()
        {
            base.OnApplicationStart();

            Id = "SaveImageToAzureBlobStorage";

            Name = "Save Image to Azure Blob Storage";

            // by passing in the add in you automatically
            // hook up OnExecute/OnExecuteConfiguration/OnCanExecute
            var menuItem = new AddInMenuItem(this)
            {
                Caption = "Save Image to Azure _Blob Storage",

                // if an icon is specified it shows on the toolbar
                // if not the add-in only shows in the add-ins menu
                FontawesomeIcon = FontAwesomeIcon.CloudUpload,
                FontawesomeIconColor = "Steelblue"
            };
            menuItem.KeyboardShortcut = "Shift-Alt-B";

            // if you don't want to display main or config menu items clear handler
            //menuItem.ExecuteConfiguration = null;

            // Must add the menu to the collection to display menu and toolbar items            
            MenuItems.Add(menuItem);

            return Task.CompletedTask;
        }

        public override Task OnExecute(object sender)
        {          
            var form = new PasteImageToAzureWindow(this);
            form.ShowDialog();

            if (!string.IsNullOrEmpty(form.ImageUrl))
            {
                SetSelection("![](" + form.ImageUrl + ")");
                SetEditorFocus();                                
                RefreshPreview();
            }

            return Task.CompletedTask;
        }
    

        public override Task OnExecuteConfiguration(object sender)
        {
            var form = new PasteImageToAzureConfigurationWindow(this);
            form.Owner = Model.Window;
            form.Show();

            return Task.CompletedTask;
        }

        public override bool OnCanExecute(object sender)
        {
            return Model.IsEditorActive;
        }


        /// <summary>
        /// Saves a file from local disk to Azure Blob storage and a given blob name.
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="connectionStringName"></param>
        /// <param name="blobName"></param>
        /// <returns></returns>
        public string SaveFileToAzureBlobStorage(string filename, string connectionStringName, string blobName = null)
        {
            if (string.IsNullOrEmpty(blobName))
                blobName = GetBlobFilename(filename);

            if (string.IsNullOrEmpty(blobName))
                return null;

            var uploader = new AzureBlobUploader();
            string url = uploader.SaveFileToAzureBlobStorage(filename, connectionStringName, blobName);

            if (string.IsNullOrEmpty(url))
            {
                ErrorMessage = uploader.ErrorMessage;
                return null;
            }

            url = url.Replace(" ", "%20");
            return url;
        }
        

        /// <summary>
        /// Saves an image directly from the image control - when pasting from clipboard
        /// </summary>
        /// <param name="image"></param>
        /// <param name="connectionStringName"></param>
        /// <param name="blobName"></param>
        /// <returns></returns>
        public string SaveBitmapSourceToAzureBlobStorage(BitmapSource image, string connectionStringName, string blobName)
        {
            if (string.IsNullOrEmpty(blobName))
                blobName = GetBlobFilename();

            if (string.IsNullOrEmpty(blobName))
                return null;


            var uploader = new AzureBlobUploader();
            string url = uploader.SaveBitmapSourceToAzureBlobStorage(image, connectionStringName, blobName);

            if (string.IsNullOrEmpty(url))
            {
                ErrorMessage = uploader.ErrorMessage;
                return null;
            }

            return url.Replace(" ","%20");            
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
    }
}
