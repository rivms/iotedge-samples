namespace IdentityTranslationModule.Messaging
{
    public class DownStreamMessage : BaseMessage
    {
        public DownStreamMessage(string topic, byte[] payload) : base(MessageDirection.UpstreamToDownstream, topic, payload)
        {
        }
    }
}

