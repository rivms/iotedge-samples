using System.Threading;
using System.Threading.Tasks;

using IdentityTranslationModule.Messaging;

namespace IdentityTranslationModule.Controller
{
    public class TwinRequestResult : MqttActionResult
    {
        public bool MustSubscribe { get; private set; }
        public long RID { get; private set; }
        
        public TwinRequestResult(long rid, bool mustSubscribe) //: base($"$iothub/twin/GET/?$rid={rid}", "")
        {
            this.MustSubscribe = mustSubscribe;
            this.RID = rid;
        }

        public override async Task ExecuteResultAsync(MessageContext context, CancellationToken stopToken)
        {
            var subscribeTopic = "$iothub/twin/res/#";
            var publishTopic = $"$iothub/twin/GET/?$rid={RID}";
            if (MustSubscribe)
            {
                await context.Client.SubscribeUpstream(subscribeTopic, stopToken);
            }

            await context.Client.SendUpstreamMessage(publishTopic, "", null, stopToken);
        }
    }
}
