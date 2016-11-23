using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;

namespace NServiceBusTutorials.FileSystemTransport.Transport
{
    internal static class HeaderSerializer
    {
        public static string Serialize(Dictionary<string, string> headers)
        {
            var serializer = BuildSerializer();
            using (var stream = new MemoryStream())
            {
                serializer.WriteObject(stream, headers);
                return Encoding.UTF8.GetString(stream.ToArray());
            }
        }

        public static Dictionary<string, string> DeSerialize(string headerJson)
        {
            var serializer = BuildSerializer();
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(headerJson)))
            {
                return (Dictionary<string, string>) serializer.ReadObject(stream);
            }
        }

        private static DataContractJsonSerializer BuildSerializer()
        {
            var settings = new DataContractJsonSerializerSettings
            {
                UseSimpleDictionaryFormat = true,
            };

            return new DataContractJsonSerializer(typeof(Dictionary<string, string>), settings);
        }
    }
}
