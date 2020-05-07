using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using MQTTnet.Formatter;
using MQTTnet.Protocol;
using NodaTime;
using static IdentityTranslationModule.Connection.CompositeDeviceConfiguration.Device;

namespace IdentityTranslationModule.Connection
{

    public enum LocalDeviceSubscriptionCategory
    {
        Unknown = 0,
        DeviceToCloudTopics,
        TwinRequestTopics
    }

    public enum LocalDeviceMqttPublicationCategory
    {
        Unknown = 0,
        CloudToDevice,
        DirectMethod,
        Twin
    }

    public enum CompositeDeviceClientStatus {
        Disconnected = 1,
        Connected = 2,
        Retrying = 3,
        Failed = 4
    }

    public enum CompositeDeviceClientState 
    {
        Start = 0,
        Connected = 1
    }

    

    public sealed class CompositeDeviceClient
    {

        public CompositeDeviceClientStatus UpstreamConnectionStatus { get; private set; }
        public CompositeDeviceClientStatus DownstreamConnectionStatus { get; private set; }

        private static readonly MqttFactory factory = new MqttFactory();
        private IMqttClient downstreamClient;
        private IMqttClient upstreamClient;
        private CompositeDeviceConfiguration.Device device;
        private IClock clock;

        private Messaging.MessageHandler upstreamMessageHandler;
        private Messaging.MessageHandler downstreamMessageHandler; 
        //private CompositeDeviceClientState state = CompositeDeviceClientState.Start; 

        public IotHubDeviceCredentials upstreamClientCredentials {get; private set;}
        public MqttBrokerCredentials downstreamClientCredentials {get; private set;}

        public IDictionary<string, MqttTopic> cloudToDevicePublishingTopics = new Dictionary<string, MqttTopic>();
        public IDictionary<string, MqttTopic> directMethodPublishingTopics = new Dictionary<string, MqttTopic>();
        public IDictionary<string, MqttTopic> twinPublishingTopics = new Dictionary<string, MqttTopic>();
        public IDictionary<string, MqttTopic> deviceToCloudSubscriptions = new Dictionary<string, MqttTopic>();
        public IDictionary<string, MqttTopic> twiRequestSubscriptions = new Dictionary<string, MqttTopic>();

        public TwinStateLifecycle TwinStateLifecycle { get; private set; }

        private MqttDeviceClient mqttClient;
        private IotEdgeDeviceClient iotEdgeClient;

        private ILogger logger;
        public static CompositeDeviceClient CreateInModule(IConfiguration config, 
            CompositeDeviceConfiguration.Device device,
            Messaging.MessageHandler upstreamToDownstream,
            Messaging.MessageHandler downstreamToUpstream,
            IClock clock,
            ILoggerFactory loggerFactory)
        {
            var moduleConnectionString = config.GetValue<string>("EdgeHubConnectionString");
            var upstreamClientCredentials = EdgeTools.GetDeviceCredentialsFromModule(moduleConnectionString, device.IothubDeviceId, device.SasKey);
            var downstreamClientCredentials = EdgeTools.GetMqttDeviceCredentials(device);
            return new CompositeDeviceClient(device, upstreamClientCredentials, downstreamClientCredentials, upstreamToDownstream, downstreamToUpstream, clock, loggerFactory);
        }


