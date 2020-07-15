using System.Threading.Tasks;

using NodaTime;

using IdentityTranslationModule.Controller;

namespace IdentityTranslationModule.Connection
{
    public class TwinStateLifecycle
    {
        public long LastRequestId { get; private set; }
        private bool isSubscribed;
        MqttDeviceClient upstreamClient;
        private IClock clock; 
        public TwinStateLifecycle(IClock clock, MqttDeviceClient upstreamClient)
        {
            this.clock = clock;
            //requestId = clock.GetCurrentInstant().ToUnixTimeMilliseconds();
            this.upstreamClient = upstreamClient;
            this.isSubscribed = false;
        }

        public TwinRequestResult RetrieveDeviceTwinProperties()
        {
            LastRequestId = clock.GetCurrentInstant().ToUnixTimeMilliseconds();
            var r = new TwinRequestResult(LastRequestId, !isSubscribed);
            return r;
        }

        private async Task SubscribeOperationResponse()
        {
            //await upstreamClient.
            await Task.CompletedTask;         
        }

        private async Task RequestTwinProperties()
        {
            await Task.CompletedTask;
        }

        private async Task HandleDeviceTwinResponse()
        {
            // Include transient error handling
            await Task.CompletedTask;
        }
    }
}
