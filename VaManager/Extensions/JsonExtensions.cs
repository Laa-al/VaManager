using System.Text.Json.Nodes;

namespace VaManager.Extensions;

public static class JsonExtensions
{
    public static string[] ToStringArray(this JsonNode? node)
    {
        if (node is not JsonArray array) return [];
        List<string> result = [];

        foreach (var element in array)
        {
            var str = element?.GetValue<string?>();
            if (string.IsNullOrEmpty(str)) continue;
            result.Add(str);
        }

        return result.ToArray();
    }
}