        private CompositeDeviceClient(CompositeDeviceConfiguration.Device device, 
            IotHubDeviceCredentials upstreamDeviceCredentials, 
            MqttBrokerCredentials downstreamClientCredentials, 
            Messaging.MessageHandler upstreamToDownstream,
            Messaging.MessageHandler downstreamToUpstream,
            IClock clock, 
            ILoggerFactory loggerFactory)
        {
            this.logger = loggerFactory.CreateLogger<CompositeDeviceClient>();//provider.GetRequiredService<ILogger<CompositeDeviceClient>>(); 
            this.device = device;
            this.upstreamClientCredentials = upstreamDeviceCredentials;
            this.downstreamClientCredentials = downstreamClientCredentials;
            this.clock = clock;
            UpstreamConnectionStatus = CompositeDeviceClientStatus.Disconnected;
            DownstreamConnectionStatus = CompositeDeviceClientStatus.Disconnected;
            upstreamMessageHandler =  upstreamToDownstream;
            downstreamMessageHandler = downstreamToUpstream;


            this.TwinStateLifecycle = new TwinStateLifecycle(clock, null);

            // create lookups for topics
            AddTopicsToDictionary(device.LocalDeviceMqttPublications.CloudToDevice, cloudToDevicePublishingTopics);
            AddTopicsToDictionary(device.LocalDeviceMqttPublications.DirectMethods, directMethodPublishingTopics);
            AddTopicsToDictionary(device.LocalDeviceMqttPublications.Twin, twinPublishingTopics);
            AddTopicsToDictionary(device.LocalDeviceMqttSubscriptions.DeviceToCloudTopics, deviceToCloudSubscriptions);
            AddTopicsToDictionary(device.LocalDeviceMqttSubscriptions.TwinRequestTopics, twiRequestSubscriptions);

            mqttClient = new MqttDeviceClient(this, downstreamToUpstream, loggerFactory.CreateLogger <MqttDeviceClient>());
            iotEdgeClient = new IotEdgeDeviceClient(this, upstreamToDownstream, loggerFactory.CreateLogger <IotEdgeDeviceClient>());
        }


        public void AddTopicsToDictionary(IEnumerable<MqttTopic> topics, IDictionary<string, MqttTopic> mapping)
        {
            foreach(var t in topics)
            {
                mapping.Add(t.Name, t);
            }
        }


        private async Task ConnectionMqttDeviceClient(CancellationToken stopToken)
        {
            downstreamClient = factory.CreateMqttClient();

            var downstreamOptions = new MqttClientOptionsBuilder()
                .WithClientId(downstreamClientCredentials.ClientId)
                .WithTcpServer(downstreamClientCredentials.BrokerAddress, downstreamClientCredentials.BrokerPort)
                .WithCredentials(downstreamClientCredentials.UserName, downstreamClientCredentials.Password)
               // .WithTls()
                .WithCleanSession()
                .Build();


            try 
            {
                logger.LogInformation($"Connecting downstream client {downstreamClientCredentials.ClientId}");

                downstreamClient.UseConnectedHandler( async e => {
                    logger.LogInformation($"Device {downstreamClientCredentials.ClientId} connected");

                    // Subscribe to cloud to device messages
                    logger.LogDebug($"Begin Device to Cloud subscription for Device {downstreamClientCredentials.ClientId} connected");
                    foreach (var topic in device.LocalDeviceMqttSubscriptions.DeviceToCloudTopics)
                    {
                        logger.LogDebug($"Subscribing to topic {topic}");
                        await downstreamClient.SubscribeAsync(new TopicFilterBuilder().WithTopic(topic.Topic).Build());
                    }


                    // Subscribe to twin request messages
                    logger.LogDebug($"Begin Twin Request subscription for Device {downstreamClientCredentials.ClientId} connected");
                    foreach (var topic in device.LocalDeviceMqttSubscriptions.TwinRequestTopics)
                    {
                        logger.LogDebug($"Subscribing to topic {topic}");
                        await downstreamClient.SubscribeAsync(new TopicFilterBuilder().WithTopic(topic.Topic).Build());
                    }
                    await Task.CompletedTask;
                });

                downstreamClient.UseDisconnectedHandler( async e => {
                    logger.LogInformation($"Device {downstreamClientCredentials.ClientId} disconnected");
                    await Task.CompletedTask;
                });
                
                downstreamClient.UseApplicationMessageReceivedHandler( async e => {
                    await mqttClient.MessageReceivedHandler(e, stopToken);

                } );

                await downstreamClient.ConnectAsync(downstreamOptions, stopToken);
            } catch (Exception ex) 
            {
                logger.LogError($"{ex}");
            }
            await Task.CompletedTask;
        }

