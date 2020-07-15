using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Azure.Devices.Client.Common;
using Microsoft.Extensions.Logging;

using IdentityTranslationModule.Controller;

namespace IdentityTranslationModule.Messaging
{
    public class MqttDeviceMessageHandler : MessageHandler
    {
        private const string c2dTopicPattern = @"devices/(?<deviceId>.+)/messages/devicebound/(?<propertyBag>.*)";
        private static readonly TimeSpan regexTimeoutMilliseconds = TimeSpan.FromMilliseconds(500);
        private Regex c2dTopicRegex = new Regex(c2dTopicPattern, RegexOptions.Compiled, regexTimeoutMilliseconds);

        private readonly IDeviceMqttController controller; 
        public MqttDeviceMessageHandler(IDeviceMqttController controller, ILogger<MqttDeviceMessageHandler> logger) : base(logger)
        {
            this.controller = controller;
        }

        public override async Task Run(MessageContext context, CancellationToken stopToken)
        {
            logger.LogInformation("To Do: Translate IoT Edge messages into controller calls");
            var e = context.MqttMessage;
           
            var topic = e.ApplicationMessage.Topic;
            logger.LogInformation($"Matching topic: {topic}");

            if (context.Client.GetLocalDeviceSubscriptionCategoryForTopic(topic) == Connection.LocalDeviceSubscriptionCategory.TwinRequestTopics)
            {
                logger.LogInformation($"Topic {topic} matches category {Connection.LocalDeviceSubscriptionCategory.TwinRequestTopics}");

                // Ask controller to handle twin request
                var result = controller.TwinRequest(context.Client.TwinStateLifecycle, e.ApplicationMessage);

                context.Result = result;
            }
            else
            {
                logger.LogInformation($"Topic {topic} not matched to any category");
                var result = controller.DeviceToCloudMessage(e.ApplicationMessage);

                context.Result = result;
            }

            // determine the message type based on the responded to topic
            var match = this.c2dTopicRegex.Match(topic);
            if (match.Success)
            {
                var d = match.Groups[1].Value;
                var pb = match.Groups[2].Value;

                logger.LogInformation($"Found c2d");

                var properties = UrlEncodedDictionarySerializer.Deserialize( pb, 0);
            }

            await Task.CompletedTask;
        }

        public void GetTopicCategory(string topic)
        {
        }
    }
}
