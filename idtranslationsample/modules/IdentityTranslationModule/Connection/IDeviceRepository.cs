using System.Collections.Generic;

namespace IdentityTranslationModule.Connection
{
    public interface IDeviceRepository
    {
        IEnumerable<CompositeDeviceConfiguration.Device> AllDevices();
    }
}

