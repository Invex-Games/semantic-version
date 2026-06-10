# `SemVer` — API Reference

**Namespace:** `Invex.SemanticVersion`  
**Assembly:** `Invex.SemanticVersion`  
**Targets:** .NET 8.0 · .NET 9.0 · .NET 10.0

```csharp
public sealed partial class SemVer
    : ISpanParsable<SemVer>,
      IComparable<SemVer>,
      IComparisonOperators<SemVer, SemVer, bool>
```

Represents an immutable semantic version following the [Semantic Versioning 2.0.0](https://semver.org/) specification.  
Supports parsing, comparison, serialization, and conversion for version strings of the form `MAJOR.MINOR.PATCH[-PRERELEASE][+METADATA]`.

---

## Constructors

`SemVer` instances are created through static factory methods (`Parse`, `TryParse`, `FromSystemVersion`) or via implicit conversion from `string`.  
There is no public constructor.

---

## Properties

### `Major`

```csharp
public int Major { get; }
```

The major version number. Incremented for incompatible API changes.

---

### `Minor`

```csharp
public int Minor { get; }
```

The minor version number. Incremented for backwards-compatible new functionality.

---

### `Patch`

```csharp
public int Patch { get; }
```

The patch version number. Incremented for backwards-compatible bug fixes.

---

### `Prefix`

```csharp
[JsonIgnore]
public string Prefix { get; }
```

The core version string in the format `"MAJOR.MINOR.PATCH"`, without pre-release or metadata.

**Example:**

```csharp
SemVer.Parse("1.2.3-alpha+build").Prefix; // "1.2.3"
```

---

### `PreRelease`

```csharp
public string? PreRelease { get; }
```

The pre-release identifier, or `null` if this is a release version.  
Pre-release identifiers are dot-separated and may contain alphanumeric characters and hyphens (e.g. `"alpha.1"`, `"rc.2"`, `"beta"`).

---

### `IsPreRelease`

```csharp
[JsonIgnore]
public bool IsPreRelease { get; }
```

`true` when `PreRelease` is non-null; `false` for release versions.

---

### `Metadata`

```csharp
public string? Metadata { get; }
```

The build metadata string, or `null` if no metadata is present.  
Metadata is considered a tiebreaker in this implementation's comparison (the spec ignores it for precedence, but this library uses it as a final tie-breaker).

---

### `BuildNumberFromPreRelease`

```csharp
[JsonIgnore]
public int BuildNumberFromPreRelease { get; }
```

The single numeric value extracted from `PreRelease`, or `0` if no single number is found.  
Returns `0` if the pre-release is `null`, contains no number, or contains more than one number.

**Examples:**

```csharp
SemVer.Parse("1.0.0-rc.42").BuildNumberFromPreRelease;   // 42
SemVer.Parse("1.0.0-alpha").BuildNumberFromPreRelease;   // 0 — no number
SemVer.Parse("1.0.0-1.2").BuildNumberFromPreRelease;     // 0 — multiple numbers
SemVer.Parse("1.0.0").BuildNumberFromPreRelease;         // 0 — no pre-release
```

---

### `BuildNumberFromMetadata`

```csharp
[JsonIgnore]
public int BuildNumberFromMetadata { get; }
```

The single numeric value extracted from `Metadata`, or `0` if no single number is found.  
Same rules as `BuildNumberFromPreRelease`.

**Examples:**

```csharp
SemVer.Parse("1.0.0+build.99").BuildNumberFromMetadata;  // 99
SemVer.Parse("1.0.0+build").BuildNumberFromMetadata;     // 0 — no number
SemVer.Parse("1.0.0").BuildNumberFromMetadata;           // 0 — no metadata
```

---

### `One` *(static)*

```csharp
public static SemVer One { get; }
```

A pre-built instance representing version `1.0.0`.

```csharp
SemVer.One.Major;       // 1
SemVer.One.Minor;       // 0
SemVer.One.Patch;       // 0
SemVer.One.PreRelease;  // null
SemVer.One.Metadata;    // null
```

---

## Static factory methods

### `Parse(string)`

```csharp
public static SemVer Parse(string s)
```

Parses a version string. Throws `ArgumentException` if `s` is not a valid SemVer 2.0.0 string.

---

### `Parse(string, IFormatProvider?)`

```csharp
public static SemVer Parse(string s, IFormatProvider? provider)
```

`ISpanParsable<SemVer>` interface implementation. The `provider` parameter is ignored — parsing is always culture-invariant.

---

### `Parse(ReadOnlySpan<char>, IFormatProvider?)`

```csharp
public static SemVer Parse(ReadOnlySpan<char> s, IFormatProvider? provider)
```

Span-based parse. Avoids a string allocation when the input is already a span. Throws `ArgumentException` for invalid input.

---

### `TryParse(string?, out SemVer?)`

```csharp
public static bool TryParse(
    [NotNullWhen(true)] string? s,
    [MaybeNullWhen(false)] out SemVer result)
```

Non-throwing parse from string. Returns `false` and sets `result` to `null` when `s` is `null` or invalid.

---

### `TryParse(string?, IFormatProvider?, out SemVer?)`

```csharp
public static bool TryParse(
    [NotNullWhen(true)] string? s,
    IFormatProvider? provider,
    [MaybeNullWhen(false)] out SemVer result)
```

`ISpanParsable<SemVer>` interface implementation. The `provider` parameter is ignored.

---

### `TryParse(ReadOnlySpan<char>, out SemVer?)`

```csharp
public static bool TryParse(
    ReadOnlySpan<char> s,
    [MaybeNullWhen(false)] out SemVer result)
```

Non-throwing span parse (convenience overload without `IFormatProvider`).

---

### `TryParse(ReadOnlySpan<char>, IFormatProvider?, out SemVer?)`

```csharp
public static bool TryParse(
    ReadOnlySpan<char> s,
    IFormatProvider? provider,
    [MaybeNullWhen(false)] out SemVer result)
```

`ISpanParsable<SemVer>` interface implementation for spans.

---

### `FromSystemVersion(Version, bool)`

```csharp
public static SemVer FromSystemVersion(Version version, bool throwIfContainsRevision = false)
```

Creates a `SemVer` from a `System.Version`, mapping `Major.Minor.Build` to `Major.Minor.Patch`.

| Parameter | Default | Description |
|-----------|---------|-------------|
| `version` | — | The `System.Version` to convert (required). |
| `throwIfContainsRevision` | `false` | When `true`, throws `ArgumentException` if `version.Revision > 0`. |

---

### `ExtractBuildNumber(string?)`

```csharp
public static int ExtractBuildNumber(string? input)
```

Extracts a single numeric sequence from `input`.

- Returns the number when exactly one numeric sequence is present.
- Returns `0` for `null`, empty, whitespace, zero matches, or more than one match.

---

## Instance methods

### `CompareTo(SemVer?)`

```csharp
public int CompareTo(SemVer? other)
```

Compares using SemVer 2.0.0 precedence rules.

| Return value | Meaning |
|-------------|---------|
| `< 0` | This instance precedes `other` |
| `0`   | Equal precedence |
| `> 0` | This instance follows `other` |

`null` is always less than any non-null instance.

---

### `IsBetween(SemVer, SemVer)`

```csharp
public bool IsBetween(SemVer firstBound, SemVer secondBound)
```

Returns `true` if this version is strictly between `firstBound` and `secondBound` (both exclusive).  
Bounds may be supplied in any order. When both bounds are equal, returns `true` only if this version equals the bounds.

---

### `ToSystemVersion(bool, bool)`

```csharp
public Version ToSystemVersion(
    bool throwIfContainsPreRelease = false,
    bool throwIfContainsMetadata = false)
```

Converts this `SemVer` to a `System.Version`, mapping `Major.Minor.Patch` to `Major.Minor.Build`.

| Parameter | Default | Description |
|-----------|---------|-------------|
| `throwIfContainsPreRelease` | `false` | Throw `ArgumentException` if `PreRelease` is non-null. |
| `throwIfContainsMetadata`   | `false` | Throw `ArgumentException` if `Metadata` is non-null. |

---

### `Equals(SemVer?)`

```csharp
public bool Equals(SemVer? other)
```

Returns `true` when `CompareTo(other) == 0`.

---

### `Equals(object?)`

```csharp
public override bool Equals(object? obj)
```

Returns `true` when `obj` is a `SemVer` and `Equals((SemVer)obj)` is `true`.

---

### `GetHashCode()`

```csharp
public override int GetHashCode()
```

Hash code based on `Major`, `Minor`, `Patch`, `PreRelease`, and `Metadata`. Consistent with `Equals`.

---

### `ToString()`

```csharp
public override string ToString()
```

Returns the canonical version string:

- `"MAJOR.MINOR.PATCH"` — no pre-release or metadata
- `"MAJOR.MINOR.PATCH-PRERELEASE"` — pre-release only
- `"MAJOR.MINOR.PATCH+METADATA"` — metadata only
- `"MAJOR.MINOR.PATCH-PRERELEASE+METADATA"` — both

---

## Operators

### Comparison operators

```csharp
public static bool operator >(SemVer left, SemVer right)
public static bool operator >=(SemVer left, SemVer right)
public static bool operator <(SemVer left, SemVer right)
public static bool operator <=(SemVer left, SemVer right)
public static bool operator ==(SemVer? left, SemVer? right)
public static bool operator !=(SemVer? left, SemVer? right)
```

All comparison operators delegate to `CompareTo`. `==` and `!=` are null-safe.

### Implicit conversions

```csharp
// string → SemVer (calls Parse)
public static implicit operator SemVer(string s)

// SemVer → string (calls ToString)
public static implicit operator string(SemVer semVer)
```

---

## JSON serialization

`SemVer` is fully compatible with `System.Text.Json` without any custom configuration.

**Serialized fields:**

| JSON key | C# property | Included when |
|----------|-------------|---------------|
| `major` | `Major` | Always |
| `minor` | `Minor` | Always |
| `patch` | `Patch` | Always |
| `preRelease` | `PreRelease` | Always (may be `null`) |
| `metadata` | `Metadata` | Always (may be `null`) |

**Excluded fields** (`[JsonIgnore]`): `Prefix`, `IsPreRelease`, `BuildNumberFromPreRelease`, `BuildNumberFromMetadata`.

---

## Interface implementations

| Interface | Notes |
|-----------|-------|
| `ISpanParsable<SemVer>` | Provides `Parse` and `TryParse` for both `string` and `ReadOnlySpan<char>`. |
| `IComparable<SemVer>` | Provides `CompareTo` for use with sorting and ordered collections. |
| `IComparisonOperators<SemVer, SemVer, bool>` | Provides `<`, `<=`, `>`, `>=`, `==`, `!=` operators. |

