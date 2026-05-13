namespace Fargo.Core.Barcodes;

#region Formats

/// <summary>
/// Represents the barcode format (symbology) of a barcode.
/// </summary>
public enum BarcodeFormat
{
    /// <summary>EAN-13 — 13-digit retail barcode.</summary>
    Ean13 = 0,

    /// <summary>EAN-8 — 8-digit compact retail barcode.</summary>
    Ean8 = 1,

    /// <summary>UPC-A — 12-digit North American retail barcode.</summary>
    UpcA = 2,

    /// <summary>UPC-E — 8-digit compressed UPC barcode.</summary>
    UpcE = 3,

    /// <summary>Code 128 — variable-length alphanumeric barcode widely used in logistics.</summary>
    Code128 = 4,

    /// <summary>Code 39 — variable-length alphanumeric barcode used in industrial and military contexts.</summary>
    Code39 = 5,

    /// <summary>ITF-14 — 14-digit barcode used on outer packaging and pallets.</summary>
    Itf14 = 6,

    /// <summary>GS1-128 — Code 128 with GS1 Application Identifiers for supply chain.</summary>
    Gs1128 = 7,

    /// <summary>QR Code — 2D matrix barcode for URLs and structured data.</summary>
    QrCode = 8,

    /// <summary>Data Matrix — compact 2D matrix barcode for small item labelling.</summary>
    DataMatrix = 9,
}

#endregion Formats

#region Values

