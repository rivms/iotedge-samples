using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using MQTTnet;

using IdentityTranslationModule.Messaging;

namespace IdentityTranslationModule.Connection
{
    public class MqttDeviceClient
    {
        private ILogger logger; 
        private CompositeDeviceClient compositeClient;
        private Messaging.MessageHandler messageHandler; 
        public MqttDeviceClient(CompositeDeviceClient client, 
            Messaging.MessageHandler messageHandler,
            ILogger<MqttDeviceClient> logger) 
        {
            this.logger = logger; 
            this.compositeClient = client;
            this.messageHandler = messageHandler;
        }

        public async Task MessageReceivedHandler(MqttApplicationMessageReceivedEventArgs e, CancellationToken stopToken) 
        {
            logger.LogInformation($"Local device message received: {e.ApplicationMessage.Topic}");

            var mCtxt = new MessageContext(compositeClient, e, MessageDirection.DownstreamToUpstream);

            logger.LogInformation($"*** Begin handling MqttDeviceClient message");

            await messageHandler.HandleMessage(mCtxt, stopToken);

            if (mCtxt.Result != null) 
            {
                //await ProcessResult(mCtxt.Result, stopToken);
                await mCtxt.Result.ExecuteResultAsync(mCtxt, stopToken);
            }       
            else 
            {
                logger.LogError("Unexpected MqttResult value, null ignored");
            }

            
            await Task.CompletedTask;
        }


        /*

        private async Task ProcessResult(PublishToTopicResult result, CancellationToken stopToken) 
        {
            switch (result) 
            {
                case DeviceToCloudResult d2c: 
                {
                    logger.LogInformation("Processing DeviceToCloudResult");
                    await compositeClient.SendUpstreamMessage(result.Payload, d2c.PropertyBag, stopToken);
                    break;
                }
                case TwinRequestResult twin:
                {

                }
                case NoResult nr:
                {

                    logger.LogInformation("Warning NoResult returned");
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
        */
    }


}