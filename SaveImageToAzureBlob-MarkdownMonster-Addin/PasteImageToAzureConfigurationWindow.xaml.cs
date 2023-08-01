using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using MahApps.Metro.Controls;
using MarkdownMonster;
using SaveImageToAzureBlobStorageAddin.Annotations;

namespace SaveImageToAzureBlobStorageAddin
{
    /// <summary>
    /// Interaction logic for PasteHref.xaml
    /// </summary>
    public partial class PasteImageToAzureConfigurationWindow : MetroWindow, INotifyPropertyChanged   
    {
        private AzureBlobConnection _activeConnection;

        public AzureBlobConnection ActiveConnection
        {
            get
            {
                if (_activeConnection == null)
                    _activeConnection = new AzureBlobConnection();
                return _activeConnection;
            }
            set
            {
                if (Equals(value, _activeConnection)) return;
                _activeConnection = value;
                OnPropertyChanged();
            }
        }
        

        public ObservableCollection<AzureBlobConnection> Connections
        {
            get { return _connections; }
            set
            {
                if (Equals(value, _connections)) return;
                _connections = value;
                OnPropertyChanged();
            }
        }
        private ObservableCollection<AzureBlobConnection> _connections;

        public PasteImageToAzureConfigurationWindow(SaveToAzureBlobStorageAddin addin)
        {
            InitializeComponent();

            mmApp.SetThemeWindowOverride(this);

            Loaded += PasteImageToAzureConfigurationWindow_Loaded;
            Unloaded += PasteImageToAzureConfigurationWindow_Unloaded;
        }
        
        private void PasteImageToAzureConfigurationWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Connections = new ObservableCollection<AzureBlobConnection>(AzureConfiguration.Current.ConnectionStrings);

            ActiveConnection = Connections.FirstOrDefault();
            if (ActiveConnection == null)
                ActiveConnection = new AzureBlobConnection();

            DataContext = this;
        }

        private void PasteImageToAzureConfigurationWindow_Unloaded(object sender, RoutedEventArgs e)
        {
            AzureConfiguration.Current.ConnectionStrings.Clear();
            foreach (var connection in Connections)
            {
                AzureConfiguration.Current.ConnectionStrings.Add(connection);
            }

            // save the configuration when you exit
            AzureConfiguration.Current.Write();
        }

        private void TextConnectionString_LostFocus(object sender, RoutedEventArgs e)
        {
            if (ActiveConnection == null)
                return;

            string val = TextConnectionString.Text;
            TextConnectionString.Text = "<hiden for security - type to change>";

            if (string.IsNullOrEmpty(val) || val.StartsWith("<"))
                return;

            ActiveConnection.ConnectionString = val;
            ActiveConnection.EncryptConnectionString(true);            
        }


        private void TextConnectionString_GotFocus(object sender, RoutedEventArgs e)
        {
            if (TextConnectionString.Text.StartsWith("<")) 
                TextConnectionString.Text = "";
        }

        private void ButtonNewConnection_Click(object sender, RoutedEventArgs e)
        {
            ActiveConnection = new AzureBlobConnection
            {
                Name = "New Connection"
            };
            Connections.Add(ActiveConnection);
        }

        private void ButtonDeleteConnection_Click(object sender, RoutedEventArgs e)
        {
            if (ActiveConnection != null)
            {
                var connection = Connections.FirstOrDefault(conn => conn.Name == ActiveConnection.Name);
                if (connection != null)
                    Connections.Remove(connection);
            }
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
