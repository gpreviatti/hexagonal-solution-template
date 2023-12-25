using System.Collections.Concurrent;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Text.Json;
using Newtonsoft.Json.Linq;
using Xunit.Sdk;

namespace Hexagonal.Solution.Template.Tests.Common.Attributes;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public sealed class JsonResourceDataAttribute : DataAttribute
{
    private static readonly ConcurrentDictionary<Assembly, ResourceReader> _resourceReaders = new();

    private readonly string _resourceName;
    private readonly string _propertyName;

    /// <summary>Load data from a JSON file as the data source for a theory.</summary>
    /// <param name="resourceName">The resource name that contains the JSON content to load.</param>
    /// <param name="propertyName">The name of the property on the JSON file that contains the data for the test.</param>
    public JsonResourceDataAttribute(string resourceName, string propertyName = null)
    {
        _resourceName = resourceName;
        _propertyName = propertyName;
    }

    public bool UseNewtonsoft { get; set; } = true;

    public override IEnumerable<object[]> GetData(MethodInfo testMethod)
    {
        if (testMethod == null || testMethod.DeclaringType == null)
        {
            throw new ArgumentNullException(nameof(testMethod));
        }

        var parameters = testMethod.GetParameters().Select(p => p.ParameterType).ToArray();
        var resourceReader = _resourceReaders.GetOrAdd(testMethod.DeclaringType.Assembly, x => new ResourceReader(x));
        string content = resourceReader.GetString(_resourceName);

        if (UseNewtonsoft)
        {
            var jObject = JObject.Parse(content);

            return Deserialize(
                string.IsNullOrEmpty(_propertyName)
                    ? jObject
                    : jObject[_propertyName],
                parameters);
        }
        else
        {
            var jsonDocument = JsonDocument.Parse(content);

            return Deserialize(
                string.IsNullOrEmpty(_propertyName)
                    ? jsonDocument.RootElement
                    : jsonDocument.RootElement.GetProperty(_propertyName),
                parameters);
        }
    }

    private static IEnumerable<object[]> Deserialize(JToken token, Type[] parameters)
    {
        if (token is null)
            return Array.Empty<object[]>();

        if (token is not JArray jArray)
        {
            return new[] { DeserializeData(token, parameters) };
        }

        var list = new List<object[]>();
        foreach (var element in jArray)
        {
            list.Add(DeserializeData(element, parameters));
        }

        return list;
    }

    private static IEnumerable<object[]> Deserialize(JsonElement jsonElement, Type[] parameters)
    {
        if (jsonElement.ValueKind != JsonValueKind.Array)
        {
            return new[] { DeserializeData(jsonElement, parameters) };
        }

        var list = new List<object[]>();
        foreach (var element in jsonElement.EnumerateArray())
        {
            list.Add(DeserializeData(element, parameters));
        }

        return list;
    }

    private static object[] DeserializeData(JToken token, Type[] parameters)
    {
        if (token is not JArray jArray)
        {
            if (parameters.Length > 1)
                throw new InvalidOperationException("JSON content must be an array to represent each parameter");

            return [DeserializeParameter(token, parameters[0])];
        }

        var result = new object[parameters.Length];
        int index = 0;
        foreach (var element in jArray)
        {
            result[index] = DeserializeParameter(element, parameters[index]);
            index++;
        }

        return result;
    }

    private static object[] DeserializeData(JsonElement jsonElement, Type[] parameters)
    {
        if (jsonElement.ValueKind != JsonValueKind.Array)
        {
            if (parameters.Length > 1)
                throw new InvalidOperationException("JSON content must be an array to represent each parameter");

            return [DeserializeParameter(jsonElement, parameters[0])];
        }

        var result = new object[parameters.Length];
        int index = 0;
        foreach (var element in jsonElement.EnumerateArray())
        {
            result[index] = DeserializeParameter(element, parameters[index]);
            index++;
        }

        return result;
    }

    private static object DeserializeParameter(JToken element, Type parameterType)
    {
        return parameterType == typeof(string) && element.Type != JTokenType.String
            ? element.ToString()
            : element.ToObject(parameterType);
    }

    private static object DeserializeParameter(JsonElement element, Type parameterType)
    {
        return parameterType == typeof(string) && element.ValueKind != JsonValueKind.String
            ? element.GetRawText()
            : element.Deserialize(parameterType);
    }
}

public sealed class ResourceReader(Assembly assembly)
{
    private readonly Assembly _thisAssembly = assembly;
    private string[] _resourceNames;

    private string[] ResourceNames => _resourceNames ??= _thisAssembly.GetManifestResourceNames();

    public string GetString(string resourceName, params object[] args)
    {
        return args is null || args.Length == 0
            ? LoadEmbeddedResource(FindResourceName(resourceName) ?? resourceName)
            : string.Format(
                CultureInfo.InvariantCulture,
                LoadEmbeddedResource(FindResourceName(resourceName) ?? resourceName),
                args);
    }

    private string FindResourceName(string partialName) =>
        Array.Find(ResourceNames, n => n.EndsWith(partialName, StringComparison.Ordinal));

    private string LoadEmbeddedResource(string resourceName)
    {
        using var stream = _thisAssembly.GetManifestResourceStream(resourceName);
        if (stream is null)
        {
            throw new ArgumentException(
                $"Could not find embedded resource {resourceName}. " +
                $"Available names: {string.Join(", ", ResourceNames)}.");
        }

        using var reader = new StreamReader(stream, Encoding.UTF8);
        return reader.ReadToEnd();
    }
}