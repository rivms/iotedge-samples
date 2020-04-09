using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using IdentityTranslationModule.Connection;


namespace IdentityTranslationModule.Tests
{
    [TestClass]
    public class NetworkToolsTests
    {
        public NetworkToolsTests()
        {

        }

        [TestMethod]
        public void Download_FromBlob()
        {
            Uri uri = new Uri("https://iotst001.blob.core.windows.net/idtranslation/deviceidlist.json?si=iotedge&sv=2019-02-02&sr=b&sig=%2BINZYKEwsw8ytwPmUwezJjig5j6WRnRg0Rd6Q0%2FKLek%3D");
            var repo = NetworkTools.DownloadTextFile(uri);

            Assert.IsFalse(repo.Length == 0, "Blank file downloaded");
        }
    }
}
