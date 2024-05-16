using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TinyPng
{
    /// <summary>
    /// Pinched from https://stackoverflow.com/questions/59059989/system-text-json-how-do-i-specify-a-custom-name-for-an-enum-value
    /// This is because System.Text.Json doesn't support EnumMemberAttribute or a way to customise what an Enum value is serialised as.
    /// </summary>
    internal class CustomJsonStringEnumConverter(JsonNamingPolicy namingPolicy = null, bool allowIntegerValues = true) : JsonConverterFactory
    {
        private readonly JsonStringEnumConverter _baseConverter = new(namingPolicy, allowIntegerValues);

        public CustomJsonStringEnumConverter() : this(null, true) { }

        public override bool CanConvert(Type typeToConvert) => _baseConverter.CanConvert(typeToConvert);

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            var query = from field in typeToConvert.GetFields(BindingFlags.Public | BindingFlags.Static)
                        let attr = field.GetCustomAttribute<EnumMemberAttribute>()
                        where attr != null
                        select (field.Name, attr.Value);
            var dictionary = query.ToDictionary(p => p.Name, p => p.Value);
            if (dictionary.Count > 0)
            {
                return new JsonStringEnumConverter(new DictionaryLookupNamingPolicy(dictionary, namingPolicy), allowIntegerValues).CreateConverter(typeToConvert, options);
            }
            else
            {
                return _baseConverter.CreateConverter(typeToConvert, options);
            }
        }
    }

    public class JsonNamingPolicyDecorator(JsonNamingPolicy underlyingNamingPolicy) : JsonNamingPolicy
    {
        public override string ConvertName(string name) => underlyingNamingPolicy == null ? name : underlyingNamingPolicy.ConvertName(name);
    }

    internal class DictionaryLookupNamingPolicy(Dictionary<string, string> dictionary, JsonNamingPolicy underlyingNamingPolicy) : JsonNamingPolicyDecorator(underlyingNamingPolicy)
    {
        readonly Dictionary<string, string> _dictionary = dictionary ?? throw new ArgumentNullException();

        public override string ConvertName(string name) => _dictionary.TryGetValue(name, out var value) ? value : base.ConvertName(name);
    }
}
