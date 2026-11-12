namespace Fargo.Cli.Output;

/// <summary>
/// Renders a collection of rows as a plain, left-aligned ASCII table with a
/// header row and a separator. Column widths are computed from the content so
/// the output stays readable on any console. When there are no rows, a friendly
/// "No results." line is written instead.
/// </summary>
public sealed class TableWriter<T>
{
    private readonly List<(string Header, Func<T, string?> Selector)> _columns = new();

    private string _emptyMessage = "No results.";

    /// <summary>
    /// Adds a column with the given header and a selector that extracts its
    /// value from a row. Null/empty values are rendered as a dash.
    /// </summary>
    public TableWriter<T> Column(string header, Func<T, string?> selector)
    {
        _columns.Add((header, selector));
        return this;
    }

    /// <summary>Sets the message shown when the collection is empty.</summary>
    public TableWriter<T> EmptyMessage(string message)
    {
        _emptyMessage = message;
        return this;
    }

    /// <summary>Writes the table for <paramref name="rows"/> to standard output.</summary>
    public void Write(IEnumerable<T>? rows)
    {
        var materialized = rows?.ToList() ?? new List<T>();

        if (materialized.Count == 0)
        {
            Console.WriteLine(_emptyMessage);
            return;
        }

        var headers = _columns.Select(c => c.Header).ToArray();

        var dataRows = materialized
            .Select(row => _columns
                .Select(c => Normalize(c.Selector(row)))
                .ToArray())
            .ToList();

        var widths = new int[_columns.Count];

        for (var i = 0; i < _columns.Count; i++)
        {
            widths[i] = headers[i].Length;

            foreach (var dataRow in dataRows)
            {
                if (dataRow[i].Length > widths[i])
                {
                    widths[i] = dataRow[i].Length;
                }
            }
        }

        Console.WriteLine(FormatRow(headers, widths));
        Console.WriteLine(FormatSeparator(widths));

        foreach (var dataRow in dataRows)
        {
            Console.WriteLine(FormatRow(dataRow, widths));
        }

        Console.WriteLine();
        Console.WriteLine(
            $"{materialized.Count} {(materialized.Count == 1 ? "result" : "results")}.");
    }

    private static string Normalize(string? value)
    {
        return string.IsNullOrEmpty(value) ? "-" : value;
    }

    private static string FormatRow(IReadOnlyList<string> cells, IReadOnlyList<int> widths)
    {
        var parts = new string[cells.Count];

        for (var i = 0; i < cells.Count; i++)
        {
            // Do not pad the last column to avoid trailing whitespace.
            parts[i] = i == cells.Count - 1
                ? cells[i]
                : cells[i].PadRight(widths[i]);
        }

        return string.Join("  ", parts);
    }

    private static string FormatSeparator(IReadOnlyList<int> widths)
    {
        return string.Join("  ", widths.Select(w => new string('-', w)));
    }
}
