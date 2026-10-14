# Invex.SemanticVersion

[![Build](https://github.com/Invex-Games/semantic-version/actions/workflows/Build.yml/badge.svg)](https://github.com/Invex-Games/semantic-version/actions/workflows/Build.yml)
[![NuGet](https://img.shields.io/nuget/v/Invex.SemanticVersion.svg)](https://www.nuget.org/packages/Invex.SemanticVersion)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://github.com/Invex-Games/semantic-version/blob/main/LICENSE.txt)
[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4.svg)](https://dotnet.microsoft.com/)

A small C# implementation of [Semantic Versioning 2.0.0](https://semver.org/).
It provides a single immutable `SemVer` type for **parsing**, **comparing**, **serializing**, and
**converting** version strings of the form `MAJOR.MINOR.PATCH[-PRERELEASE][+METADATA]`.

## Features

- **Spec-compliant parsing.** Parses `MAJOR.MINOR.PATCH[-PRERELEASE][+METADATA]` using the official SemVer regex.
- **Correct precedence comparison.** Release > pre-release, numeric identifiers (up to `int.MaxValue`) compared numerically, longer pre-release wins on tie.
- **Familiar BCL surface.** Implements `ISpanParsable<SemVer>`, `IComparable<SemVer>`, and `IComparisonOperators<SemVer, SemVer, bool>`.
- **`System.Text.Json` support.** Round-trips through `JsonSerializer` out of the box, with no custom converter.
- **Implicit conversions.** Converts to and from `string`, plus interop with `System.Version`.

## Installation

Install from [NuGet](https://www.nuget.org/packages/Invex.SemanticVersion):

```shell
dotnet add package Invex.SemanticVersion
```

The package targets .NET 10.0. It references `JetBrains.Annotations` and
`System.IO.Abstractions`, which NuGet restores automatically.

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

See [Parsing](https://github.com/Invex-Games/semantic-version/blob/main/docs/parsing.md).

### Comparing and sorting

```csharp
SemVer.Parse("1.0.0")           > SemVer.Parse("1.0.0-alpha");   // True
SemVer.Parse("1.0.0-alpha.10")  > SemVer.Parse("1.0.0-alpha.9"); // True (numeric)
SemVer.Parse("1.0.0-alpha.1.2") > SemVer.Parse("1.0.0-alpha.1"); // True (longer)

var list = new List<SemVer> { "2.0.0", "1.0.0", "1.0.0-rc.1" };
list.Sort();   // 1.0.0-rc.1, 1.0.0, 2.0.0
```

Build metadata is used only as a final tiebreaker. See [Comparison & Precedence](https://github.com/Invex-Games/semantic-version/blob/main/docs/comparison.md).

### Range checks

```csharp
var v = SemVer.Parse("1.5.0");

v.IsBetween("1.0.0", "2.0.0");   // True  (exclusive bounds)
v.IsBetween("2.0.0", "1.0.0");   // True  (bounds may be supplied in any order)
v.IsBetween("1.0.0", "1.5.0");   // False (upper bound is exclusive)
```

See [Range Checks](https://github.com/Invex-Games/semantic-version/blob/main/docs/range-checks.md).

### Converting to strings, JSON and `System.Version`

```csharp
SemVer version = "1.2.3";                         // string -> SemVer
string text    = version;                         // SemVer -> string ("1.2.3")

string json = JsonSerializer.Serialize(version);  // System.Text.Json, no converter needed

Version sysVer = SemVer.Parse("3.2.1").ToSystemVersion();     // 3.2.1
SemVer fromSys = SemVer.FromSystemVersion(new Version(1, 2, 3));
```

See [String Conversion](https://github.com/Invex-Games/semantic-version/blob/main/docs/string-conversion.md), [JSON Serialization](https://github.com/Invex-Games/semantic-version/blob/main/docs/json-serialization.md)
and [System.Version Interop](https://github.com/Invex-Games/semantic-version/blob/main/docs/system-version-interop.md).

### Extracting build numbers

```csharp
SemVer.Parse("1.0.0-rc.42").BuildNumberFromPreRelease;   // 42
SemVer.Parse("1.0.0+build.99").BuildNumberFromMetadata;  // 99
SemVer.ExtractBuildNumber("1.2.3");                      // 0 (multiple numbers)
```

See [Build Number Extraction](https://github.com/Invex-Games/semantic-version/blob/main/docs/build-numbers.md).

## Documentation

Full documentation: [https://invex-games.github.io/semantic-version/](https://invex-games.github.io/semantic-version/)

| Guide | Contents |
|---|---|
| [Introduction](https://github.com/Invex-Games/semantic-version/blob/main/docs/introduction.md) | When to use the library, key concepts and design principles |
| [Getting Started](https://github.com/Invex-Games/semantic-version/blob/main/docs/getting-started.md) | Installation and a five-minute quick start |
| [Parsing](https://github.com/Invex-Games/semantic-version/blob/main/docs/parsing.md) | `Parse`, `TryParse`, span overloads and validation rules |
| [Comparison & Precedence](https://github.com/Invex-Games/semantic-version/blob/main/docs/comparison.md) | `CompareTo`, operators and SemVer ordering rules |
| [Range Checks](https://github.com/Invex-Games/semantic-version/blob/main/docs/range-checks.md) | `IsBetween`: exclusive, order-independent ranges |
| [String Conversion](https://github.com/Invex-Games/semantic-version/blob/main/docs/string-conversion.md) | Implicit `string` conversions and `ToString` |
| [JSON Serialization](https://github.com/Invex-Games/semantic-version/blob/main/docs/json-serialization.md) | `System.Text.Json` round-trip support |
| [System.Version Interop](https://github.com/Invex-Games/semantic-version/blob/main/docs/system-version-interop.md) | Converting to and from `System.Version` |
| [Build Number Extraction](https://github.com/Invex-Games/semantic-version/blob/main/docs/build-numbers.md) | `BuildNumberFromPreRelease`, `BuildNumberFromMetadata`, `ExtractBuildNumber` |
| [Changelog](https://github.com/Invex-Games/semantic-version/blob/main/docs/changelog.md) | Release history |
| [API Reference](https://invex-games.github.io/semantic-version/api/index.html) | Every public member |

## Development

The repository requires the .NET 10 SDK. From the repository root:

```shell
dotnet build Invex.SemanticVersion.slnx
dotnet test Invex.SemanticVersion.slnx
```

The `_atom` project is the [DecSm.Atom](https://github.com/DecSm-Atom) build pipeline used to build, test, and
publish the package; it is **not** part of the published library. It also builds the DocFX site:

```shell
atom BuildDocs   # build the site into _site/
atom ServeDocs   # build and serve it locally
```

## License

Distributed under the [MIT License](https://github.com/Invex-Games/semantic-version/blob/main/LICENSE.txt).
