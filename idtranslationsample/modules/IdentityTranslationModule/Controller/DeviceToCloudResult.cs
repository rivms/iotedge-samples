using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using IdentityTranslationModule.Messaging;

namespace IdentityTranslationModule.Controller
{

    public class DeviceToCloudResult : MqttActionResult 
    {
        public IDictionary<string, string> PropertyBag {get; private set;}
        public string Payload { get; private set; }
        public DeviceToCloudResult(string topicName, string payload, IDictionary<string, string> propertyBag)
        {
            this.PropertyBag = propertyBag;
            this.Payload = payload;
        }


        public override async Task ExecuteResultAsync(MessageContext context, CancellationToken stopToken)
        {
            await context.Client.SendUpstreamMessage(Payload, PropertyBag, stopToken);
        }
    }
}

