using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace McpHost.Utilities;

internal class PrepareFunction
{
    public static BinaryData PrepareFunctionSchema(string rawSchemaJson)
    {
        using JsonDocument doc = JsonDocument.Parse(rawSchemaJson);
        using var output = new MemoryStream();
        using var writer = new Utf8JsonWriter(output);

        writer.WriteStartObject();

        foreach (var property in doc.RootElement.EnumerateObject())
        {
            if (property.Name != "name" && property.Name != "description" && property.Name != "title")
            {
                property.WriteTo(writer);
            }
        }

        writer.WriteEndObject();
        writer.Flush();

        return BinaryData.FromBytes(output.ToArray());
    }
}
