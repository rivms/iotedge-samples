namespace IdentityTranslationModule.Controller
{

    public class MqttResult 
    {
        public string TopicName {get; private set;}
        //public byte[] Payload {get; private set;}

        public string Payload {get; private set;}
        public MqttResult(string topicName, string payload)
        {
            TopicName = topicName;
            Payload = payload;
        }
    }
}

