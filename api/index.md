# API Reference

`Invex.SemanticVersion` exposes a single public type; this reference is generated from its XML documentation.

## Invex.SemanticVersion

| Type | Kind | Description |
|---|---|---|
| [`SemVer`](xref:Invex.SemanticVersion.SemVer) | sealed class | An immutable Semantic Versioning 2.0.0 version with parsing, precedence comparison, `string` and `System.Version` conversions and `System.Text.Json` support. |

## Members at a glance

| Member | Description |
|---|---|
| `Major`, `Minor`, `Patch` | Core version components. |
| `PreRelease`, `Metadata` | Optional pre-release and build-metadata segments (nullable). |
| `Prefix` | `"MAJOR.MINOR.PATCH"` without pre-release/metadata. |
| `IsPreRelease` | `true` when a pre-release identifier is present. |
| `BuildNumberFromPreRelease` / `BuildNumberFromMetadata` | Single numeric value extracted from the respective segment, otherwise `0`. |
| `SemVer.One` | Convenience instance for `1.0.0`. |
| `Parse` / `TryParse` | String and `ReadOnlySpan<char>` parsing (`ISpanParsable<SemVer>`). |
| `CompareTo`, `Equals`, comparison operators | SemVer-compliant precedence and equality. |
| `IsBetween` | Exclusive, order-independent range check. |
| `ToSystemVersion` / `FromSystemVersion` | Conversion to and from `System.Version`. |
| `ExtractBuildNumber` | Static helper to pull a single number from a string. |
| Implicit conversions | `string` to `SemVer` (parses, throws on invalid input) and `SemVer` to `string`. |

## Typical flow

```csharp
using Invex.SemanticVersion;

if (SemVer.TryParse(input, out var version) && version >= "2.0.0")
    Console.WriteLine($"Supported: {version.Prefix}");
```

For task-oriented guides, start with the [Introduction](../docs/introduction.md).
