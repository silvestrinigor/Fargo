namespace Fargo.Cli.Output;

/// <summary>
/// Renders rows as a plain, left-aligned ASCII table with a header and separator, or writes the empty
/// message when there are no rows.
/// </summary>
public sealed class TableWriter<T>
{
    private readonly List<(string Header, Func<T, string?> Selector)> _columns = new();

    private string _emptyMessage = "No results.";

    /// <summary>
    /// Null or empty values are rendered as a dash.
    /// </summary>
    public TableWriter<T> Column(string header, Func<T, string?> selector)
    {
        _columns.Add((header, selector));
        return this;
    }

    public TableWriter<T> EmptyMessage(string message)
    {
        _emptyMessage = message;
        return this;
    }

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

    private static string FormatRow(string[] cells, int[] widths)
    {
        var parts = new string[cells.Length];

        for (var i = 0; i < cells.Length; i++)
        {
            // Do not pad the last column to avoid trailing whitespace.
            parts[i] = i == cells.Length - 1
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
