using System.Text.Json;

namespace SimpleObjectComparer.Tests.Helpers
{
    internal class Helper
    {
        public static T? GetModel<T>(string jsonFileName)
        {
            string content = File.ReadAllText(Path.Combine("Json", jsonFileName.ToLower().Replace(".json", "")+".json"));
            
            return JsonSerializer.Deserialize<T>(content);
        }
    }
}
