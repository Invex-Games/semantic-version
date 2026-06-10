# Invex.SemanticVersion

A small, dependency-free C# library that provides a single immutable `SemVer` type for **parsing**, **comparing**, **serializing**, and **converting** version strings that conform to the [Semantic Versioning 2.0.0](https://semver.org/) specification.

[![NuGet](https://img.shields.io/nuget/v/Invex.SemanticVersion.svg)](https://www.nuget.org/packages/Invex.SemanticVersion/)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](../LICENSE.txt)

---

## What is it?

`Invex.SemanticVersion` gives you a first-class `SemVer` value type that fits naturally into the .NET BCL:

- Implements **`ISpanParsable<SemVer>`**, **`IComparable<SemVer>`**, and **`IComparisonOperators<SemVer, SemVer, bool>`**.
- Works out of the box with **`System.Text.Json`** (round-trip serialization).
- Provides **implicit conversions** to/from `string` so you can drop it into any existing string-based API.
- Includes interop with **`System.Version`** (the legacy .NET version type).
- Targets **.NET 8, 9, and 10** — no third-party runtime dependencies.

---

## In this documentation

| Page | Description |
|------|-------------|
| [Getting Started](getting-started.md) | Installation and a five-minute quick-start |
| [Parsing](parsing.md) | `Parse`, `TryParse`, span overloads |
| [Comparison & Precedence](comparison.md) | `CompareTo`, operators, SemVer ordering rules |
| [Range Checks](range-checks.md) | `IsBetween` — exclusive, order-independent ranges |
| [String Conversion](string-conversion.md) | Implicit `string ↔ SemVer` conversions and `ToString` |
| [JSON Serialization](json-serialization.md) | `System.Text.Json` round-trip support |
| [System.Version Interop](system-version-interop.md) | Converting to/from `System.Version` |
| [Build Number Extraction](build-numbers.md) | `BuildNumberFromPreRelease`, `BuildNumberFromMetadata`, `ExtractBuildNumber` |
| [API Reference](../api/SemVer.md) | Complete member-by-member reference |
| [Changelog](changelog.md) | Release history |

