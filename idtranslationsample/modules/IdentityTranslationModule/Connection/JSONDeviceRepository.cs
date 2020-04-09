using System;
using System.Collections.Generic;

namespace IdentityTranslationModule.Connection
{

    public class JSONDeviceRepository : IDeviceRepository
    {

        private readonly CompositeDeviceConfiguration mapping; 
        public static IDeviceRepository CreateFromUrl(Uri uri) 
        {
            Console.WriteLine($"Downloading file: {uri}");
            var fileText = NetworkTools.DownloadTextFile(uri);
            Console.WriteLine($"File content: {fileText}");
            return JSONDeviceRepository.Create(fileText);
        }

        

        public static IDeviceRepository Create(string jsonDeviceCredentialMapping) 
        {
            var deviceIdList = NetworkTools.DeserializeJson<CompositeDeviceConfiguration>(jsonDeviceCredentialMapping);
            return new JSONDeviceRepository(deviceIdList);
        }

        public IEnumerable<CompositeDeviceConfiguration.Device> AllDevices()
        {
            foreach(var d in mapping.Devices) 
            {
                yield return d;
            }
        }

        protected JSONDeviceRepository(CompositeDeviceConfiguration mapping) 
        {
            this.mapping = mapping;
        }

    }
}
