namespace Fargo.Cli.Output;

/// <summary>
/// Renders a single entity as an aligned key/value block.
/// </summary>
public sealed class DetailWriter
{
    private readonly List<(string Label, string? Value)> _fields = [];

    private string? _title;

    public DetailWriter Title(string title)
    {
        _title = title;
        return this;
    }

    /// <summary>
    /// Null or empty values are shown as a dash.
    /// </summary>
    public DetailWriter Field(string label, string? value)
    {
        _fields.Add((label, value));
        return this;
    }

    public DetailWriter Field(string label, object? value)
    {
        return Field(label, value?.ToString());
    }

    public void Write()
    {
        if (!string.IsNullOrEmpty(_title))
        {
            Console.WriteLine(_title);
            Console.WriteLine(new string('-', _title.Length));
        }

        var labelWidth = _fields.Count == 0
            ? 0
            : _fields.Max(f => f.Label.Length);

        foreach (var (label, value) in _fields)
        {
            var text = string.IsNullOrEmpty(value) ? "-" : value;
            Console.WriteLine($"{label.PadRight(labelWidth)}  {text}");
        }
    }
}
