using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

namespace IdentityTranslationModule.Messaging
{

    public class MqttNetMessageHandler : MessageHandler
    {
        public MqttNetMessageHandler(ILogger<MqttNetMessageHandler> logger): base(logger)
        {
        }

        public override async Task Run(MessageContext context, CancellationToken stopToken)
        {
            logger.LogInformation($"Handling message");

            var e = context.MqttMessage;
            // Extract properties
                    // Incoming topic is: devices/<device id>/messages/devicebound/<properties>
                    

                    logger.LogInformation($"{context.Client.upstreamClientCredentials.DeviceId} ### RECEIVED APPLICATION MESSAGE ###");
                    logger.LogInformation($"+ Topic = {e.ApplicationMessage.Topic}");
                    logger.LogInformation($"+ Payload = {Encoding.UTF8.GetString(e.ApplicationMessage.Payload)}");
                    logger.LogInformation($"+ QoS = {e.ApplicationMessage.QualityOfServiceLevel}");
                    logger.LogInformation($"+ Retain = {e.ApplicationMessage.Retain}");
                    logger.LogInformation("###---- End Application Message ###");

            if (context.Direction == MessageDirection.UpstreamToDownstream)
            {
                var msg = new DownStreamMessage(context.MqttMessage.ApplicationMessage.Topic, context.MqttMessage.ApplicationMessage.Payload);
                context.Message = msg;
            }
            await Task.CompletedTask;
        }
    }
}

