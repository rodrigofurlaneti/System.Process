using System.IO;
using System.Text.Json;

namespace System.Process.IntegrationTests.Common
{
    public class ConvertJson
    {
        public static T ReadJson<T>(string file)
        {
            using (StreamReader reader = new StreamReader($"../../../Inputs/{file}"))
            {
                var json = reader.ReadToEnd();
                return JsonSerializer.Deserialize<T>(json);
            }
        }
    }
}
