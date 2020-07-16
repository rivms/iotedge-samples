using System.Threading;
using System.Threading.Tasks;

using IdentityTranslationModule.Messaging;

namespace IdentityTranslationModule.Controller
{

    public class CloudToDeviceResult : MqttActionResult
    {
        public string TopicName {get; private set;}
        //public byte[] Payload {get; private set;}

        public string Payload {get; private set;}

        public CommunicationDirection Direction { get { return CommunicationDirection.ToIoTHub; } }
        public CloudToDeviceResult(string topicName, string payload)
        {
            TopicName = topicName;
            Payload = payload;
        }


        public override async Task ExecuteResultAsync(MessageContext context, CancellationToken stopToken)
        {       
            await context.Client.SendCloudToDeviceMessage(TopicName, Payload, stopToken);            
        }
    }
}

