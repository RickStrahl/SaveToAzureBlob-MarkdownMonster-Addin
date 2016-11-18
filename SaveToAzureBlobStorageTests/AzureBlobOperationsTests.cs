using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SaveToAzureBlobStorage;
using Westwind.Utilities;

namespace SaveToAzureBlobStorageTests
{
    [TestClass]
    public class AzureBlobOperationsTests
    {
        [TestMethod]
        public void UploadFileTest()
        {
            var addin = new SaveToAzureBlobStorageAddin();
            var url = addin.SaveFileToAzureBlobStorage("c:\\sailbig.jpg", "West Wind Weblog Images");

            Assert.IsNotNull(url);

            Console.WriteLine(url);
            ShellUtils.GoUrl(url);
        }

        [TestMethod]
        public void UploadBitmapSourceTest()
        {
            
        }
    }
}
