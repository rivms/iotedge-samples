using IdentityTranslationModule.Connection;
using IdentityTranslationModule.Controller;
using MQTTnet;

namespace IdentityTranslationModule.Messaging
{

    public class MessageContext
    {
        public BaseMessage Message { get; set;}

        public MessageDirection Direction { get; private set; }
        public MqttApplicationMessageReceivedEventArgs MqttMessage { get; private set;}

        public CompositeDeviceClient Client { get; private set; }    

        public MqttResult Result {get; set;}

        public MessageContext(CompositeDeviceClient client, 
            MqttApplicationMessageReceivedEventArgs eventArgs, 
            MessageDirection direction) 
        {
            this.Message = null;
            this.MqttMessage = eventArgs;
            this.Client = client; 
            this.Direction = direction;
        }
    }
}