        private async Task ConnectIoTEdgeDeviceClient(CancellationToken stopToken)
        {
            upstreamClient = factory.CreateMqttClient();
            

            logger.LogInformation($"HostName: {upstreamClientCredentials.HostName} : SasKey: {upstreamClientCredentials.SaSKey}");
            

            var sig = EdgeTools.GenerateSasToken(upstreamClientCredentials.HostName, 
                upstreamClientCredentials.DeviceId,
                upstreamClientCredentials.SaSKey,
                TimeSpan.FromDays(365*1000));
            
            logger.LogInformation($"Device {upstreamClientCredentials.DeviceId} Shared Sig : {sig}");

            var upstreamOptions = new MqttClientOptionsBuilder()
                .WithTcpServer(upstreamClientCredentials.HostName, 8883)
                .WithClientId(upstreamClientCredentials.DeviceId)
                .WithCredentials(upstreamClientCredentials.MqttDeviceUserName, sig)
                .WithProtocolVersion(MqttProtocolVersion.V311)
                .WithTls( new MqttClientOptionsBuilderTlsParameters() { UseTls = true })
                .Build();

            try 
            {
                logger.LogInformation($"Connecting upstream client {device.IothubDeviceId}");

                upstreamClient.UseConnectedHandler( async e => {
                    logger.LogInformation($"Device {upstreamClientCredentials.DeviceId} connected");
                    // Subscribe to cloud to device messages
                    var topic = $"devices/{upstreamClientCredentials.DeviceId}/messages/devicebound/#";
                    await upstreamClient.SubscribeAsync(new TopicFilterBuilder().WithTopic(topic).Build());

                    // Initialise twin handling
                    //TwinStateLifecycle twinState = new TwinStateLifecycle(clock, upstreamClient);
                    
                    await Task.CompletedTask;
                });

                upstreamClient.UseDisconnectedHandler( async e => {
                    logger.LogInformation($"Device {upstreamClientCredentials.DeviceId} disconnected");
                    await Task.CompletedTask;
                });
                
                upstreamClient.UseApplicationMessageReceivedHandler( async e => {
                    await iotEdgeClient.HandleMessage(e, stopToken);

                    

                } );

                await upstreamClient.ConnectAsync(upstreamOptions, stopToken);
            } catch (Exception ex) 
            {
                logger.LogError($"{ex}");
            }
        }
        public async Task Connect(CancellationToken stopToken) 
        {
            await ConnectIoTEdgeDeviceClient(stopToken);

            await ConnectionMqttDeviceClient(stopToken);
            //await upstreamClient.ConnectAsync(upstreamOptions, stopToken);     
            //await downstreamClient.ConnectAsync(upstreamOptions, stopToken);     

        }

        public async Task PublishMessage(IMqttClient client, string topic, string payload, IDictionary<string, string> propertyBag, CancellationToken stopToken)
        {
            var t = $"{topic.TrimEnd('/')}/";

            if (propertyBag != null)
            {
                var propertyUrlString = UrlEncodedDictionarySerializer.Serialize(propertyBag);
            
                t = $"{topic.TrimEnd('/')}/{propertyUrlString}";
            }

            logger.LogInformation($"SendMessage: Topic {topic} with Payload {payload}");
            var msg = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(payload)
                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce) // qos = 1
                .Build();

                await client.PublishAsync(msg, stopToken);
        }


        public async Task SubscribeUpstream(string topic, CancellationToken stopToken)
        {
            await upstreamClient.SubscribeAsync(new TopicFilterBuilder().WithTopic(topic).Build());
        }

        public async Task SendUpstreamMessage(string topic, string payload, IDictionary<string, string> propertyBag, CancellationToken stopToken)
        {

            var propertyUrlString = "";

            if (propertyBag != null)
            {
                propertyUrlString = UrlEncodedDictionarySerializer.Serialize(propertyBag);
            }

            var t = topic.Trim('/') + "/" + propertyUrlString;

            logger.LogInformation($"SendMessage: Topic {t} with Payload {payload}");
            var msg = new MqttApplicationMessageBuilder()
                .WithTopic(t)
                .WithPayload(payload)
                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce) // qos = 1
                .Build();

