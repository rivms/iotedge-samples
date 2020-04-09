using System;
using System.Collections;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using IdentityTranslationModule.Connection;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Client.Transport.Mqtt;
using Microsoft.Azure.Devices.Shared;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace IdentityTranslationModule
{

    class IdentityTranslationService 
    {

        private readonly ILogger logger;
        private string IOTEDGE_TRUSTED_CA_CERTIFICATE_PEM_PATH;
        private string MODULE_VERSION;
         private string IOTEDGE_DEVICEID;
        private string IOTEDGE_MODULEID;

        private string IOTEDGE_IOTHUBHOSTNAME;
        const string EdgehubConnectionstringVariableName = "EdgeHubConnectionString";
        const string IothubConnectionstringVariableName = "IotHubConnectionString";

        public IHost Host {get; set;}

        private int Counter = 0;

        private IConfiguration Configuration; 

        private CompositeDeviceClientConnectionManager ConnectionManager {get; set;}

        
        public IdentityTranslationService(ILogger<IdentityTranslationService> logger,
            IHostApplicationLifetime applicationLifetime,
            IConfiguration config) 
        {
            Console.WriteLine("Hello Service!");
            IOTEDGE_TRUSTED_CA_CERTIFICATE_PEM_PATH = config.GetValue<string>("IOTEDGE_TRUSTED_CA_CERTIFICATE_PEM_PATH");
            MODULE_VERSION = config.GetValue<string>("MODULE_VERSION");
            IOTEDGE_DEVICEID = config.GetValue<string>("IOTEDGE_DEVICEID");
            IOTEDGE_MODULEID = config.GetValue<string>("IOTEDGE_MODULEID");
            IOTEDGE_IOTHUBHOSTNAME = config.GetValue<string>("IOTEDGE_IOTHUBHOSTNAME");

            logger.LogInformation($"Environment:\nModule Version: {MODULE_VERSION}\nDeviceId: {IOTEDGE_DEVICEID}\nModuleID: {IOTEDGE_MODULEID}\nIoTHubHostName: {IOTEDGE_IOTHUBHOSTNAME}");
            this.logger = logger;
            logger.LogInformation($"CA Cert path is: {IOTEDGE_TRUSTED_CA_CERTIFICATE_PEM_PATH}");
            this.Configuration = config; 

        }

        string GetValueFromEnvironment(IDictionary envVariables, string variableName)
        {
            if (envVariables.Contains(variableName))
            {
                return envVariables[variableName].ToString();
            }

            return null;
        }
         private async Task<bool> Init(CancellationToken stopToken)
        {
            IDictionary envVariables = Environment.GetEnvironmentVariables();

            var conn1 = this.GetValueFromEnvironment(envVariables, EdgehubConnectionstringVariableName);

            var version = this.GetValueFromEnvironment(envVariables, "MODULE_VERSION");

            logger.LogInformation($"Conn1: {conn1}\n");


            MqttTransportSettings mqttSetting = new MqttTransportSettings(TransportType.Mqtt_Tcp_Only);
            ITransportSettings[] settings = { mqttSetting };

            logger.LogInformation("Creating module client");
            // Open a connection to the Edge runtime
            ModuleClient ioTHubModuleClient = await ModuleClient.CreateFromEnvironmentAsync(settings);
            logger.LogInformation("Opening module connection");

            try
            {
                logger.LogInformation($"Module Info: {ioTHubModuleClient.ProductInfo}");
                await ioTHubModuleClient.OpenAsync();

                TwinCollection reportedProperties, connectivity;
                reportedProperties = new TwinCollection();
                connectivity = new TwinCollection();
                connectivity["type"] = "cellular";
                reportedProperties["connectivity"] = connectivity;
                
                await ioTHubModuleClient.UpdateReportedPropertiesAsync(reportedProperties);
            }
            catch (Exception ex)
            {
                logger.LogError(ex.ToString());
                return false;
            }
            logger.LogInformation("IoT Hub module client initialized.");
            var twin = await ioTHubModuleClient.GetTwinAsync();

            var twinJson = twin.Properties.Desired.ToJson();

            logger.LogInformation($"Twin properties found:\n{twinJson}");

            string deviceListFileUri = twin.Properties.Desired["deviceListFile"];
            
            logger.LogInformation($"*Desired property: deviceListFile = {deviceListFileUri}");

            var repo = JSONDeviceRepository.CreateFromUrl(new Uri(deviceListFileUri));

            logger.LogInformation($"Device mappinng list: {repo}");

            var dl = Host.Services.GetRequiredService<ILogger<CompositeDeviceClientConnectionManager>>();
            dl.LogInformation("About to create ConnectionManager");

            logger.LogInformation($"Created logger for connection manager {dl}");
            // TODO: Pass reference to logger factory or refactor to inject factories in general for client dependencies
            ConnectionManager = new CompositeDeviceClientConnectionManager(Host.Services, Configuration,  repo, 
                dl);

            await ConnectionManager.StartAsync(stopToken);
          
            // Register callback to be called when a message is received by the module
            await ioTHubModuleClient.SetInputMessageHandlerAsync("input1", PipeMessage, ioTHubModuleClient);
            return true;
        }


        public async Task StartAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("==>Begin: StartAsync");

            InstallCACert();
            logger.LogInformation("Certificates installed");
            await Init(cancellationToken);

            logger.LogInformation("==>End: StartAsync");

            //return Task.CompletedTask;

        }

        private void InstallCACert()
        {
            string trustedCACertPath = IOTEDGE_TRUSTED_CA_CERTIFICATE_PEM_PATH;//Environment.GetEnvironmentVariable("IOTEDGE_TRUSTED_CA_CERTIFICATE_PEM_PATH");
            if (!string.IsNullOrWhiteSpace(trustedCACertPath))
            {
                logger.LogInformation("User configured CA certificate path: {0}", trustedCACertPath);
                if (!File.Exists(trustedCACertPath))
                {
                    // cannot proceed further without a proper cert file
                    logger.LogError("Certificate file not found: {0}", trustedCACertPath);
                    throw new InvalidOperationException("Invalid certificate file.");
                }
                else
                {
                    logger.LogInformation("Attempting to install CA certificate: {0}", trustedCACertPath);
                    X509Store store = new X509Store(StoreName.Root, StoreLocation.CurrentUser);
                    store.Open(OpenFlags.ReadWrite);
                    store.Add(new X509Certificate2(X509Certificate.CreateFromCertFile(trustedCACertPath)));
                    logger.LogInformation("Successfully added certificate: {0}", trustedCACertPath);
                    store.Close();
                }
            }
            else
            {
                Console.WriteLine("CA_CERTIFICATE_PATH was not set or null, not installing any CA certificate");
            }
        }


 
        async Task<MessageResponse> PipeMessage(Message message, object userContext)
        {

            // Echo message to parent IoT Edge
            // Add a property to indicate this is a hierarchical message
            int counterValue = Interlocked.Increment(ref Counter);

            var moduleClient = userContext as ModuleClient;
            if (moduleClient == null)
            {
                throw new InvalidOperationException("UserContext doesn't contain " + "expected values");
            }

            byte[] messageBytes = message.GetBytes();
            string messageString = Encoding.UTF8.GetString(messageBytes);
            Console.WriteLine($"Received message: {counterValue}, Body: [{messageString}]");

            if (!string.IsNullOrEmpty(messageString))
            {
                using (var pipeMessage = new Message(messageBytes))
                {
                    foreach (var prop in message.Properties)
                    {
                        pipeMessage.Properties.Add(prop.Key, prop.Value);
                    }
                    await moduleClient.SendEventAsync("output1", pipeMessage);
                
                    Console.WriteLine("Received message sent");
                    
                }
            }
            return MessageResponse.Completed;
        }
    }
}