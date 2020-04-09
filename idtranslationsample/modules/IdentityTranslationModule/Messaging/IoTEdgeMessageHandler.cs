using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using IdentityTranslationModule.Controller;
using Microsoft.Azure.Devices.Client.Common;
using Microsoft.Extensions.Logging;

namespace IdentityTranslationModule.Messaging
{

    public class IoTEdgeMessageHandler : MessageHandler
    {

        private const string c2dTopicPattern = @"devices/(?<deviceId>.+)/messages/devicebound/(?<propertyBag>.*)";
        private static readonly TimeSpan regexTimeoutMilliseconds = TimeSpan.FromMilliseconds(500);
        private Regex c2dTopicRegex = new Regex(c2dTopicPattern, RegexOptions.Compiled, regexTimeoutMilliseconds);

        private readonly IDeviceIotEdgeController controller; 
        public IoTEdgeMessageHandler(IDeviceIotEdgeController controller, ILogger<IoTEdgeMessageHandler> logger) : base(logger)
        {
            this.controller = controller;
        }

        public override async Task Run(MessageContext context, CancellationToken stopToken)
        {
            logger.LogInformation("To Do: Translate IoT Edge messages into controller calls");
            var e = context.MqttMessage;
           

            var topic = e.ApplicationMessage.Topic;

            logger.LogInformation($"Matching topic: {topic}");

            // determine the message type based on the responded to topic
            var match = this.c2dTopicRegex.Match(topic);
            if (match.Success)
            {
                var d = match.Groups[1].Value;
                var pb = match.Groups[2].Value;


                logger.LogInformation($"Found c2d");

                var properties = UrlEncodedDictionarySerializer.Deserialize( pb, 0);
                
                var result = controller.CloudToDeviceMessage(e.ApplicationMessage, properties);
                context.Result = result;
            }

            await Task.CompletedTask;
        }
    }
}
