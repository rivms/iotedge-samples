using System;
using System.Collections.Generic;
using Microsoft.Azure.Devices.Client;
using Microsoft.Extensions.Configuration;

namespace IdentityTranslationModule.Connection
{

    public class EdgeTools
    {
        public static string GetDeviceConnectionFromEnvironment(IConfiguration config, string deviceId, string sasKey) 
        {
            return null;
        }


        public static MqttBrokerCredentials GetMqttDeviceCredentials(CompositeDeviceConfiguration.Device device)
        {
            var m = new MqttBrokerCredentials {
                BrokerAddress = device.MqttBrokerAddress,
                BrokerPort = device.MqttBrokerPort,
                ClientId = device.LocalDeviceId,
                Password = device.MqttPassword,
                UserName = device.MqttUserName
            };
            return m;
        }

        public static IotHubDeviceCredentials GetDeviceCredentialsFromModule(string moduleConnectionString, string deviceId, string sasKey)
        {
            var moduleConnectionDictionary = ConnectionStringAsDictionary(moduleConnectionString);

            return new IotHubDeviceCredentials {
                HostName = moduleConnectionDictionary["hostname"],
                DeviceId = deviceId,
                SaSKey = sasKey,
                GatewayHostName = moduleConnectionDictionary["gatewayhostname"]
            };
        }

        public static string GetDeviceConnectionFromModule(string moduleConnectionString, string deviceId, string sasKey) 
        {
            var creds = GetDeviceCredentialsFromModule(moduleConnectionString, deviceId, sasKey);
            return creds.IoTEdgeConnectionString;
        }

        public static IDictionary<string, string> ConnectionStringAsDictionary(string connectionString) 
        {
            var d = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
            var pairs = connectionString.Split(';');

            foreach(var pair in pairs) 
            {
                var kv = pair.Split('=');
                d.Add(kv[0].ToLower(), kv[1]);
            }

            return d;
        }


        public static string GenerateSasToken(string iotHubHostName, string deviceId, string key, TimeSpan expiry)
        {
            var builder = new SharedAccessSignatureBuilder()
            {
                Key = key,
                Target = $"{iotHubHostName}/devices/{deviceId}",
                TimeToLive = expiry
            };

            return builder.ToSignature();
        }
    }
}
