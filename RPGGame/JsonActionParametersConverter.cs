using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RPGGame
{
    internal class JsonActionParametersConverter : JsonConverter<Dictionary<string, object?>>
    {
        private readonly struct ObjectWrapper(object? obj)
        {
            public readonly Type? ObjectType = obj?.GetType();
            public readonly object? Object = obj;
        }

        public override void WriteJson(JsonWriter writer, Dictionary<string, object?>? value, JsonSerializer serializer)
        {
            Dictionary<string, ObjectWrapper>? wrapperDict = value?.ToDictionary(v => v.Key, v => new ObjectWrapper(v.Value));

            if (wrapperDict is not null)
            {
                JObject.FromObject(wrapperDict).WriteTo(writer);
            }
        }

        public override Dictionary<string, object?> ReadJson(JsonReader reader, Type objectType, Dictionary<string, object?>? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            Dictionary<string, object?> parameters = new();
            JObject jObj = JObject.Load(reader);

            foreach (JProperty param in jObj.Properties())
            {
                string? typeName = param.Value[nameof(ObjectWrapper.ObjectType)]?.Value<string?>();
                if (typeName is null)
                {
                    parameters[param.Name] = null;
                    continue;
                }

                Type? paramType = Type.GetType(typeName);
                JValue? objValue = param.Value[nameof(ObjectWrapper.Object)]?.Value<JValue>();
                object? obj = objValue?.Value;
                // If the value is a string, convert it to whatever the original type was - otherwise keep the primitive type
                parameters[param.Name] = paramType is null || obj is null || objValue is null
                    ? null
                    : objValue.Type == JTokenType.String
                        ? TypeDescriptor.GetConverter(paramType).ConvertFrom(null, CultureInfo.CurrentCulture, (string)obj)
                        : obj;
            }

            return parameters;
        }
    }
}
