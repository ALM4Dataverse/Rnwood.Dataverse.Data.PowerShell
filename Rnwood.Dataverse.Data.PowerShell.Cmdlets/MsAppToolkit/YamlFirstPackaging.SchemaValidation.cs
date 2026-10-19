using System.Globalization;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Json.Schema;
using YamlDotNet.Core;
using YamlDotNet.RepresentationModel;

namespace MsAppToolkit;

public static partial class YamlFirstPackaging
{
    private static Json.Schema.JsonSchema? _paYamlSchema;
    private static readonly object _paYamlSchemaLock = new();

    /// <summary>
    /// Loads (and caches) the embedded pa.schema.yaml, converting it from YAML to a
    /// JsonSchema.Net <see cref="Json.Schema.JsonSchema"/> instance.
    /// </summary>
    private static Json.Schema.JsonSchema LoadPaYamlSchema()
    {
        if (_paYamlSchema is not null)
        {
            return _paYamlSchema;
        }

        lock (_paYamlSchemaLock)
        {
            if (_paYamlSchema is not null)
            {
                return _paYamlSchema;
            }

            var assembly = typeof(YamlFirstPackaging).Assembly;
            using var stream = assembly.GetManifestResourceStream("MsAppToolkit.Resources.pa.schema.yaml")
                ?? throw new InvalidOperationException(
                    "Embedded resource 'MsAppToolkit.Resources.pa.schema.yaml' not found. "
                    + "Ensure pa.schema.yaml is included as an EmbeddedResource in the project.");

            var yamlText = new StreamReader(stream, Encoding.UTF8).ReadToEnd();

            // Fix a known defect in the official pa.schema.yaml: the CodeComponent-ComponentName
            // pattern has an unbalanced ')' that .NET's regex engine (and JsonSchema.Net) rejects.
            // Patch the raw YAML text before conversion so the outer optional group is complete.
            yamlText = yamlText.Replace(
                @"^([a-zA-Z][a-zA-Z0-9]{1,7})_)?(\w+\.)+(\w+)(\([0-9a-f-]{36}\))?$",
                @"^(([a-zA-Z][a-zA-Z0-9]{1,7})_)?(\w+\.)+(\w+)(\([0-9a-f-]{36}\))?$");

            var schemaNode = ParseYamlToJsonNode(yamlText)
                ?? throw new InvalidOperationException("pa.schema.yaml parsed to null.");

            _paYamlSchema = Json.Schema.JsonSchema.FromText(schemaNode.ToJsonString());
            return _paYamlSchema;
        }
    }

    /// <summary>
    /// Validates all Src/*.pa.yaml entries against the embedded pa.schema.yaml (Draft-07).
    /// Returns a list of human-readable issue strings; empty means fully valid.
    /// </summary>
    internal static IReadOnlyList<string> ValidatePaYamlFilesAgainstSchema(
        IEnumerable<KeyValuePair<string, byte[]>> entries)
    {
        Json.Schema.JsonSchema schema;
        try
        {
            schema = LoadPaYamlSchema();
        }
        catch (Exception ex)
        {
            return [$"Could not load pa.schema.yaml: {ex.Message}"];
        }

        var options = new EvaluationOptions
        {
            OutputFormat = OutputFormat.List,
        };

        var allIssues = new List<string>();

        foreach (var (path, bytes) in entries
            .Where(e => e.Key.StartsWith("Src/", StringComparison.OrdinalIgnoreCase)
                     && e.Key.EndsWith(".pa.yaml", StringComparison.OrdinalIgnoreCase))
            .OrderBy(e => e.Key, StringComparer.OrdinalIgnoreCase))
        {
            var yaml = Encoding.UTF8.GetString(bytes);

            JsonNode? node;
            try
            {
                node = ParseYamlToJsonNode(yaml);
            }
            catch (Exception ex)
            {
                allIssues.Add($"[{path}] YAML parse error: {ex.Message}");
                continue;
            }

            if (node is null)
            {
                continue;
            }

            using var document = JsonDocument.Parse(node.ToJsonString());
            var result = schema.Evaluate(document.RootElement, options);
            if (!result.IsValid)
            {
                CollectSchemaErrors(result, path, allIssues);
            }
        }

        return allIssues;
    }

    private static void CollectSchemaErrors(EvaluationResults result, string filePath, List<string> issues)
    {
        if (result.IsValid)
        {
            return;
        }

        if (result.Errors is { Count: > 0 })
        {
            var location = result.InstanceLocation.ToString();
            if (string.IsNullOrEmpty(location))
            {
                location = "/";
            }
            foreach (var (keyword, message) in result.Errors)
            {
                issues.Add($"[{filePath}] at {location}: {keyword} - {message}");
            }
        }

        if (result.Details is not null)
        {
            foreach (var child in result.Details)
            {
                CollectSchemaErrors(child, filePath, issues);
            }
        }
    }

    /// <summary>
    /// Converts a YAML string to a <see cref="JsonNode"/> using <see cref="YamlStream"/> so
    /// that each scalar's quoting style is honoured: plain scalars receive YAML 1.1 type
    /// coercions (boolean / null / integer / float), while quoted scalars stay as strings.
    /// </summary>
    private static JsonNode? ParseYamlToJsonNode(string yaml)
    {
        using var reader = new StringReader(yaml);
        var stream = new YamlStream();
        stream.Load(reader);

        if (stream.Documents.Count == 0)
        {
            return null;
        }

        return YamlNodeToJsonNode(stream.Documents[0].RootNode);
    }

    private static JsonNode? YamlNodeToJsonNode(YamlNode node)
    {
        switch (node)
        {
            case YamlMappingNode mapping:
            {
                var obj = new JsonObject();
                foreach (var (keyNode, valueNode) in mapping)
                {
                    var key = (keyNode as YamlScalarNode)?.Value ?? string.Empty;
                    if (!string.IsNullOrEmpty(key))
                    {
                        obj[key] = YamlNodeToJsonNode(valueNode);
                    }
                }

                return obj;
            }

            case YamlSequenceNode sequence:
            {
                var arr = new JsonArray();
                foreach (var item in sequence)
                {
                    arr.Add(YamlNodeToJsonNode(item));
                }

                return arr;
            }

            case YamlScalarNode scalar:
            {
                var value = scalar.Value ?? string.Empty;
                var style = scalar.Style;

                // Quoted scalars (single-quoted, double-quoted, literal, folded) are always
                // JSON strings — no YAML type coercion applied.
                if (style is not ScalarStyle.Plain and not ScalarStyle.Any)
                {
                    return JsonValue.Create(value);
                }

                // Empty plain scalar or explicit YAML null → JSON null.
                if (value.Length == 0 || value is "null" or "Null" or "NULL" or "~")
                {
                    return null;
                }

                // YAML 1.1 booleans → JSON boolean.
                if (value is "true" or "True" or "TRUE" or "yes" or "Yes" or "YES" or "on" or "On" or "ON")
                    return JsonValue.Create(true);

                if (value is "false" or "False" or "FALSE" or "no" or "No" or "NO" or "off" or "Off" or "OFF")
                    return JsonValue.Create(false);

                // Integer.
                if (long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var lv))
                    return JsonValue.Create(lv);

                // Floating-point.
                if (double.TryParse(value, NumberStyles.Float | NumberStyles.AllowLeadingSign,
                                    CultureInfo.InvariantCulture, out var dv))
                    return JsonValue.Create(dv);

                return JsonValue.Create(value);
            }

            default:
                return null;
        }
    }
}
