using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using IdentityTranslationModule.Messaging;

namespace IdentityTranslationModule.Controller
{
    public class MqttCompositeActionResult : MqttActionResult
    {
        private List<MqttActionResult> actions = new List<MqttActionResult>();

        public MqttCompositeActionResult()
        {
        }

        public void Add(MqttActionResult result)
        {
            actions.Add(result);
        }

        public override async Task ExecuteResultAsync(MessageContext context, CancellationToken stopToken)
        {
            foreach(var a in actions)
            {
                await a.ExecuteResultAsync(context, stopToken);
            }

            await Task.CompletedTask;
        }
    }
}
