namespace IdentityTranslationModule.Controller
{

    public class DirectMethodResult : MqttResult 
    {
        public DirectMethodResult(string topicName, string payload) : base(topicName, payload)
        {
        }
    }
}

