using Microsoft.Extensions.Logging;
using MQTTnet;

namespace IdentityTranslationModule.Controller
{


    public interface IDeviceMqttController 
    {
        MqttResult DeviceToCloudMessage(MqttApplicationMessage message);
    }
    public class DeviceMqttController : IDeviceMqttController
    {
        private ILogger logger; 
        public DeviceMqttController(ILogger<DeviceMqttController> logger)
        {
            this.logger = logger;
        }

        public MqttResult DeviceToCloudMessage(MqttApplicationMessage message)
        {

            var msg = System.Text.Encoding.UTF8.GetString(message.Payload, 0, message.Payload.Length);
            logger.LogInformation($"Controller: Device to cloud mesage found, message is: {msg}");
            var result = new DeviceToCloudResult(ControllerConstants.IoTEdgeD2C, msg, null);
            return result;
        }

    }
}

