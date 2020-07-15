using System;

namespace IdentityTranslationModule.Connection
{
    public sealed class MqttBrokerCredentials 
    {
        public string BrokerAddress {get; set;}   
        public int BrokerPort {get; set;}

        public string ClientId {get; set;}

        public string UserName {get; set;}
        public string Password {get; set;}

        public string CACertificateFile {get;set;}

        public string ClientCertificateFile {get;set;}

        public string ClientKeyFile {get;set;}

        public bool UseTls
        {
            get 
            {
                if (string.IsNullOrWhiteSpace(CACertificateFile) || string.IsNullOrWhiteSpace(ClientCertificateFile))
                {
                    return false;
                }
                return true;
            }
        }

        public bool RequiresClientKeyFile
        {
            get
            {
                if (UseTls && ClientCertificateFile.EndsWith(".pfx", StringComparison.CurrentCultureIgnoreCase))
                {
                    return false;
                }
                return true;
            }
        }

        public bool UseCredentials
        {
            get 
            {
                if (string.IsNullOrWhiteSpace(Password))
                {
                    return false;
                }
                return true;
            }
        }

    }
}