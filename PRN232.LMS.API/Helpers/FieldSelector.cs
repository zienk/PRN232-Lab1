using System.Reflection;

namespace PRN232.LMS.API.Helpers;

/// <summary>
/// Helper to dynamically select fields from objects based on comma-separated field names.
/// </summary>
public static class FieldSelector
{
    /// <summary>
    /// Selects only the specified fields from an object, returning a dictionary.
    /// If fields is null or empty, returns all properties.
    /// </summary>
    public static object SelectFields<T>(T source, string? fields) where T : class
    {
        if (string.IsNullOrWhiteSpace(fields) || source == null)
            return source!;

        var fieldList = fields.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(f => f.Trim())
            .ToList();

        var result = new Dictionary<string, object?>();
        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var prop in properties)
        {
            if (fieldList.Any(f => f.Equals(prop.Name, StringComparison.OrdinalIgnoreCase)))
            {
                result[char.ToLower(prop.Name[0]) + prop.Name[1..]] = prop.GetValue(source);
            }
        }

        return result;
    }

    /// <summary>
    /// Selects fields from a list of objects.
    /// </summary>
    public static List<object> SelectFieldsList<T>(IEnumerable<T> source, string? fields) where T : class
    {
        if (string.IsNullOrWhiteSpace(fields))
            return source.Cast<object>().ToList();

        return source.Select(item => SelectFields(item, fields)).ToList();
    }
}
