using System;
using System.Collections.ObjectModel;

namespace IdentityTranslationModule.Messaging
{

    public enum MessageDirection
    {
        UpstreamToDownstream = 1,
        DownstreamToUpstream = 2
    }

    public abstract class BaseMessage
    {
        private readonly MessageDirection direction;

        protected readonly string topic; 
        protected readonly ReadOnlyCollection<byte> payload;

        public BaseMessage(MessageDirection direction, string topic, byte[] payload)
        {
            this.direction = direction;
            this.topic = topic;
            this.payload = Array.AsReadOnly<byte>(payload); 
        }
    }
}

