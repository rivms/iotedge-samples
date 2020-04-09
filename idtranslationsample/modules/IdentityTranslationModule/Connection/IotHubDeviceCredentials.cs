namespace IdentityTranslationModule.Connection
{
    public sealed class IotHubDeviceCredentials 
    {
        public string HostName { get; set; }
        public string DeviceId { get; set; }

        public string SaSKey { get; set; }

        public string GatewayHostName {get; set;}

        public string IoTHubConnectionString {
            get {
             var deviceConnectionString = $"HostName={HostName};DeviceId={DeviceId};SharedAccessKey={SaSKey}";
             return deviceConnectionString;
            } 
        }

        public string IoTEdgeConnectionString {
            get {
             return $"{IoTHubConnectionString};GatewayHostName={GatewayHostName}";
            }
        }

        public string MqttDeviceUserName { 
            get {
            return $"{HostName}/{DeviceId}/?api-version=2018-06-30";
            }
        }
        
    }
}