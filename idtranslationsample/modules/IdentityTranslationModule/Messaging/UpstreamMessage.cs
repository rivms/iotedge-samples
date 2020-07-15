namespace IdentityTranslationModule.Messaging
{
    public class UpstreamMessage : BaseMessage
    {
        public UpstreamMessage(string topic, byte[] payload) : base(MessageDirection.DownstreamToUpstream, topic, payload)
        {
        }
    }
}

