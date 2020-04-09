namespace IdentityTranslationModule.Controller
{

    public class CloudToDeviceResult : MqttResult 
    {
        public CloudToDeviceResult(string topicName, string payload) : base(topicName, payload)
        {
        }
    }
}

