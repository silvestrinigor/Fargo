namespace Fargo.SimulationWorker;

/// <summary>
/// Generates server-valid, collision-free identifiers for the simulation.
///
/// The Fargo value objects impose rules the simulation must respect:
///  • Nameid: 3–100 chars, lowercased, no spaces, starts/ends with a letter or
///    digit, only letters/digits/'.'/'_'/'-', no consecutive separators.
///  • FirstName / LastName: 1–50 chars, letters plus single internal spaces or
///    hyphens only (effectively letters-only here).
///  • EAN-13: exactly 13 ASCII digits with a valid check digit; unique per
///    article, so every generated barcode must differ.
///  • Password: 9+ chars with at least one letter, one digit and one special.
///
/// A per-process monotonic counter guarantees uniqueness within a run, and the
/// run tag disambiguates across runs against the same database.
/// </summary>
internal sealed class Names(string runTag)
{
    private static readonly string[] FirstNames =
    [
        "Olivia", "Liam", "Emma", "Noah", "Ava", "Mateus", "Sofia", "Lucas",
        "Maria", "Pedro", "Alice", "Gabriel", "Helena", "Arthur", "Laura", "Bernardo",
    ];

    private readonly string _runTag = string.IsNullOrWhiteSpace(runTag)
        ? DateTime.UtcNow.ToString("yyMMddHHmmss")
        : runTag;

    private long _counter;

    /// <summary>Gets the run tag in use (for logging).</summary>
    public string RunTag => _runTag;

    /// <summary>A monotonically increasing, process-unique sequence value.</summary>
    public long Next() => Interlocked.Increment(ref _counter);

    /// <summary>
    /// Builds a unique, valid nameid of the form
    /// <c>{prefix}-{runTag}-{seq}</c> (lowercased). The prefix should be a plain
    /// lowercase word; separators never repeat because the parts are joined by a
    /// single hyphen and the run tag / sequence are alphanumeric.
    /// </summary>
    public string Nameid(string prefix) => $"{prefix}-{_runTag}-{Next():x}".ToLowerInvariant();

    /// <summary>Builds a unique, valid article/partition/group display name.</summary>
    public string Display(string prefix) => $"{prefix}-{_runTag}-{Next():x}";

    /// <summary>Picks a letters-only first name (optionally seeded by an index).</summary>
    public string FirstName(int index) => FirstNames[Math.Abs(index) % FirstNames.Length];

    /// <summary>
    /// Builds a unique, valid 13-digit EAN-13 (12 payload digits + check digit)
    /// from the next sequence value, so each article gets a distinct code.
    /// </summary>
    public string Ean13()
    {
        // 12 payload digits derived from run tag hash + sequence, kept in range.
        var seed = (ulong)(((uint)_runTag.GetHashCode()) ^ 0x9E3779B9) + (ulong)Next();
        var twelve = (100000000000UL + (seed % 899999999999UL)).ToString("D12");

        var sum = 0;
        for (var i = 0; i < 12; i++)
        {
            var d = twelve[i] - '0';
            sum += (i % 2 == 0) ? d : d * 3;
        }
        var check = (10 - (sum % 10)) % 10;

        return twelve + check.ToString();
    }
}
