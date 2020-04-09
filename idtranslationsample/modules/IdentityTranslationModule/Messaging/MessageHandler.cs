using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace IdentityTranslationModule.Messaging
{

    public abstract class MessageHandler
    {
        private MessageHandler next;
        protected ILogger logger;

        public MessageHandler(ILogger logger)
        {
            this.logger = logger;
        }

        public MessageHandler SetNext(MessageHandler nextHandler) 
        {
            var lastHandler = this;

            while (lastHandler.next != null) 
            {
                lastHandler = lastHandler.next;
            }

            lastHandler.next = nextHandler;

            return this;
        }

        public async Task HandleMessage(MessageContext context, CancellationToken stopToken) 
        {
            Console.WriteLine("Handling message");
            await Run(context, stopToken);
            if (next != null) {
                await next.HandleMessage(context, stopToken);
            }
        }

        public abstract Task Run(MessageContext context, CancellationToken stopToken);

    }
}

