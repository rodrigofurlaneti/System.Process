using System.IO;
using System.Text.Json;

namespace System.Process.Base.IntegrationTests
{
    public class ProcessIntegrationTestsConfiguration
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