/// <summary>EAN-13 barcode value assigned to an article. Code must be exactly 13 digits.</summary>
public readonly struct Ean13 : IEquatable<Ean13>
{
    public const int CodeLength = 13;

    private readonly string code;

    /// <exception cref="ArgumentException">Thrown when the value is not exactly 13 ASCII digits.</exception>
    public Ean13(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        if (value.Length != CodeLength || !value.All(char.IsAsciiDigit))
        {
            throw new ArgumentException($"EAN-13 code must be exactly {CodeLength} digits.", nameof(value));
        }

        code = value;
    }

    private Ean13(string value, bool _) => code = value;

    /// <summary>Reconstructs an <see cref="Ean13"/> from stored data, bypassing validation.</summary>
    public static Ean13 FromStorage(string value) => new(value, false);

    /// <summary>Gets the barcode code string.</summary>
    public string Code => code ?? throw new InvalidOperationException("Ean13 not initialized.");

    /// <inheritdoc />
    public bool Equals(Ean13 other) => string.Equals(code, other.code, StringComparison.Ordinal);

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is Ean13 other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => code is null ? 0 : code.GetHashCode(StringComparison.Ordinal);

    /// <inheritdoc />
    public override string ToString() => Code;

    /// <summary>Determines whether two <see cref="Ean13"/> instances are equal.</summary>
    public static bool operator ==(Ean13 left, Ean13 right) => left.Equals(right);

    /// <summary>Determines whether two <see cref="Ean13"/> instances are not equal.</summary>
    public static bool operator !=(Ean13 left, Ean13 right) => !left.Equals(right);

    /// <summary>Explicitly converts a string to an <see cref="Ean13"/>.</summary>
    public static explicit operator Ean13(string value) => new(value);

    /// <summary>Implicitly converts an <see cref="Ean13"/> to its string representation.</summary>
    public static implicit operator string(Ean13 barcode) => barcode.Code;
}
/// <summary>EAN-8 barcode value assigned to an article. Code must be exactly 8 digits.</summary>
public readonly struct Ean8 : IEquatable<Ean8>
{
    public const int CodeLength = 8;

    private readonly string code;

    /// <exception cref="ArgumentException">Thrown when the value is not exactly 8 ASCII digits.</exception>
    public Ean8(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        if (value.Length != CodeLength || !value.All(char.IsAsciiDigit))
        {
            throw new ArgumentException($"EAN-8 code must be exactly {CodeLength} digits.", nameof(value));
        }

        code = value;
    }

    private Ean8(string value, bool _) => code = value;

    /// <summary>Reconstructs an <see cref="Ean8"/> from stored data, bypassing validation.</summary>
    public static Ean8 FromStorage(string value) => new(value, false);

    /// <summary>Gets the barcode code string.</summary>
    public string Code => code ?? throw new InvalidOperationException("Ean8 not initialized.");

    /// <inheritdoc />
    public bool Equals(Ean8 other) => string.Equals(code, other.code, StringComparison.Ordinal);

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is Ean8 other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => code is null ? 0 : code.GetHashCode(StringComparison.Ordinal);

    /// <inheritdoc />
    public override string ToString() => Code;

    /// <summary>Determines whether two <see cref="Ean8"/> instances are equal.</summary>
    public static bool operator ==(Ean8 left, Ean8 right) => left.Equals(right);

    /// <summary>Determines whether two <see cref="Ean8"/> instances are not equal.</summary>
    public static bool operator !=(Ean8 left, Ean8 right) => !left.Equals(right);

    /// <summary>Explicitly converts a string to an <see cref="Ean8"/>.</summary>
    public static explicit operator Ean8(string value) => new(value);

    /// <summary>Implicitly converts an <see cref="Ean8"/> to its string representation.</summary>
    public static implicit operator string(Ean8 barcode) => barcode.Code;
}
/// <summary>UPC-A barcode value assigned to an article. Code must be exactly 12 digits.</summary>
public readonly struct UpcA : IEquatable<UpcA>
{
    public const int CodeLength = 12;

    private readonly string code;

    /// <exception cref="ArgumentException">Thrown when the value is not exactly 12 ASCII digits.</exception>
    public UpcA(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        if (value.Length != CodeLength || !value.All(char.IsAsciiDigit))
        {
            throw new ArgumentException($"UPC-A code must be exactly {CodeLength} digits.", nameof(value));
        }

        code = value;
    }

    private UpcA(string value, bool _) => code = value;

    /// <summary>Reconstructs a <see cref="UpcA"/> from stored data, bypassing validation.</summary>
    public static UpcA FromStorage(string value) => new(value, false);

    /// <summary>Gets the barcode code string.</summary>
    public string Code => code ?? throw new InvalidOperationException("UpcA not initialized.");

    /// <inheritdoc />
    public bool Equals(UpcA other) => string.Equals(code, other.code, StringComparison.Ordinal);

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is UpcA other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => code is null ? 0 : code.GetHashCode(StringComparison.Ordinal);

    /// <inheritdoc />
    public override string ToString() => Code;

    /// <summary>Determines whether two <see cref="UpcA"/> instances are equal.</summary>
    public static bool operator ==(UpcA left, UpcA right) => left.Equals(right);

    /// <summary>Determines whether two <see cref="UpcA"/> instances are not equal.</summary>
    public static bool operator !=(UpcA left, UpcA right) => !left.Equals(right);

    /// <summary>Explicitly converts a string to a <see cref="UpcA"/>.</summary>
    public static explicit operator UpcA(string value) => new(value);

    /// <summary>Implicitly converts a <see cref="UpcA"/> to its string representation.</summary>
    public static implicit operator string(UpcA barcode) => barcode.Code;
}
/// <summary>UPC-E barcode value assigned to an article. Code must be exactly 8 digits.</summary>
public readonly struct UpcE : IEquatable<UpcE>
{
    public const int CodeLength = 8;

    private readonly string code;

    /// <exception cref="ArgumentException">Thrown when the value is not exactly 8 ASCII digits.</exception>
    public UpcE(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        if (value.Length != CodeLength || !value.All(char.IsAsciiDigit))
        {
            throw new ArgumentException($"UPC-E code must be exactly {CodeLength} digits.", nameof(value));
        }

        code = value;
    }

    private UpcE(string value, bool _) => code = value;

    /// <summary>Reconstructs a <see cref="UpcE"/> from stored data, bypassing validation.</summary>
    public static UpcE FromStorage(string value) => new(value, false);

    /// <summary>Gets the barcode code string.</summary>
    public string Code => code ?? throw new InvalidOperationException("UpcE not initialized.");

    /// <inheritdoc />
    public bool Equals(UpcE other) => string.Equals(code, other.code, StringComparison.Ordinal);

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is UpcE other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => code is null ? 0 : code.GetHashCode(StringComparison.Ordinal);

    /// <inheritdoc />
    public override string ToString() => Code;

    /// <summary>Determines whether two <see cref="UpcE"/> instances are equal.</summary>
    public static bool operator ==(UpcE left, UpcE right) => left.Equals(right);

    /// <summary>Determines whether two <see cref="UpcE"/> instances are not equal.</summary>
    public static bool operator !=(UpcE left, UpcE right) => !left.Equals(right);

    /// <summary>Explicitly converts a string to a <see cref="UpcE"/>.</summary>
    public static explicit operator UpcE(string value) => new(value);

    /// <summary>Implicitly converts a <see cref="UpcE"/> to its string representation.</summary>
    public static implicit operator string(UpcE barcode) => barcode.Code;
}
/// <summary>Code 128 barcode value assigned to an article. Code must be 1–80 printable ASCII characters.</summary>
public readonly struct Code128 : IEquatable<Code128>
{
    public const int MinLength = 1;
    public const int MaxLength = 80;

    private readonly string code;

    /// <exception cref="ArgumentException">Thrown when the value is not 1–80 printable ASCII characters.</exception>
    public Code128(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        if (value.Length is < MinLength or > MaxLength || !value.All(c => c is >= '\x20' and <= '\x7E'))
        {
            throw new ArgumentException($"Code 128 code must be {MinLength}–{MaxLength} printable ASCII characters.", nameof(value));
        }

        code = value;
    }

    private Code128(string value, bool _) => code = value;

    /// <summary>Reconstructs a <see cref="Code128"/> from stored data, bypassing validation.</summary>
    public static Code128 FromStorage(string value) => new(value, false);

    /// <summary>Gets the barcode code string.</summary>
    public string Code => code ?? throw new InvalidOperationException("Code128 not initialized.");

    /// <inheritdoc />
    public bool Equals(Code128 other) => string.Equals(code, other.code, StringComparison.Ordinal);

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is Code128 other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => code is null ? 0 : code.GetHashCode(StringComparison.Ordinal);

    /// <inheritdoc />
    public override string ToString() => Code;

    /// <summary>Determines whether two <see cref="Code128"/> instances are equal.</summary>
    public static bool operator ==(Code128 left, Code128 right) => left.Equals(right);

    /// <summary>Determines whether two <see cref="Code128"/> instances are not equal.</summary>
    public static bool operator !=(Code128 left, Code128 right) => !left.Equals(right);

    /// <summary>Explicitly converts a string to a <see cref="Code128"/>.</summary>
    public static explicit operator Code128(string value) => new(value);

    /// <summary>Implicitly converts a <see cref="Code128"/> to its string representation.</summary>
    public static implicit operator string(Code128 barcode) => barcode.Code;
}
/// <summary>Code 39 barcode value assigned to an article. Code must be 1–80 printable ASCII characters.</summary>
public readonly struct Code39 : IEquatable<Code39>
{
    public const int MinLength = 1;
    public const int MaxLength = 80;

    private readonly string code;

    /// <exception cref="ArgumentException">Thrown when the value is not 1–80 printable ASCII characters.</exception>
    public Code39(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        if (value.Length is < MinLength or > MaxLength || !value.All(c => c is >= '\x20' and <= '\x7E'))
        {
            throw new ArgumentException($"Code 39 code must be {MinLength}–{MaxLength} printable ASCII characters.", nameof(value));
        }

        code = value;
    }

    private Code39(string value, bool _) => code = value;

    /// <summary>Reconstructs a <see cref="Code39"/> from stored data, bypassing validation.</summary>
    public static Code39 FromStorage(string value) => new(value, false);

    /// <summary>Gets the barcode code string.</summary>
    public string Code => code ?? throw new InvalidOperationException("Code39 not initialized.");

    /// <inheritdoc />
    public bool Equals(Code39 other) => string.Equals(code, other.code, StringComparison.Ordinal);

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is Code39 other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => code is null ? 0 : code.GetHashCode(StringComparison.Ordinal);

    /// <inheritdoc />
    public override string ToString() => Code;

    /// <summary>Determines whether two <see cref="Code39"/> instances are equal.</summary>
    public static bool operator ==(Code39 left, Code39 right) => left.Equals(right);

    /// <summary>Determines whether two <see cref="Code39"/> instances are not equal.</summary>
    public static bool operator !=(Code39 left, Code39 right) => !left.Equals(right);

    /// <summary>Explicitly converts a string to a <see cref="Code39"/>.</summary>
    public static explicit operator Code39(string value) => new(value);

    /// <summary>Implicitly converts a <see cref="Code39"/> to its string representation.</summary>
    public static implicit operator string(Code39 barcode) => barcode.Code;
}
/// <summary>ITF-14 barcode value assigned to an article. Code must be exactly 14 digits.</summary>
public readonly struct Itf14 : IEquatable<Itf14>
{
    public const int CodeLength = 14;

    private readonly string code;

    /// <exception cref="ArgumentException">Thrown when the value is not exactly 14 ASCII digits.</exception>
    public Itf14(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        if (value.Length != CodeLength || !value.All(char.IsAsciiDigit))
        {
            throw new ArgumentException($"ITF-14 code must be exactly {CodeLength} digits.", nameof(value));
        }

        code = value;
    }

    private Itf14(string value, bool _) => code = value;

    /// <summary>Reconstructs an <see cref="Itf14"/> from stored data, bypassing validation.</summary>
    public static Itf14 FromStorage(string value) => new(value, false);

    /// <summary>Gets the barcode code string.</summary>
    public string Code => code ?? throw new InvalidOperationException("Itf14 not initialized.");

    /// <inheritdoc />
    public bool Equals(Itf14 other) => string.Equals(code, other.code, StringComparison.Ordinal);

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is Itf14 other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => code is null ? 0 : code.GetHashCode(StringComparison.Ordinal);

    /// <inheritdoc />
    public override string ToString() => Code;

    /// <summary>Determines whether two <see cref="Itf14"/> instances are equal.</summary>
    public static bool operator ==(Itf14 left, Itf14 right) => left.Equals(right);

    /// <summary>Determines whether two <see cref="Itf14"/> instances are not equal.</summary>
    public static bool operator !=(Itf14 left, Itf14 right) => !left.Equals(right);

    /// <summary>Explicitly converts a string to an <see cref="Itf14"/>.</summary>
    public static explicit operator Itf14(string value) => new(value);

    /// <summary>Implicitly converts an <see cref="Itf14"/> to its string representation.</summary>
    public static implicit operator string(Itf14 barcode) => barcode.Code;
}
/// <summary>GS1-128 barcode value assigned to an article. Code must be 1–80 printable ASCII characters.</summary>
public readonly struct Gs1128 : IEquatable<Gs1128>
{
    public const int MinLength = 1;
    public const int MaxLength = 80;

    private readonly string code;

    /// <exception cref="ArgumentException">Thrown when the value is not 1–80 printable ASCII characters.</exception>
    public Gs1128(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        if (value.Length is < MinLength or > MaxLength || !value.All(c => c is >= '\x20' and <= '\x7E'))
        {
            throw new ArgumentException($"GS1-128 code must be {MinLength}–{MaxLength} printable ASCII characters.", nameof(value));
        }

        code = value;
    }

    private Gs1128(string value, bool _) => code = value;

    /// <summary>Reconstructs a <see cref="Gs1128"/> from stored data, bypassing validation.</summary>
    public static Gs1128 FromStorage(string value) => new(value, false);

    /// <summary>Gets the barcode code string.</summary>
    public string Code => code ?? throw new InvalidOperationException("Gs1128 not initialized.");

    /// <inheritdoc />
    public bool Equals(Gs1128 other) => string.Equals(code, other.code, StringComparison.Ordinal);

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is Gs1128 other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => code is null ? 0 : code.GetHashCode(StringComparison.Ordinal);

    /// <inheritdoc />
    public override string ToString() => Code;

    /// <summary>Determines whether two <see cref="Gs1128"/> instances are equal.</summary>
    public static bool operator ==(Gs1128 left, Gs1128 right) => left.Equals(right);

    /// <summary>Determines whether two <see cref="Gs1128"/> instances are not equal.</summary>
    public static bool operator !=(Gs1128 left, Gs1128 right) => !left.Equals(right);

    /// <summary>Explicitly converts a string to a <see cref="Gs1128"/>.</summary>
    public static explicit operator Gs1128(string value) => new(value);

    /// <summary>Implicitly converts a <see cref="Gs1128"/> to its string representation.</summary>
    public static implicit operator string(Gs1128 barcode) => barcode.Code;
}
/// <summary>QR Code barcode value assigned to an article. Code must be 1–2953 characters.</summary>
public readonly struct QrCode : IEquatable<QrCode>
{
    public const int MinLength = 1;
    public const int MaxLength = 2953;

    private readonly string code;

    /// <exception cref="ArgumentException">Thrown when the value is not 1–2953 characters.</exception>
    public QrCode(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        if (value.Length is < MinLength or > MaxLength)
        {
            throw new ArgumentException($"QR Code must be {MinLength}–{MaxLength} characters.", nameof(value));
        }

        code = value;
    }

    private QrCode(string value, bool _) => code = value;

    /// <summary>Reconstructs a <see cref="QrCode"/> from stored data, bypassing validation.</summary>
    public static QrCode FromStorage(string value) => new(value, false);

    /// <summary>Gets the barcode code string.</summary>
    public string Code => code ?? throw new InvalidOperationException("QrCode not initialized.");

    /// <inheritdoc />
    public bool Equals(QrCode other) => string.Equals(code, other.code, StringComparison.Ordinal);

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is QrCode other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => code is null ? 0 : code.GetHashCode(StringComparison.Ordinal);

    /// <inheritdoc />
    public override string ToString() => Code;

    /// <summary>Determines whether two <see cref="QrCode"/> instances are equal.</summary>
    public static bool operator ==(QrCode left, QrCode right) => left.Equals(right);

    /// <summary>Determines whether two <see cref="QrCode"/> instances are not equal.</summary>
    public static bool operator !=(QrCode left, QrCode right) => !left.Equals(right);

    /// <summary>Explicitly converts a string to a <see cref="QrCode"/>.</summary>
    public static explicit operator QrCode(string value) => new(value);

    /// <summary>Implicitly converts a <see cref="QrCode"/> to its string representation.</summary>
    public static implicit operator string(QrCode barcode) => barcode.Code;
}
/// <summary>Data Matrix barcode value assigned to an article. Code must be 1–2335 characters.</summary>
public readonly struct DataMatrix : IEquatable<DataMatrix>
{
    public const int MinLength = 1;
    public const int MaxLength = 2335;

    private readonly string code;

    /// <exception cref="ArgumentException">Thrown when the value is not 1–2335 characters.</exception>
    public DataMatrix(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        if (value.Length is < MinLength or > MaxLength)
        {
            throw new ArgumentException($"Data Matrix code must be {MinLength}–{MaxLength} characters.", nameof(value));
        }

        code = value;
    }

    private DataMatrix(string value, bool _) => code = value;

    /// <summary>Reconstructs a <see cref="DataMatrix"/> from stored data, bypassing validation.</summary>
    public static DataMatrix FromStorage(string value) => new(value, false);

    /// <summary>Gets the barcode code string.</summary>
    public string Code => code ?? throw new InvalidOperationException("DataMatrix not initialized.");

    /// <inheritdoc />
    public bool Equals(DataMatrix other) => string.Equals(code, other.code, StringComparison.Ordinal);

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is DataMatrix other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => code is null ? 0 : code.GetHashCode(StringComparison.Ordinal);

    /// <inheritdoc />
    public override string ToString() => Code;

    /// <summary>Determines whether two <see cref="DataMatrix"/> instances are equal.</summary>
    public static bool operator ==(DataMatrix left, DataMatrix right) => left.Equals(right);

    /// <summary>Determines whether two <see cref="DataMatrix"/> instances are not equal.</summary>
    public static bool operator !=(DataMatrix left, DataMatrix right) => !left.Equals(right);

    /// <summary>Explicitly converts a string to a <see cref="DataMatrix"/>.</summary>
    public static explicit operator DataMatrix(string value) => new(value);

    /// <summary>Implicitly converts a <see cref="DataMatrix"/> to its string representation.</summary>
    public static implicit operator string(DataMatrix barcode) => barcode.Code;
}

#endregion Values
