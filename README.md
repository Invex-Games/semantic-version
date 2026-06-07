# Invex.SemanticVersion

A small, dependency-free C# implementation of [Semantic Versioning 2.0.0](https://semver.org/).
It provides a single immutable `SemVer` type for **parsing**, **comparing**, **serializing**, and
**converting** version strings of the form `MAJOR.MINOR.PATCH[-PRERELEASE][+METADATA]`.

[![NuGet](https://img.shields.io/nuget/v/Invex.SemanticVersion.svg)](https://www.nuget.org/packages/Invex.SemanticVersion/)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE.txt)

---

## Features

### Spec-compliant parsing

Spec-compliant parsing of `MAJOR.MINOR.PATCH[-PRERELEASE][+METADATA]` using the official SemVer regex.

### Correct precedence comparison

Release > pre-release, numeric identifiers compared numerically, longer pre-release wins on tie.

### Familiar BCL surface

Implements `ISpanParsable<SemVer>`, `IComparable<SemVer>`, and `IComparisonOperators<SemVer, SemVer, bool>`.

### `System.Text.Json` support

Out of the box (round-trips through `JsonSerializer`).

### Implicit conversions

To/from `string`, plus interop with `System.Version`.

## Installation

Install from [NuGet](https://www.nuget.org/packages/Invex.SemanticVersion/):

```sh
dotnet add package Invex.SemanticVersion
```

Or via the Package Manager Console:

```powershell
Install-Package Invex.SemanticVersion
```

## Quick start

```csharp
using Invex.SemanticVersion;

// Parse from a string
var version = SemVer.Parse("1.2.3-alpha.1+build.123");

Console.WriteLine(version.Major);        // 1
Console.WriteLine(version.Minor);        // 2
Console.WriteLine(version.Patch);        // 3
Console.WriteLine(version.PreRelease);   // alpha.1
Console.WriteLine(version.Metadata);     // build.123
Console.WriteLine(version.Prefix);       // 1.2.3
Console.WriteLine(version.IsPreRelease); // True

// Compare with natural operators
var stable = SemVer.Parse("2.0.0");
bool isNewer = stable > version;         // True (release > pre-release)
```

## Usage

### Parsing

```csharp
// Throws ArgumentException on invalid input
SemVer v = SemVer.Parse("1.0.0");

// Non-throwing variant
if (SemVer.TryParse("1.0.0-rc.1", out var parsed))
{
    // use parsed
}

// ReadOnlySpan<char> overloads are also available (ISpanParsable<SemVer>)
SemVer fromSpan = SemVer.Parse("1.0.0".AsSpan(), null);
```

### Comparison & precedence

`SemVer` follows SemVer precedence rules:

1. `Major`, `Minor`, and `Patch` are compared numerically.
2. A release version has **higher** precedence than a pre-release of the same core version.
3. Pre-release identifiers are compared dot-by-dot; numeric identifiers compare numerically, others lexically.
4. A longer pre-release chain wins when all leading identifiers are equal.
5. Build metadata is used only as a final tiebreaker.

```csharp
SemVer.Parse("1.0.0")          > SemVer.Parse("1.0.0-alpha");   // True
SemVer.Parse("1.0.0-alpha.10") > SemVer.Parse("1.0.0-alpha.9"); // True (numeric)
SemVer.Parse("1.0.0-alpha.1.2") > SemVer.Parse("1.0.0-alpha.1"); // True (longer)

var list = new List<SemVer> { "2.0.0", "1.0.0", "1.0.0-rc.1" };
list.Sort();   // 1.0.0-rc.1, 1.0.0, 2.0.0
```

Equality and operators (`==`, `!=`, `<`, `<=`, `>`, `>=`) are all supported, along with
`Equals`, `GetHashCode`, and `CompareTo`.

### Range checks

```csharp
var v = SemVer.Parse("1.5.0");

v.IsBetween("1.0.0", "2.0.0");   // True  (exclusive bounds, order-independent)
v.IsBetween("2.0.0", "1.0.0");   // True  (bounds may be supplied in any order)
v.IsBetween("1.0.0", "1.5.0");   // False (upper bound is exclusive)
```

### Implicit string conversion

```csharp
SemVer version = "1.2.3";          // string -> SemVer
string text    = version;          // SemVer -> string ("1.2.3")
```

### JSON serialization

`SemVer` works directly with `System.Text.Json`:

```csharp
var version = SemVer.Parse("1.2.3-alpha+build.1");

string json = JsonSerializer.Serialize(version);
SemVer restored = JsonSerializer.Deserialize<SemVer>(json)!;

restored.ToString(); // "1.2.3-alpha+build.1"
```

### Interop with `System.Version`

```csharp
// SemVer -> System.Version (maps Major.Minor.Patch to Major.Minor.Build)
Version sysVer = SemVer.Parse("3.2.1").ToSystemVersion();

// Optionally guard against losing pre-release / metadata information
SemVer.Parse("1.0.0-alpha").ToSystemVersion(throwIfContainsPreRelease: true); // throws

// System.Version -> SemVer (Build becomes Patch; Revision is ignored by default)
SemVer fromSys = SemVer.FromSystemVersion(new Version(1, 2, 3));
```

### Extracting build numbers

When a pre-release or metadata segment contains exactly one numeric sequence, you can read it directly:

```csharp
SemVer.Parse("1.0.0-rc.42").BuildNumberFromPreRelease;   // 42
SemVer.Parse("1.0.0+build.99").BuildNumberFromMetadata;  // 99
SemVer.Parse("1.0.0-alpha").BuildNumberFromPreRelease;   // 0 (no number)

// Or use the static helper directly
SemVer.ExtractBuildNumber("alpha.123"); // 123
SemVer.ExtractBuildNumber("1.2.3");     // 0  (multiple numbers -> 0)
```

## API overview

| Member                                                  | Description                                                                |
|---------------------------------------------------------|----------------------------------------------------------------------------|
| `Major`, `Minor`, `Patch`                               | Core version components.                                                   |
| `PreRelease`, `Metadata`                                | Optional pre-release and build-metadata segments (nullable).               |
| `Prefix`                                                | `"MAJOR.MINOR.PATCH"` without pre-release/metadata.                        |
| `IsPreRelease`                                          | `true` when a pre-release identifier is present.                           |
| `BuildNumberFromPreRelease` / `BuildNumberFromMetadata` | Single numeric value extracted from the respective segment, otherwise `0`. |
| `SemVer.One`                                            | Convenience instance for `1.0.0`.                                          |
| `Parse` / `TryParse`                                    | String and `ReadOnlySpan<char>` parsing (`ISpanParsable<SemVer>`).         |
| `CompareTo`, `Equals`, comparison operators             | SemVer-compliant precedence and equality.                                  |
| `IsBetween`                                             | Exclusive, order-independent range check.                                  |
| `ToSystemVersion` / `FromSystemVersion`                 | Conversion to and from `System.Version`.                                   |
| `ExtractBuildNumber`                                    | Static helper to pull a single number from a string.                       |

## Target frameworks

- .NET 8.0
- .NET 9.0
- .NET 10.0

## Building & testing

The `_atom` project in this repository is the [DecSm.Atom](https://github.com/DecSm-Atom) build
pipeline used to build, test, and publish the package — it is **not** part of the published library.
For everyday development you can use the standard .NET CLI:

```sh
dotnet build
dotnet test
```

## License

Distributed under the [MIT License](LICENSE.txt).
