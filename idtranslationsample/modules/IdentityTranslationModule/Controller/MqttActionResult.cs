using System.Threading;
using System.Threading.Tasks;

using IdentityTranslationModule.Messaging;

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
