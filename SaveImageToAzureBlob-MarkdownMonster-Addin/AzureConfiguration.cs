using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using MarkdownMonster;
using MarkdownMonster.AddIns;
using SaveImageToAzureBlobStorageAddin.Annotations;
using Westwind.Utilities.Configuration;

namespace SaveImageToAzureBlobStorageAddin
{
    public class AzureConfiguration : BaseAddinConfiguration<AzureConfiguration>
    {        
        public List<AzureBlobConnection> ConnectionStrings { get; set; }

        static AzureConfiguration()
        {            
            Current = new AzureConfiguration();
            Current.Initialize();
        }
        
        public AzureConfiguration()
        {
            ConfigurationFilename = "SaveToAzureBlobStorageAddIn.json";
            ConnectionStrings = new List<AzureBlobConnection>();
        }

                //#region AppConfiguration
        //protected override IConfigurationProvider OnCreateDefaultProvider(string sectionName, object configData)
        //{
        //    var provider = new JsonFileConfigurationProvider<AzureConfiguration>()
        //    {
        //        JsonConfigurationFile = Path.Combine(mmApp.Configuration.CommonFolder, "SaveToAzureBlobStorageAddIn.json")
        //    };

        //    if (!File.Exists(provider.JsonConfigurationFile))
        //    {
        //        if (!Directory.Exists(Path.GetDirectoryName(provider.JsonConfigurationFile)))
        //            Directory.CreateDirectory(Path.GetDirectoryName(provider.JsonConfigurationFile));
        //    }

        //    return provider;
        //}
        //#endregion

        //#region INotifyPropertyChanged
        //public event PropertyChangedEventHandler PropertyChanged;

        
        //protected virtual void OnPropertyChanged(string propertyName)
        //{
        //    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        //}
        //#endregion
    }
}
