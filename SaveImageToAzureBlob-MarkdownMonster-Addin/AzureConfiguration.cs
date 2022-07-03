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
    /// <summary>
    /// Configuration class that holds any configuration values associated with the Addin.
    /// In this case only a collection of Connection entries are stored that hold connection
    /// string credentials and container names. The connection strings are assumed to be
    /// encrypted.
    /// </summary>
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
    }
}
