using System;
using System.Collections.Generic;

using Newtonsoft.Json;

namespace IdentityTranslationModule.Connection
{
    public class CompositeDeviceConfiguration
    {
        public class Device {

            public class MqttTopic
            {
                public string Name {get; set;}
                public string Topic {get; set;}
            }

            public class MqttSubscriptions 
            {
                [JsonProperty("deviceToCloudTopics")]
                public List<MqttTopic> DeviceToCloudTopics {get; set;}

                [JsonProperty("twinRequestTopics")]
                public List<MqttTopic> TwinRequestTopics { get; set; }

                [JsonProperty("decoder")]
                public string Decoder {get; set;}
            }


            public class MqttPublications 
            {
                [JsonProperty("cloudToDevice")]
                public List<MqttTopic> CloudToDevice {get; set;}

                [JsonProperty("directMethods")]
                public List<MqttTopic> DirectMethods {get; set;}

                [JsonProperty("twin")]
                public List<MqttTopic> Twin {get; set;}

                [JsonProperty("encoder")]
                public string Encoder {get; set;}
            }

            [JsonProperty("iothubDeviceId")]
            public String IothubDeviceId {get; set;}

            [JsonProperty("sasKey")]
            public String SasKey {get; set;}

            [JsonProperty("localDeviceId")]
            public string LocalDeviceId {get; set;}

            [JsonProperty("mqttUseCleanSession")]
            public bool MqttUseCleanSession {get;set;}

            [JsonProperty("mqttBrokerAddress")]
            public string MqttBrokerAddress {get; set;}

            [JsonProperty("mqttBrokerPort")]
            public int MqttBrokerPort {get; set;}

            [JsonProperty("mqttUserName")]
            public string MqttUserName {get; set;}

            [JsonProperty("mqttPassword")]
            public string MqttPassword {get; set;}

            [JsonProperty("mqttCACertificateFile")]
            public string MqttCACertificateFile {get;set;}

            [JsonProperty("mqttClientCertificateFile")]
            public string MqttClientCertificateFile {get;set;}

            [JsonProperty("mqttClientKeyFile")]
            public string MqttClientKeyFile {get;set;}

            [JsonProperty("iotEdgeController")]
            public string IotEdgeController { get; set; }

            [JsonProperty("mqttController")]
            public string MqttController { get; set; }

            [JsonProperty("localDeviceMqttSubscriptions")]
            public MqttSubscriptions LocalDeviceMqttSubscriptions {get;set;}

            [JsonProperty("localDeviceMqttPublications")]
            public MqttPublications LocalDeviceMqttPublications {get;set;}
        }

        [JsonProperty("devices")]
        public List<Device> Devices {get; set;} 
    }
}

