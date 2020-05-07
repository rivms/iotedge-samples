using IdentityTranslationModule.Messaging;
using System.Threading;
using System.Threading.Tasks;

namespace IdentityTranslationModule.Controller
{
    public abstract class MqttActionResult
    {
        public async virtual Task ExecuteResultAsync(MessageContext context, CancellationToken stopToken)
        {
            await Task.CompletedTask;
        }
    }
}
