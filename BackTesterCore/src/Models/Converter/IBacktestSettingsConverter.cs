using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Backtesting.Models;
using Backtesting.Services;

namespace Backtesting.Models
{
    public class IBacktestSettingsJsonConverter : JsonConverter<IBacktestSettings>
    {
        public override IBacktestSettings Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException("Coversion from string to IBacktestSettings is not implimented");
        }

        public override void Write(Utf8JsonWriter writer, IBacktestSettings settings, JsonSerializerOptions options)
        {
            switch (settings.Strategy)
            {
                case Strategies.MACD_CROSS:
                    writer.WriteRawValue(JsonSerializer.Serialize((MacdBacktestOptions)settings));
                    break;
                default:
                    throw new NotImplementedException($"Coversion to string from {settings.Strategy} is not implimented");
            }
            
        }
    }
}