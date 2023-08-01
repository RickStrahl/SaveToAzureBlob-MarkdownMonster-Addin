using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MahApps.Metro.Controls;
using MarkdownMonster;
using MarkdownMonster.Windows;
using Microsoft.Win32;

namespace SaveImageToAzureBlobStorageAddin
{    
    public partial class PasteImageToAzureWindow : MetroWindow, INotifyPropertyChanged   
    {
        public SaveToAzureBlobStorageAddin Addin { get; set; }

        public AzureConfiguration Configuration { get; set; }


        public string ImageUrl { get; set; }

        StatusBarHelper Status { get; set; }
        

        public string ImageFilename
        {
            get { return _ImageFilename; }
            set
            {
                if (value == _ImageFilename) return;
                _ImageFilename = value;
                OnPropertyChanged(nameof(ImageFilename));
                OnPropertyChanged(nameof(IsSaveEnabled));
            }
        }
        private string _ImageFilename = "";
        

        public string BlobFileName
        {
            get { return _blobFileName; }
            set
            {
                if (_blobFileName == value) return;
                _blobFileName = value;
                OnPropertyChanged();
            }
        }
        private string _blobFileName;


        
        public bool IsSaveEnabled
        {
            get
            {
                bool enabled = IsBitmap || !string.IsNullOrEmpty(ImageFilename);
                if (enabled)
                    FaSaveImage.Foreground = Brushes.LightGreen;
                else
                    FaSaveImage.Foreground = Brushes.Silver;
                return enabled;
            }            
        }

        public bool IsBitmap
        {
            get { return _isBitmap; }
            set { if (_isBitmap == value) return;
                _isBitmap = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsSaveEnabled));

               
            }
        }
        private bool _isBitmap;

        public AzureBlobConnection ActiveConnection { get; set; }
        
        
        #region Intialization

        public PasteImageToAzureWindow(SaveToAzureBlobStorageAddin addin)
        {
            Addin = addin;
            Configuration = AzureConfiguration.Current;

            Top = Addin.Model.Window.Top;
            Left = Addin.Model.Window.Left;


            InitializeComponent();

            DataContext = this;
            mmApp.SetThemeWindowOverride(this);

            Status = new StatusBarHelper(StatusText, StatusIcon);

            Loaded += PasteImageToAzure_Loaded;
            SizeChanged += PasteImageToAzureWindow_SizeChanged;        
        }

        private void PasteImageToAzureWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {

            //TextFilename.Width = Width - 440;

            var image = ImagePreview.Source as BitmapSource;
            if (image == null)
                return;

            if (image.Width < Width - 20 && image.Height < PageGrid.RowDefinitions[1].ActualHeight)
                ImagePreview.Stretch = Stretch.None;
            else
                ImagePreview.Stretch = Stretch.Uniform;

            
        }

        private void PasteImageToAzure_Loaded(object sender, RoutedEventArgs e)
        {
            if (ClipboardHelper.ContainsImage())
                ToolButtonPasteImage_Click(this, null);
        }
        #endregion


        public void PasteImage()
        {
            if (!ClipboardHelper.ContainsImage())
            {
                Status.ShowStatusError("Clipboard doesn't contain an image.", 6000);
                return;
            }
            
            ImagePreview.Source = ClipboardHelper.GetImageSource();

            BlobFileName = Addin.GetBlobFilename();
            if (string.IsNullOrEmpty(BlobFileName))
            {
                Status.ShowStatusError("No filename selected.", 6000);
                return;
            }
            
            PasteImageToAzureWindow_SizeChanged(this, null);

            IsBitmap = true;
            ImageFilename = null;

            // Get just the filename to highlight
            var justFile = Path.GetFileName(BlobFileName);
            var start = BlobFileName.Length - justFile.Length;

            TextFilename.Focus();
            TextFilename.Select(start, justFile.Length - 4);

            Status.ShowStatusSuccess("Image pasted from Clipboard.", 8000);
        }


        #region Event Handlers        

        private void ToolButtonOpenImage_Click(object sender, RoutedEventArgs e)
        {
            ImageFilename = null;
            IsBitmap = false;

            var fd = new OpenFileDialog
            {
                DefaultExt = ".png",
                Filter = "Image files (*.png;*.jpg;*.gif;)|*.png;*.jpg;*.jpeg;*.gif|All Files (*.*)|*.*",
                CheckFileExists = true,
                RestoreDirectory = true,
                Multiselect = false,
                Title = "Embed Image"
            };

            string markdownFile = Addin.Model.ActiveDocument?.Filename;

            if (!string.IsNullOrEmpty(markdownFile))
                fd.InitialDirectory = System.IO.Path.GetDirectoryName(markdownFile);
            else
            {
                if (!string.IsNullOrEmpty(Addin.Model.ActiveDocument?.LastImageFolder))
                    fd.InitialDirectory = Addin.Model.ActiveDocument.LastImageFolder;
                else
                    fd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            }

            var res = fd.ShowDialog();
            if (res == null || !res.Value)
                return;

            // display in preview control
            ImageFilename = fd.FileName;
            Uri fileUri = new Uri("file:///" + fd.FileName.Replace("\\", "/"));
            ImagePreview.Source = new BitmapImage(fileUri);
            PasteImageToAzureWindow_SizeChanged(this, null);
            BlobFileName = Addin.GetBlobFilename(ImageFilename);
            IsBitmap = false;
        }

        private void ToolButtonPasteImage_Click(object sender, RoutedEventArgs e)
        {
            PasteImage();
        }

        private void ToolButtonClearImage_Click(object sender, RoutedEventArgs e)
        {
            ImageUrl = null;
            IsBitmap = false;
            ImagePreview.Source = null;
            BlobFileName = null;
        }
        
        private async void ToolButtonSaveToAzure_Click(object sender, RoutedEventArgs e)
        {
            ImageUrl = null;

            if (IsBitmap)
            {
                var image = ImagePreview.Source as BitmapSource;
                ImageUrl = await Addin.SaveBitmapSourceToAzureBlobStorage(image, ActiveConnection.Name, BlobFileName);
            }
            else            
                ImageUrl = await Addin.SaveFileToAzureBlobStorage(ImageFilename, ActiveConnection.Name, BlobFileName);
            

            if (ImageUrl == null)
                Status.ShowStatusError("Image upload failed: " + Addin.ErrorMessage, 8000);
            else
                Close();
        }

        private void ToolButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            ImageUrl = null;
            Close();
        }


        private void PasteImageToAzureForm_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.V &&
                (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
                PasteImage();
        }

        private void PasteImageToAzureForm_Activated(object sender, EventArgs e)
        {
            if (ImagePreview.Source == null && ClipboardHelper.ContainsImage())
                PasteImage();
        }

        #endregion

        #region IPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;
        
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion


        private void ToolButtonConfiguration_Click(object sender, RoutedEventArgs e)
        {
            var form = new PasteImageToAzureConfigurationWindow(Addin);
            form.Owner = this;
            form.Show();
        }
    }
}
