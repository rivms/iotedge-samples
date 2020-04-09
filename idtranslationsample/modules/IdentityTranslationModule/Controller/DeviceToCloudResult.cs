using System.Collections.Generic;

namespace IdentityTranslationModule.Controller
{

    public class DeviceToCloudResult : MqttResult 
    {
        public IDictionary<string, string> PropertyBag {get; private set;}
        public DeviceToCloudResult(string topicName, string payload, IDictionary<string, string> propertyBag) : base(topicName, payload)
        {
            this.PropertyBag = propertyBag;
        }
    }
}

