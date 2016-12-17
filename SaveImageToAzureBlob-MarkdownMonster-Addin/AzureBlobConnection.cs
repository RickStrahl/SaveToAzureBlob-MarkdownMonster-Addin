using MarkdownMonster;
using Westwind.Utilities;

namespace SaveImageToAzureBlobStorageAddin
{
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

        const string cnstr = "612dklj33as334r*44;dZpKl+y";

        public string EncryptConnectionString(bool force = false)
        {
            if (string.IsNullOrEmpty(ConnectionString))
                return ConnectionString;

            string postfix = "~~";
            if (!force && ConnectionString.EndsWith("~~"))
                return ConnectionString;

            ConnectionString = Encryption.EncryptString(ConnectionString,cnstr) + "~~";
            return ConnectionString;
        }

        public string DecryptConnectionString()
        {
            if (string.IsNullOrEmpty(ConnectionString))
                return ConnectionString;

            string postfix = "~~";
            if (!ConnectionString.EndsWith("~~"))
                return ConnectionString;

            var striped = ConnectionString.Substring(0, ConnectionString.Length - 2);
            return Encryption.DecryptString(striped, cnstr);
        }
    }
}