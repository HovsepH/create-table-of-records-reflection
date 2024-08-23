using System.Reflection;

namespace TableOfRecords;

/// <summary>
/// Presents method that write in table form to the text stream a set of elements of type T.
/// </summary>
public static class TableOfRecordsCreator
{
    /// <summary>
    /// Write in table form to the text stream a set of elements of type T (<see cref="ICollection{T}"/>),
    /// where the state of each object of type T is described by public properties that have only build-in
    /// type (int, char, string etc.)
    /// </summary>
    /// <typeparam name="T">Type selector.</typeparam>
    /// <param name="collection">Collection of elements of type T.</param>
    /// <param name="writer">Text stream.</param>
    /// <exception cref="ArgumentNullException">Throw if <paramref name="collection"/> is null.</exception>
    /// <exception cref="ArgumentNullException">Throw if <paramref name="writer"/> is null.</exception>
    /// <exception cref="ArgumentException">Throw if <paramref name="collection"/> is empty.</exception>
    public static void WriteTable<T>(ICollection<T>? collection, TextWriter? writer)
    {
        if (collection == null)
        {
            throw new ArgumentNullException(nameof(collection), "Collection is null");
        }

        if (writer == null)
        {
            throw new ArgumentNullException(nameof(writer), "writer is null");
        }

        if (collection.Count == 0)
        {
            throw new ArgumentException("collection is empty", nameof(collection));
        }

        Type type = typeof(T);
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.PropertyType.IsPrimitive || p.PropertyType == typeof(string))
            .ToArray();
        if (properties.Length == 0)
        {
            throw new ArgumentException("Type must have at least one public property of a built-in type.");
        }

        var columnWidths = properties
                .Select(p => collection.Max(e => p.GetValue(e)?.ToString().Length ?? 0))
                .ToArray();
        WriteRow<T>(writer, properties, columnWidths, isHeader: true);
        WriteSeparator(writer, columnWidths);

        foreach (var item in collection)
        {
            WriteRow(writer, properties, columnWidths, item, isHeader: false);
            WriteSeparator(writer, columnWidths);
        }
    }

    private static void WriteRow<T>(TextWriter writer, PropertyInfo[] properties, int[] columnWidths, T? item = default, bool isHeader = false)
    {
        writer.Write("|");
        for (int i = 0; i < properties.Length; i++)
        {
            var value = isHeader ? properties[i].Name : properties[i].GetValue(item)?.ToString() ?? string.Empty;
            var width = columnWidths[i];
            writer.Write(" " + FormatCell(value, width, properties[i].PropertyType) + " |");
        }

        writer.WriteLine();
    }

    private static void WriteSeparator(TextWriter writer, int[] columnWidths)
    {
        writer.Write("+");
        foreach (var width in columnWidths)
        {
            writer.Write(new string('-', width + 2)); // +2 for spaces before and after cell content
            writer.Write("+");
        }

        writer.WriteLine();
    }

    private static string FormatCell(string value, int width, Type propertyType)
    {
        if (propertyType == typeof(int) || propertyType == typeof(float) || propertyType == typeof(double) || propertyType == typeof(decimal))
        {
            return value.PadLeft(width); // Right-align numbers
        }
        else
        {
            return value.PadRight(width); // Left-align strings
        }
    }
}
