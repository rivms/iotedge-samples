using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace IdentityTranslationModule
{

    class IdentityTranslationBackgroundServiceWrapper : BackgroundService
    {
        
        private readonly ILogger logger;
        private IdentityTranslationService service; 
        public IdentityTranslationBackgroundServiceWrapper(ILogger<IdentityTranslationBackgroundServiceWrapper> logger,
            IdentityTranslationService service, 
            IConfiguration config) 
        {
            this.logger = logger;
            this.service = service;
            logger.LogInformation("IdentityTranslationBackgroundServiceWrapper created");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {

            try 
            {
                logger.LogInformation("ExecuteAsync started");

                stoppingToken.Register( () =>
                {
                    logger.LogInformation("BotWorker stopping");
                });

                await service.StartAsync(stoppingToken);

            } catch (Exception ex)
            {
                logger.LogCritical($"Background service exited with exception: {ex}");
            }

        }

    }
}
