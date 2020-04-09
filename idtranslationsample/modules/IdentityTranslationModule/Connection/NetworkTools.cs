using System;
using System.IO;
using Newtonsoft.Json;

namespace IdentityTranslationModule.Connection
{

    public class NetworkTools
    {
        public static string DownloadTextFile(Uri fileUri) 
        {
            var net = new System.Net.WebClient();
            var data = net.DownloadData(fileUri);
            //var content = new System.IO.MemoryStream(data);
            return  System.Text.Encoding.UTF8.GetString(data);
        }

        public static T DeserializeJson<T>(string json)
        {
            Newtonsoft.Json.JsonSerializer s = new JsonSerializer();
            return s.Deserialize<T>(new JsonTextReader(new StringReader(json)));
        }
    }
}
