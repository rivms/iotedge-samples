using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using IdentityTranslationModule.Connection;


namespace IdentityTranslationModule.Tests
{
    [TestClass]
    public class JSONDeviceRepositoryTests
    {
        public JSONDeviceRepositoryTests()
        {

        }

        [Ignore]
        [TestMethod]
        public void TestCredentialMapping_Deserialization()
        {
            Uri uri = new Uri("https://zzz.blob.core.windows.net/");
            var fileText = NetworkTools.DownloadTextFile(uri);
            var repo = NetworkTools.DeserializeJson<CompositeDeviceConfiguration>(fileText);

            Assert.IsNotNull(repo, "Should not be null");
            Assert.AreEqual(2, repo.Devices.Count, "Mismated # of mappings");
            var compare1 = SameDevice(repo.Devices[0], "deviceidt001", "zzz", "localdevice001");
            Assert.IsTrue(compare1, "Device 1 mismatch");

            var compare2 = SameDevice(repo.Devices[1], "deviceidt002", "zzz", "localdevice002");
            Assert.IsTrue(compare2, "Device 2 mismatch");
        }


        private static bool SameDevice(CompositeDeviceConfiguration.Device device, string iothubDeviceId, string sasKey, string localDeviceId)
        {
            return device.IothubDeviceId == iothubDeviceId 
                && device.SasKey == sasKey
                && device.LocalDeviceId == localDeviceId;
        }
    }
}
