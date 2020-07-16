using System;
using System.Collections.Generic;

using Microsoft.Extensions.Logging;

using MQTTnet;

namespace IdentityTranslationModule.Controller
{
    public interface IDeviceIotEdgeController
    {
        void DirectMethod();
        MqttActionResult CloudToDeviceMessage(MqttApplicationMessage message, IDictionary<string, string> properties);
        MqttActionResult DeviceTwin(MqttApplicationMessage message, string statusCode, string rid, long lastRid);
        void DeviceTwinPatch();
    }
    public class DeviceIotEdgeController : IDeviceIotEdgeController
    {
        private ILogger logger;
        public DeviceIotEdgeController(ILogger<DeviceIotEdgeController> logger)
        {
            this.logger = logger;
        }

        public MqttActionResult CloudToDeviceMessage(MqttApplicationMessage message, IDictionary<string, string> properties)
        {

            var msg = System.Text.Encoding.UTF8.GetString(message.Payload, 0, message.Payload.Length);
            logger.LogInformation($"Controller: Cloud to device mesage found, message is: {message.Payload}");

            foreach (var kv in properties)
            {
                logger.LogInformation($"Property: ({kv.Key}, {kv.Value}");
            }

            var result = new CloudToDeviceResult("c2d", msg);
            return result;
        }

        public MqttActionResult DeviceTwin(MqttApplicationMessage message, string statusCode, string rid, long lastRid)
        {            
            var msg = System.Text.Encoding.UTF8.GetString(message.Payload, 0, message.Payload.Length);
            logger.LogInformation($"Handling device twin response with payload {msg}, statusCode {statusCode} and rd {rid}");

            long nRid ;
            if (!long.TryParse(rid, out nRid))
            {
                return new ErrorResult("BADRID", $"Rid received with invalid format: {rid}");
            }

            if (lastRid != nRid)
            {
                logger.LogInformation($"Stale twin response, lastRid {lastRid} and received Rid {nRid}");
                return new NoResult();
            }

            return new PublishToLocalDeviceTopicResult(Connection.LocalDeviceMqttPublicationCategory.Twin, msg);
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

