using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Azure.Devices.Client.Common;
using Microsoft.Extensions.Logging;

using IdentityTranslationModule.Controller;

namespace IdentityTranslationModule.Messaging
{
    public class IoTEdgeMessageHandler : MessageHandler
    {
        private const string c2dTopicPattern = @"devices/(?<deviceId>.+)/messages/devicebound/(?<propertyBag>.*)";
        private const string twinResponseTopicPattern = @"\$iothub/twin/res/(\d+)/(\?.+)";
        
        private static readonly TimeSpan regexTimeoutMilliseconds = TimeSpan.FromMilliseconds(500);
        private Regex c2dTopicRegex = new Regex(c2dTopicPattern, RegexOptions.Compiled, regexTimeoutMilliseconds);
        private Regex twinResponseTopicRegex = new Regex(twinResponseTopicPattern, RegexOptions.Compiled, regexTimeoutMilliseconds);

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

            // Separate twin messages from 

            // determine the message type based on the responded to topic
            var c2dMatch = this.c2dTopicRegex.Match(topic);
            var twinResponseMatch = twinResponseTopicRegex.Match(topic);
            if (c2dMatch.Success)
            {
                var d = c2dMatch.Groups[1].Value;
                var pb = c2dMatch.Groups[2].Value;


                logger.LogInformation($"Found c2d");

                var properties = UrlEncodedDictionarySerializer.Deserialize(pb, 0);
                
                var result = controller.CloudToDeviceMessage(e.ApplicationMessage, properties);
                context.Result = result;
            }
            else if (twinResponseMatch.Success)
            {
                var responseCode = twinResponseMatch.Groups[1].Value;
                var ridProperty = twinResponseMatch.Groups[2].Value;

                var rid = ridProperty.Split('=')[1].TrimEnd('/');
                logger.LogInformation($"Twin response received with status {responseCode} and rid property {ridProperty}");
                
                var result = controller.DeviceTwin(e.ApplicationMessage, responseCode, rid, context.Client.TwinStateLifecycle.LastRequestId);
                context.Result = result;
            }

            await Task.CompletedTask;
        }
    }
}
