using System;
using System.Collections;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Client.Transport.Mqtt;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace edge2edgemodule
{

    class Edge2EdgeModuleService : IHostedService
    {

        private readonly ILogger logger;
        private readonly IHostApplicationLifetime appLifetime;
        private string IOTEDGE_TRUSTED_CA_CERTIFICATE_PEM_PATH;
        private string MODULE_VERSION;
        private string IOTEDGE_DEVICEID;
        private string IOTEDGE_MODULEID;

        private string IOTEDGE_IOTHUBHOSTNAME;
        const string EdgehubConnectionstringVariableName = "EdgeHubConnectionString";
        const string IothubConnectionstringVariableName = "IotHubConnectionString";

        private int Counter = 0;
        private static DeviceClient ParentDeviceClient;
        
        public Edge2EdgeModuleService(ILogger<Edge2EdgeModuleService> logger,
            IHostApplicationLifetime applicationLifetime,
            IConfiguration config) 
        {
            IOTEDGE_TRUSTED_CA_CERTIFICATE_PEM_PATH = config.GetValue<string>("IOTEDGE_TRUSTED_CA_CERTIFICATE_PEM_PATH");
            MODULE_VERSION = config.GetValue<string>("MODULE_VERSION");
            IOTEDGE_DEVICEID = config.GetValue<string>("IOTEDGE_DEVICEID");
            IOTEDGE_MODULEID = config.GetValue<string>("IOTEDGE_MODULEID");
            IOTEDGE_IOTHUBHOSTNAME = config.GetValue<string>("IOTEDGE_IOTHUBHOSTNAME");

            logger.LogInformation($"Environment:\nModule Version: {MODULE_VERSION}\nDeviceId: {IOTEDGE_DEVICEID}\nModuleID: {IOTEDGE_MODULEID}\nIoTHubHostName: {IOTEDGE_IOTHUBHOSTNAME}");
            this.logger = logger;
            this.appLifetime = applicationLifetime;
            logger.LogInformation($"CA Cert path is: {IOTEDGE_TRUSTED_CA_CERTIFICATE_PEM_PATH}");

        }

        string GetValueFromEnvironment(IDictionary envVariables, string variableName)
        {
            if (envVariables.Contains(variableName))
            {
                return envVariables[variableName].ToString();
            }

            return null;
        }
         private async Task<bool> Init()
        {
            IDictionary envVariables = Environment.GetEnvironmentVariables();

            var conn1 = this.GetValueFromEnvironment(envVariables, EdgehubConnectionstringVariableName);
            var conn2 = this.GetValueFromEnvironment(envVariables, IothubConnectionstringVariableName);
            var version = this.GetValueFromEnvironment(envVariables, "MODULE_VERSION");

            logger.LogInformation($"Conn1: {conn1}\nConn2: {conn2}\nVersion: {version}");

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

            var parentEdge = twin.Properties.Desired["parentEdge"];

            logger.LogInformation($"edge2edgeKey is: {parentEdge.edge2edgeKey}");

            
            string deviceConnectionString = parentEdge.parentEdgeConnectionString;
            logger.LogInformation($"Connecting to parent device with connection stringz {deviceConnectionString}");

            ParentDeviceClient = DeviceClient.CreateFromConnectionString(deviceConnectionString);

            logger.LogInformation("$Connection created");

            if (ParentDeviceClient == null)
            {
                logger.LogError("Failed to create DeviceClient for Parent");
            } else
            {
                logger.LogInformation("Opening device connection");
                await ParentDeviceClient.OpenAsync();
                logger.LogInformation("Connection opened...");

                var stopDataPoint = new
                {
                    messageId = -1,
                };

                string messageString = JsonConvert.SerializeObject(stopDataPoint);
                Message message = new Message(Encoding.ASCII.GetBytes(messageString));
                message.Properties.Add("status", "started");
                await ParentDeviceClient.SendEventAsync(message);
                logger.LogInformation("Start message sent");

            }
            
            // Register callback to be called when a message is received by the module
            await ioTHubModuleClient.SetInputMessageHandlerAsync("input1", PipeMessage, ioTHubModuleClient);
            return true;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("==>Begin: StartAsync");
            appLifetime.ApplicationStarted.Register(OnStarted);
            appLifetime.ApplicationStopping.Register(OnStopping);
            appLifetime.ApplicationStopped.Register(OnStopped);

            InstallCACert();
            logger.LogInformation("Certificates installed");
            await Init();

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


        public Task StopAsync(CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        private void OnStarted() 
        {
            logger.LogInformation("OnStarted has been called.");
        }

        private void OnStopping()
        {
            logger.LogInformation("OnStopped has been called.");

            // Perform post-stopped activities here
        }

        private void OnStopped()
        {
            logger.LogInformation("OnStopped has been called.");

            // Perform post-stopped activities here
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

                    if (ParentDeviceClient != null) {
                        pipeMessage.Properties.Add("ChildEdgeDeviceId", IOTEDGE_DEVICEID);
                        pipeMessage.Properties.Add("ChildEdgeModuleId", IOTEDGE_MODULEID);
                        pipeMessage.Properties.Add("ChildEdgeIoTHub", IOTEDGE_IOTHUBHOSTNAME);
                        pipeMessage.Properties.Add("ChildEdgeModuleVersion", MODULE_VERSION);
                        await ParentDeviceClient.SendEventAsync(pipeMessage);
                    }
                    
                }
            }
            return MessageResponse.Completed;
        }
    }
}