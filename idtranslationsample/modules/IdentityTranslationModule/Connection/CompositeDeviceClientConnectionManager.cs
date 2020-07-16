using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using MQTTnet;
using NodaTime;

using IdentityTranslationModule.Messaging;
using IdentityTranslationModule.Controller;

namespace IdentityTranslationModule.Connection
{
    public class CompositeDeviceClientConnectionManager 
    {
        private readonly ILogger logger;
        private IDeviceRepository deviceRepository;
        private readonly MqttFactory factory;
        private readonly IDictionary<string, CompositeDeviceClient> deviceClients;
        private IClock clock;
         
        private readonly IList<CompositeDeviceClient> connectedClients = new List<CompositeDeviceClient>();
        //private IServiceProvider provider;
        private ILoggerFactory loggerFactory;

        private IConfiguration Configuration; 

        public CompositeDeviceClientConnectionManager(ILoggerFactory loggerFactory, IConfiguration config, IDeviceRepository repository, IClock clock, ILogger<CompositeDeviceClientConnectionManager> logger)
        {
            this.deviceRepository = repository;
            this.logger = logger; 

            logger.LogInformation("***************CompositeDeviceClientConnectionManager created");
            factory = new MqttFactory();
            deviceClients = new Dictionary<string, CompositeDeviceClient>();
            this.Configuration = config;
            this.loggerFactory = loggerFactory;
            this.clock = clock;
        }

        public async Task StartAsync(CancellationToken stopToken)
        {
            await Task.Run(async () => await Run(stopToken) );
        }


        private async Task Run(CancellationToken stopToken)
        {
            // Manage list of devices 
            logger.LogInformation("Running");

            stopToken.Register(() => {
                logger.LogInformation("Stopping");
            });

            await ConnectAllDevices(stopToken);

            while (!stopToken.IsCancellationRequested)
            {
                //logger.LogInformation("Manager is managing");
                var propertyBag = new Dictionary<string, String>() { 
                    {"app-event-time", DateTime.Now.ToString() },
                    {"x-app-specific", "asdfsaf"}
                };
                //await connectedClients[0].SendUpstreamMessage($"Message at {DateTime.Now}", propertyBag, stopToken);
                await Task.Delay(10000, stopToken);
            }

            logger.LogInformation("Exiting");
        }


        private async Task ConnectAllDevices(CancellationToken stopToken)
        {
            
            foreach(var d in deviceRepository.AllDevices()) 
            {
                logger.LogInformation($"Connecting device {d.IothubDeviceId}");

                try
                {

                    var handler = CreateIoTEdgeMessageHandler(d.IotEdgeController);
                    var mqttHandler = CreateMqttMessageHandler(d.MqttController);
                    var client = CompositeDeviceClient.CreateInModule(Configuration,
                        d,
                        handler,
                        mqttHandler,
                        clock, 
                        loggerFactory);

                    await client.Connect(stopToken);
                    logger.LogInformation($"mqttSubscription: {d.LocalDeviceMqttSubscriptions.DeviceToCloudTopics[0]}");

                    connectedClients.Add(client);
                }
                catch (Exception ex)
                {
                    logger.LogError($"Error creating composite device ({d.IothubDeviceId}, {d.LocalDeviceId}) with error: {ex}");
                }
            }

            await Task.CompletedTask;
        }

        private Messaging.MessageHandler CreateMqttMessageHandler(string controllerName)
        {
            // TODO: Create controller based on controller name
            var m = new MqttNetMessageHandler(loggerFactory.CreateLogger<MqttNetMessageHandler>());
            var controller = new DeviceMqttController(loggerFactory.CreateLogger<DeviceMqttController>());
            var mqtt = new MqttDeviceMessageHandler(controller, loggerFactory.CreateLogger<MqttDeviceMessageHandler>());
            m.SetNext(mqtt);
            return m;
        }
        private Messaging.MessageHandler CreateIoTEdgeMessageHandler(string controllerName) 
        {
            // TODO: Create controller based on controller name
            var m = new MqttNetMessageHandler(loggerFactory.CreateLogger<MqttNetMessageHandler>());
            var controller = new DeviceIotEdgeController(loggerFactory.CreateLogger<DeviceIotEdgeController>());
            var edge = new IoTEdgeMessageHandler(controller, loggerFactory.CreateLogger<IoTEdgeMessageHandler>());
            m.SetNext(edge);
            return m;
        }
    }
}
