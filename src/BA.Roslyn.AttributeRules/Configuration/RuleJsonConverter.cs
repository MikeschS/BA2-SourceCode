using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BA.Roslyn.AttributeRules.Configuration
{
    internal class RuleJsonConverter : JsonConverter<AttributeRuleBaseConfig>
    {
        public override bool CanConvert(Type typeToConvert) =>
            typeof(AttributeRuleBaseConfig).IsAssignableFrom(typeToConvert);

        public override AttributeRuleBaseConfig? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            Utf8JsonReader readerClone = reader;

            if (readerClone.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException();
            }

            readerClone.Read();
            if (readerClone.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException();
            }

            string? propertyName = readerClone.GetString();
            if (propertyName != "Type")
            {
                throw new JsonException();
            }

            readerClone.Read();
            if (readerClone.TokenType != JsonTokenType.String)
            {
                throw new JsonException();
            }

            if(!Enum.TryParse<RuleType>(readerClone.GetString(), out var type))
            {
                throw new JsonException();
            }

            AttributeRuleBaseConfig config = type switch
            {
                RuleType.BaseClass => JsonSerializer.Deserialize<BaseClassRuleConfig>(ref reader)!,
                _ => throw new JsonException()
            };
            return config;
        }

        public override void Write(Utf8JsonWriter writer, AttributeRuleBaseConfig value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }
}
