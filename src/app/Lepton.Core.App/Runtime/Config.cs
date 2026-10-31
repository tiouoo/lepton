using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Lepton.Core.App.Runtime;

public partial class Config
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
        Converters = { new JsonStringEnumConverter(allowIntegerValues: false) }
    };

    [JsonIgnore]
    public IReadOnlyDictionary<string, Exception> LoadErrors { get; private set; }
        = ReadOnlyDictionary<string, Exception>.Empty;

    [JsonIgnore]
    public Exception? LoadException { get; private set; }
    
    public void Initialize(string jsonPath, Action<Exception>? onError = null)
    {
        Config loaded;
        try
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(jsonPath);
            using var stream = File.OpenRead(jsonPath);
            using var document = JsonDocument.Parse(stream);
            if (document.RootElement.ValueKind != JsonValueKind.Object)
                throw new JsonException("Configuration must be a JSON object.");

            loaded = new Config();
            var errors = new Dictionary<string, Exception>();
            var properties = SerializerOptions.GetTypeInfo(typeof(Config)).Properties
                .Where(property => property.Set is not null)
                .ToDictionary(property => property.Name, StringComparer.OrdinalIgnoreCase);

            foreach (var member in document.RootElement.EnumerateObject())
            {
                if (!properties.TryGetValue(member.Name, out var property)) continue;
                var path = "$[" + JsonSerializer.Serialize(member.Name) + "]";
                try
                {
                    var value = member.Value.Deserialize(property.PropertyType, SerializerOptions);
                    property.Set!(loaded, value);
                }
                catch (JsonException exception)
                {
                    var nestedPath = exception.Path;
                    if (nestedPath?.StartsWith('$') == true)
                        path += nestedPath[1..];
                    errors[path] = exception;
                }
            }

            loaded.LoadErrors = new ReadOnlyDictionary<string, Exception>(errors);
        }
        catch (Exception exception) when (exception is FileNotFoundException or DirectoryNotFoundException)
        {
            loaded = new Config();
        }
        catch (Exception exception) when (exception is JsonException or IOException
                                         or UnauthorizedAccessException or ArgumentException
                                         or NotSupportedException or System.Security.SecurityException)
        {
            Context.Config.LoadErrors = ReadOnlyDictionary<string, Exception>.Empty;
            Context.Config.LoadException = exception;
            Trace.TraceError("Configuration load failed: {0}", exception);
            onError?.Invoke(exception);
            return;
        }

        Context.Config = loaded;
    }

    public string ToJson() => JsonSerializer.Serialize(this, SerializerOptions);
}
