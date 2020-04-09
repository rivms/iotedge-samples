using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using IdentityTranslationModule.Connection;
using Microsoft.Azure.Devices.Client;

namespace IdentityTranslationModule.Tests
{
    [TestClass]
    public class EdgeoolsTests
    {
        public EdgeoolsTests()
        {

        }


        [TestMethod]
        public void Valid_Connection_String()
        {
            var deviceId = "did1128";
            var sasKey = "aaasssdddff";
            var moduleConnectionString = "HostName=iothubmqttdev01.azure-devices.net;GatewayHostName=desktop-81q7unf.homeops.net;DeviceId=ryzendesktopedge;ModuleId=IdentityTranslationModule;SharedAccessKey=HZlN9MhpZDQ4ClP8UTm6sizAIEPXOwdpBsT7RBiln1A=";
            var deviceConnectionString = $"HostName=iothubmqttdev01.azure-devices.net;DeviceId={deviceId};SharedAccessKey={sasKey};GatewayHostName=desktop-81q7unf.homeops.net";

            var conn = EdgeTools.GetDeviceConnectionFromModule(moduleConnectionString, deviceId, sasKey);

            Assert.AreEqual(deviceConnectionString, conn, "Connection string mismatch");

        }

    }
}
