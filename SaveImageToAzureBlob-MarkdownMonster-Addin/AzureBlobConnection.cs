﻿using System;
using System.Security.Cryptography;
using System.Text;
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

        private readonly byte[] cnstr = new byte[] {45, 66, 222, 12, 87, 88, 32, 97, 113, 179};


        /// <summary>
        /// Url to the Azure Blob Storage Container root folder so
        /// you can quickly browse the container contents
        /// </summary>
        public string AzurePortalContainerUrl { get; set; }

        public AzureBlobConnection()
        {
        }

        public string EncryptConnectionString(bool force = false)
        {
            if (string.IsNullOrEmpty(ConnectionString))
                return ConnectionString;

            string postfix = "~~";
            if (!force && ConnectionString.EndsWith(postfix))
                return ConnectionString;

            var encryptBytes = ProtectedData.Protect(Encoding.UTF8.GetBytes(ConnectionString),
                cnstr, DataProtectionScope.LocalMachine);
            ConnectionString = Convert.ToBase64String(encryptBytes) + postfix;

            return ConnectionString;
        }

        public string DecryptConnectionString()
        {
            if (string.IsNullOrEmpty(ConnectionString))
                return ConnectionString;

            string postfix = "~~";
            if (!ConnectionString.EndsWith(postfix))
                return ConnectionString;

            var striped = ConnectionString.Substring(0, ConnectionString.Length - 2);
            var bytes = Convert.FromBase64String(striped);
            bytes = ProtectedData.Unprotect(bytes, cnstr, DataProtectionScope.LocalMachine);
            return Encoding.UTF8.GetString(bytes);
        }
    }
}