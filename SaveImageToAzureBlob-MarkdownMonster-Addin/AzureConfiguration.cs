using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using MarkdownMonster;
using SaveToAzureBlobStorage.Annotations;
using Westwind.Utilities.Configuration;

namespace SaveToAzureBlobStorage
{
    public class AzureConfiguration : AppConfiguration, INotifyPropertyChanged  
    {
        public static AzureConfiguration Current;

        public List<AzureBlobConnection> ConnectionStrings { get; set; }

        static AzureConfiguration()
        {
            Current = new AzureConfiguration();
            Current.Initialize();
        }
        
        public AzureConfiguration()
        {
            ConnectionStrings = new List<AzureBlobConnection>();
        }


        #region AppConfiguration
        protected override IConfigurationProvider OnCreateDefaultProvider(string sectionName, object configData)
        {
            var provider = new JsonFileConfigurationProvider<AzureConfiguration>()
            {
                JsonConfigurationFile = Path.Combine(mmApp.Configuration.CommonFolder, "SaveToAzureBlobStorageAddIn.json")
            };

            if (!File.Exists(provider.JsonConfigurationFile))
            {
                if (!Directory.Exists(Path.GetDirectoryName(provider.JsonConfigurationFile)))
                    Directory.CreateDirectory(Path.GetDirectoryName(provider.JsonConfigurationFile));
            }

            return provider;
        }
        #endregion

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }

    public class AzureBlobConnection
    {
        /// <summary>
        ///  The unique name for this connection - displayed in UI
        /// </summary>
        public string Name { get; set; }


        /// <summary>
        /// The Azure connection string to connect to this blob storage account
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// Name of the container to connect to
        /// </summary>
        public string ContainerName { get; set; }
    }
}
