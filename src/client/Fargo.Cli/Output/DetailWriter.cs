namespace Fargo.Cli.Output;

/// <summary>
/// Renders a single entity as an aligned key/value block, for example:
/// <code>
/// GUID      00000000-0000-0000-0000-000000000004
/// Username  admin
/// Active    True
/// </code>
/// Labels are right-padded to a common width so the values line up.
/// </summary>
public sealed class DetailWriter
{
    private readonly List<(string Label, string? Value)> _fields = new();

    private string? _title;

    /// <summary>Sets an optional heading printed above the fields.</summary>
    public DetailWriter Title(string title)
    {
        _title = title;
        return this;
    }

    /// <summary>Adds a labelled field. Null/empty values are shown as a dash.</summary>
    public DetailWriter Field(string label, string? value)
    {
        _fields.Add((label, value));
        return this;
    }

    /// <summary>Adds a labelled field from any value using its string form.</summary>
    public DetailWriter Field(string label, object? value)
    {
        return Field(label, value?.ToString());
    }

    /// <summary>Writes the detail block to standard output.</summary>
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
