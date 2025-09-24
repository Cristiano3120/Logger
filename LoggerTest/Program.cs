using Cristiano3120.Logging;
using System.Reflection;
using System.Text.Json;

namespace LoggerTest;

internal class Program
{
    static void Main()
    {
        {
            LoggerSettings settings = new()
            {
                DeactivateFileLogging = true,
                LogLevel = LogLevel.Debug,
            };
            Logger logger = new(settings);

            logger.AddAttributeToFilter<FilterAtrribute>(static (prop, jsonNode) =>
            {
                FilterAtrribute? filterAtrribute = prop.GetCustomAttribute<FilterAtrribute>();
                _ = (jsonNode?[prop.Name] = filterAtrribute?.FilterSymbol);
            });

            JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions()
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            };

            string jsonContent = JsonSerializer.Serialize(12, jsonSerializerOptions);
            logger.LogHttpPayload<int>(LoggerParams.None, PayloadType.Sent, HttpRequestType.Post, jsonContent);
        }
    }
}
