namespace IdentityTranslationModule.Connection
{
    public sealed class MqttBrokerCredentials 
    {
        public string BrokerAddress {get; set;}   
        public int BrokerPort {get; set;}

        public string ClientId {get; set;}

        public string UserName {get; set;}
        public string Password {get; set;}

    }
}