using IdentityTranslationModule.Messaging;
using System.Threading;
using System.Threading.Tasks;

namespace IdentityTranslationModule.Controller
{
    public class SubscribeToTopicResult : MqttActionResult
    {
        private string topic;
        public SubscribeToTopicResult(string topic)
        {
            this.topic = topic;
        }
        public override async Task ExecuteResultAsync(MessageContext context, CancellationToken stopToken)
        {
            await context.Client.SubscribeUpstream(topic, stopToken);
        }
    }
}