            await upstreamClient.PublishAsync(msg, stopToken);
        }

        public async Task PublishLeafDeviceMessage(LocalDeviceMqttPublicationCategory category, string topicName, string payload, IDictionary<string, string> propertyBag, CancellationToken stopToken)
        {

            var propertyUrlString = "";

            if (propertyBag != null)
            {
                propertyUrlString = UrlEncodedDictionarySerializer.Serialize(propertyBag);
            }


            if (category == LocalDeviceMqttPublicationCategory.Twin)
            {
                // if topic name is null send to all topics

                foreach (var t in device.LocalDeviceMqttPublications.Twin)
                {
                    var t1 = t.Topic.Trim('/') + "/" + propertyUrlString;  // Ensure topic ends with a forward slash /                  

                    if (topicName == null)
                    {
                        // send to all topics
                        logger.LogInformation($"SendMessage: Topic {t1} with Payload {payload}");
                        var msg = new MqttApplicationMessageBuilder()
                            .WithTopic(t1)
                            .WithPayload(payload)
                            .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce) // qos = 1
                            .Build();

                        await downstreamClient.PublishAsync(msg, stopToken);

                    }
                    else if (topicName == t.Name)
                    {
                        logger.LogInformation($"SendMessage: Topic {t1} with Payload {payload}");
                        var msg = new MqttApplicationMessageBuilder()
                            .WithTopic(t1)
                            .WithPayload(payload)
                            .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce) // qos = 1
                            .Build();

                        await downstreamClient.PublishAsync(msg, stopToken);
                    }
 
                }

            }

            

            
        }

        public async Task SendCloudToDeviceMessage(string topicKey, string payload, CancellationToken stopToken)
        {
            MqttTopic topic = null;
            cloudToDevicePublishingTopics.TryGetValue(topicKey, out topic);

            if (topic != null)
            {

                logger.LogInformation($"Cloud to device message for mqtt device {downstreamClientCredentials.ClientId} on topic {topic.Topic}");

                await PublishMessage(downstreamClient, topic.Topic, payload, null, stopToken);
            }
            else
            {
                logger.LogError($"Unexpected topic key {topicKey} for mqtt device {downstreamClientCredentials.ClientId}");
            }
            await Task.CompletedTask;
        }

        public async Task SendUpstreamMessage(string payload, IDictionary<string, string> propertyBag, CancellationToken stopToken)
        {

            var propertyUrlString = "";
            
            if (propertyBag != null)
            {
                propertyUrlString = UrlEncodedDictionarySerializer.Serialize(propertyBag);
            }
            var topic = $"devices/{upstreamClientCredentials.DeviceId}/messages/events/{propertyUrlString}";

            logger.LogInformation($"SendMessage: Topic {topic} with Payload {payload}");
            var msg = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(payload)
                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce) // qos = 1
                .Build();

                await upstreamClient.PublishAsync(msg, stopToken);
        }

        public LocalDeviceSubscriptionCategory GetLocalDeviceSubscriptionCategoryForTopic(string topic)
        {
            // Look through each of the dictionaries
            // Search deviceToCloudTopics
            foreach (var t in device.LocalDeviceMqttSubscriptions.DeviceToCloudTopics)
            {
                var t1 = t.Topic.TrimEnd('/') + "/"; // Ensure topic ends with a forward slash /
                var t2 = topic.TrimEnd('/') + "/"; 
                if (t1 == t2)
                {
                    return LocalDeviceSubscriptionCategory.DeviceToCloudTopics;
                }
            }

            // Search twinRequestTopics

            foreach ( var t in device.LocalDeviceMqttSubscriptions.TwinRequestTopics)
            {
                var t1 = t.Topic.TrimEnd('/') + "/"; // Ensure topic ends with a forward slash /
                var t2 = topic.TrimEnd('/') + "/";
                if (t1 == t2)
                {
                    return LocalDeviceSubscriptionCategory.TwinRequestTopics;
                }
            }
            return LocalDeviceSubscriptionCategory.Unknown;
        }

    }

}