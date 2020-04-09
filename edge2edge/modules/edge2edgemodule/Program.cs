namespace edge2edgemodule
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Runtime.Loader;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Devices.Client;
    using Microsoft.Azure.Devices.Client.Transport.Mqtt;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    class Program
    {
        static int counter;
        //private static DeviceClient ParentDeviceClient;

        static void Main(string[] args)
        {

            Console.WriteLine("Creating HostBuilder");
            var tBuild = CreateHostBuilder(args).UseConsoleLifetime().Build().RunAsync();

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
                    services.AddHostedService<Edge2EdgeModuleService>();
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

        /// <summary>
        /// Initializes the ModuleClient and sets up the callback to receive
        /// messages containing temperature information
        /// </summary>
        //static async Task Init()
        //{
        //    MqttTransportSettings mqttSetting = new MqttTransportSettings(TransportType.Mqtt_Tcp_Only);
        //    ITransportSettings[] settings = { mqttSetting };

            // Open a connection to the Edge runtime
        //    ModuleClient ioTHubModuleClient = await ModuleClient.CreateFromEnvironmentAsync(settings);
        //    await ioTHubModuleClient.OpenAsync();
        //    Console.WriteLine("IoT Hub module client initialized.");
        //    var twin = await ioTHubModuleClient.GetTwinAsync();

        //    var twinJson = twin.Properties.Desired.ToJson();

        //    Console.WriteLine($"Twin properties found:\n{twinJson}");

        //    var parentEdge = twin.Properties.Desired["parentEdge"];

        //    Console.WriteLine($"edge2edgeKey is: {parentEdge.edge2edgeKey}");

            
        //    string deviceConnectionString = parentEdge.parentEdgeConnectionString;
        //    Console.WriteLine($"Connecting to parent device with connection string {deviceConnectionString}");

            //ParentDeviceClient = DeviceClient.CreateFromConnectionString(deviceConnectionString);

            //if (ParentDeviceClient == null)
            //{
            //    Console.WriteLine("Failed to create DeviceClient for Parent");
            //}

            // Register callback to be called when a message is received by the module
            //await ioTHubModuleClient.SetInputMessageHandlerAsync("input1", PipeMessage, ioTHubModuleClient);
        //}

        /// <summary>
        /// This method is called whenever the module is sent a message from the EdgeHub. 
        /// It just pipe the messages without any change.
        /// It prints all the incoming messages.
        /// </summary>
        static async Task<MessageResponse> PipeMessage(Message message, object userContext)
        {

            // Echo message to parent IoT Edge
            // Add a property to indicate this is a hierarchical message
            int counterValue = Interlocked.Increment(ref counter);

            var moduleClient = userContext as ModuleClient;
            if (moduleClient == null)
            {
                throw new InvalidOperationException("UserContext doesn't contain " + "expected values");
            }

            byte[] messageBytes = message.GetBytes();
            string messageString = Encoding.UTF8.GetString(messageBytes);
            Console.WriteLine($"Received message: {counterValue}, Body: [{messageString}]");

            if (!string.IsNullOrEmpty(messageString))
            {
                using (var pipeMessage = new Message(messageBytes))
                {
                    foreach (var prop in message.Properties)
                    {
                        pipeMessage.Properties.Add(prop.Key, prop.Value);
                    }
                    await moduleClient.SendEventAsync("output1", pipeMessage);
                
                    Console.WriteLine("Received message sent");

                    //if (ParentDeviceClient != null) {
                    //    pipeMessage.Properties.Add("ChildEdgeDevice", "true");
                    //    await ParentDeviceClient.SendEventAsync(pipeMessage);
                    //}
                    
                }
            }
            return MessageResponse.Completed;
        }
    }
}
