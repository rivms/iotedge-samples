using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using MQTTnet;

namespace IdentityTranslationModule.Controller
{
    public interface IDeviceIotEdgeController
    {
        void DirectMethod();
        MqttResult CloudToDeviceMessage(MqttApplicationMessage message, IDictionary<string, string> properties);
        void DeviceTwin();
        void DeviceTwinPatch();
    }
    public class DeviceIotEdgeController : IDeviceIotEdgeController
    {
        private ILogger logger; 
        public DeviceIotEdgeController(ILogger<DeviceIotEdgeController> logger)
        {
            this.logger = logger;
        }

        public MqttResult CloudToDeviceMessage(MqttApplicationMessage message, IDictionary<string, string> properties)
        {

            var msg = System.Text.Encoding.UTF8.GetString(message.Payload, 0, message.Payload.Length);
            logger.LogInformation($"Controller: Cloud to device mesage found, message is: {message.Payload}");

            foreach(var kv in properties)
            {
                logger.LogInformation($"Property: ({kv.Key}, {kv.Value}");
            }

            var result = new CloudToDeviceResult("c2d", msg);
            return result;
        }

        public void DeviceTwin()
        {
            throw new NotImplementedException();
        }

        public void DeviceTwinPatch()
        {
            throw new NotImplementedException();
        }

        public void DirectMethod()
        {
            throw new NotImplementedException();
        }
    }
}

