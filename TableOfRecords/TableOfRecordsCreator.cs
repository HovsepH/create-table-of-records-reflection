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
            .Where(p => p.PropertyType.IsPrimitive || p.PropertyType == typeof(string) || p.PropertyType == typeof(decimal))
            .ToArray();
        if (properties.Length == 0)
        {
            throw new ArgumentException("Type must have at least one public property of a built-in type.");
        }
#pragma warning disable CS8602
        var columnWidths = properties
            .Select(p => Math.Max(
                p.Name.Length,
                collection.Max(e => p.GetValue(e)?.ToString().Length ?? 0))).ToArray();
#pragma warning restore CS8602
        WriteSeparator(writer, columnWidths);
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
            if (width < 3)
            {
                width = 3;
            }

            writer.Write(" " + FormatCell(value, width, properties[i].PropertyType) + " |");
        }

        writer.Write($"{Environment.NewLine}");
    }

    private static void WriteSeparator(TextWriter writer, int[] columnWidths)
    {
        writer.Write("+");
        foreach (var width in columnWidths)
        {
            if (width < 5)
            {
                int newWidth = 5;
                writer.Write(new string('-', newWidth));
            }
            else
            {
                writer.Write(new string('-', width + 2));
            }

            writer.Write("+");
        }

        writer.Write($"{Environment.NewLine}");
    }

    private static string FormatCell(string value, int width, Type propertyType)
    {
        if (propertyType == typeof(int) || propertyType == typeof(float) || propertyType == typeof(double))
        {
            return value.PadLeft(width);
        }
        else if (propertyType == typeof(decimal))
        {
            return value.PadRight(width);
        }
        else if (propertyType == typeof(char))
        {
            return value.PadRight(width);
        }
        else if (propertyType.IsPrimitive)
        {
            return value.PadLeft(width);
        }
        else
        {
            return value.PadRight(width);
        }
    }
}
