using IdentityTranslationModule.Messaging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace IdentityTranslationModule.Controller
{
    class ErrorResult : MqttActionResult
    {
        public string ErrorCode { get; private set; }
        public string Description { get; private set; }
        public ErrorResult(string errorCode, string errorDescription)
        {

        }


        public override async Task ExecuteResultAsync(MessageContext context, CancellationToken stopToken)
        {
            // TO DO: Use Logger
            Console.WriteLine($"Error result with code {ErrorCode} and description {Description}");
            await Task.CompletedTask;
        }
    }


}
