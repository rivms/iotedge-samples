using IdentityTranslationModule.Connection;
using Microsoft.Extensions.Logging;
using MQTTnet;

namespace IdentityTranslationModule.Controller
{


    public interface IDeviceMqttController 
    {
        MqttActionResult DeviceToCloudMessage(MqttApplicationMessage message);
        MqttActionResult TwinRequest(TwinStateLifecycle lc, MqttApplicationMessage message);
    }
    public class DeviceMqttController : IDeviceMqttController
    {
        private ILogger logger; 
        public DeviceMqttController(ILogger<DeviceMqttController> logger)
        {
            this.logger = logger;
        }

        public MqttActionResult DeviceToCloudMessage(MqttApplicationMessage message)
        {

            var msg = System.Text.Encoding.UTF8.GetString(message.Payload, 0, message.Payload.Length);
            logger.LogInformation($"Controller: Device to cloud mesage found, message is: {msg}");
            var result = new DeviceToCloudResult(ControllerConstants.IoTEdgeD2C, msg, null);
            return result;
        }

        public MqttActionResult TwinRequest(TwinStateLifecycle lc, MqttApplicationMessage message)
        {

            // Controller needs a request context    
            var r = lc.RetrieveDeviceTwinProperties();

            var msg = System.Text.Encoding.UTF8.GetString(message.Payload, 0, message.Payload.Length);
            logger.LogInformation($"Handling twin request with message: {msg}");
            return r;
        }

    }
}

