using System.Threading;
using System.Threading.Tasks;

using IdentityTranslationModule.Connection;
using IdentityTranslationModule.Messaging;

namespace IdentityTranslationModule.Controller
{
    public class PublishToLocalDeviceTopicResult : MqttActionResult
    {
        public LocalDeviceMqttPublicationCategory Category { get; private set; }
        public string Payload { get; private set; }

        public PublishToLocalDeviceTopicResult(LocalDeviceMqttPublicationCategory category, string payload)
        {
            Category = category;
            Payload = payload;
        }
        public async override Task ExecuteResultAsync(MessageContext context, CancellationToken stopToken)
        {
            await context.Client.PublishLeafDeviceMessage(Category, null, Payload, null, stopToken);
            //switch (Category)
            //{
            //    case LocalDeviceMqttPublicationCategory.CloudToDevice:
            //    {
            //        break;
            //    }
            //    case LocalDeviceMqttPublicationCategory.DirectMethod:
            //    {
            //        break;
            //    }
            //    case LocalDeviceMqttPublicationCategory.Twin:
            //    {
            //        context.Client.PublishMessage()
            //        break;
            //    }
            //    default {
            //            break;
            //    }
            //}
            await Task.CompletedTask;
        }
    }
}
