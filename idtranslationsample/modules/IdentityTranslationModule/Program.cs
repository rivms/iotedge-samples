using System;
using System.Runtime.Loader;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace IdentityTranslationModule
{
    class Program
    {

        static void Main(string[] args)
        {

            Console.WriteLine("Creating HostBuilderZZZ");
            var host = CreateHostBuilder(args).UseConsoleLifetime().Build();

             var logger = host.Services.GetRequiredService<ILogger<Program>>();
            var service = host.Services.GetService<IdentityTranslationService>();

            if (service ==  null) {
                logger.LogError($"Null service");
            }
            else
            { 
                // Pass IHost to the identity translation service
                // Can't use Wrapper service as its transient 
                service.Host = host;
                logger.LogInformation("Singleton found!");
            }

            logger.LogInformation("-------------------------===============*****************");
            //service.Host = host;
            // logger.LogInformation("HostBuilder created.");
            var tBuild = host.RunAsync();

            tBuild.Wait();
            Console.WriteLine("Built HostBuilder");
            //var tInit = Init();

            
            
            //Task.WaitAll(new Task[] {tBuild, tInit});

            // Wait until the app unloads or is cancelled
            var cts = new CancellationTokenSource();
            AssemblyLoadContext.Default.Unloading += (ctx) => cts.Cancel();
            Console.CancelKeyPress += (sender, cpe) => cts.Cancel();
            WhenCancelled(cts.Token).Wait();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, config) => {
                    config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
                    config.AddEnvironmentVariables();
                    config.AddCommandLine(args);
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddSingleton<IdentityTranslationService>();
                    services.AddHostedService<IdentityTranslationBackgroundServiceWrapper>();
                })
                .ConfigureLogging((hostContext, configLogging) =>
                {
                    configLogging.AddConsole();
                    configLogging.AddDebug();
                });

        /// <summary>
        /// Handles cleanup operations when app is cancelled or unloads
        /// </summary>
        public static Task WhenCancelled(CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<bool>();
            cancellationToken.Register(s => ((TaskCompletionSource<bool>)s).SetResult(true), tcs);
            return tcs.Task;
        }
       
    }
}
