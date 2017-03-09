using Confifu.Abstractions;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Confifu.Config.Json
{
    public class JsonConfigVariables : IConfigVariables
    {
        private readonly string jsonContent;
        private readonly Dictionary<string, string> dict = new Dictionary<string, string>();

        public JsonConfigVariables(string jsonContent)
        {
            if (jsonContent == null) throw new ArgumentNullException(nameof(jsonContent));
            this.jsonContent = jsonContent;

            Load();
        }

        private void Load()
        {
            var json = JObject.Parse(jsonContent);

            ReadJsonRec(json);
        }

        private void ReadJsonRec(JObject json, string prefix = "")
        {
            foreach (var prop in json.Properties())
            {
                if (prop.Value is JValue)
                {
                    dict[prefix + prop.Name] = prop.Value.Value<string>();
                }
                if (prop.Value is JObject)
                {
                    ReadJsonRec((JObject)prop.Value, prefix + prop.Name + ":");
                }
            }
        }

        public string this[string key]
        {
            get
            {
                string val = null;
                if (dict.TryGetValue(key, out val))
                    return val;
                return null;
            }
        }
    }

    public class JsonConfigVariablesBuilder : IConfigVariablesBuilder
    {
        private IConfigVariables result = new EmptyConfigVariables();

        public JsonConfigVariablesBuilder UseJsonContent(string jsonContent)
        {
            result = new JsonConfigVariables(jsonContent);
            return this;
        }

        public JsonConfigVariablesBuilder UseFile(string filePath, bool optional)
        {
            if(!File.Exists(filePath))
            {
                if (!optional)
                    throw new InvalidOperationException($"Required file {filePath} not found");
                return this;
            }

            var fileContent = File.ReadAllText(filePath);
            result = new JsonConfigVariables(fileContent);
            return this;
        }

        public IConfigVariables Build() => result;
    }

    public static class Exts
    {
        public static ConfigVariablesBuilder Json(
            this ConfigVariablesBuilder builder, 
            Action<JsonConfigVariablesBuilder> configAction
            )
        {
            var jsonBuilder = new JsonConfigVariablesBuilder();

            configAction?.Invoke(jsonBuilder);
            builder.AddBuilder(jsonBuilder);
            
            return builder;
        }

        public static ConfigVariablesBuilder Json(this ConfigVariablesBuilder builder,
            string jsonContent)
        {

            return builder.Json(b => b.UseJsonContent(jsonContent));
        }

        public static ConfigVariablesBuilder JsonFile(this ConfigVariablesBuilder builder,
            string filePath, bool optional = false)
        {

            return builder.Json(b => b.UseFile(filePath, optional));
        }
    }
}
