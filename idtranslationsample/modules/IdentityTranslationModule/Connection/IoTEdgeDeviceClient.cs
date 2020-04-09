using System.Threading;
using System.Threading.Tasks;
using IdentityTranslationModule.Controller;
using IdentityTranslationModule.Messaging;
using Microsoft.Extensions.Logging;
using MQTTnet;

namespace IdentityTranslationModule.Connection
{
    public class IotEdgeDeviceClient
    {
        private ILogger logger;
        private CompositeDeviceClient compositeClient;
        private Messaging.MessageHandler messageHandler;
        public IotEdgeDeviceClient(CompositeDeviceClient client,
            Messaging.MessageHandler messageHandler,
            ILogger<IotEdgeDeviceClient> logger)
        {
            this.logger = logger;
            this.compositeClient = client;
            this.messageHandler = messageHandler;
        }

        public async Task HandleMessage(MqttApplicationMessageReceivedEventArgs e, CancellationToken stopToken)
        {
            logger.LogInformation($"IotEdgeDeviceClient device message received: {e.ApplicationMessage.Topic}");

            var mCtxt = new MessageContext(compositeClient, e, MessageDirection.UpstreamToDownstream);

            logger.LogInformation($"*** Begin handling IotEdgeDeviceClient message");

            await messageHandler.HandleMessage(mCtxt, stopToken);

            if (mCtxt.Result != null)
            {
                await ProcessResult(mCtxt.Result, stopToken);
            }
            else
            {
                logger.LogError("Unexpected MqttResult value, null ignored");
            }


            await Task.CompletedTask;
        }

        private async Task ProcessResult(MqttResult result, CancellationToken stopToken)
        {
            switch (result)
            {
                case CloudToDeviceResult c2d:
                    {
                        logger.LogInformation("Processing DeviceToCloudResult");
                        //await compositeClient.SendDow(result.Payload, d2c.PropertyBag, stopToken);
                        await compositeClient.SendCloudToDeviceMessage(result.TopicName, result.Payload, stopToken);
                        break;
                    }
                default:
                    {
                        logger.LogError($"Unexpected MqttResult type {result.GetType()}");
                        break;
                    }

            }
            await Task.CompletedTask;
        }

    }


}