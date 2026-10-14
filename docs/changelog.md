# Changelog

All notable changes to `Invex.SemanticVersion` are documented here.  
This project adheres to [Semantic Versioning](https://semver.org/).

---

## [Unreleased]

### Changed

- Pre-release precedence now follows SemVer 2.0.0 §11.4 exactly: numeric identifiers of any length are compared
  numerically (previously identifiers larger than `int.MaxValue` were compared as strings), numeric identifiers always
  have lower precedence than alphanumeric ones (previously `1.0.0-9` > `1.0.0-10a` and `1.0.0-1` > `1.0.0--1`), and
  identifiers such as `-1` are no longer parsed as negative numbers.
- The ordering operators `<`, `<=`, `>` and `>=` are null-safe: `null` precedes every version, so a `null` left
  operand no longer throws `NullReferenceException`.
- `FromSystemVersion` maps an undefined `Version.Build` (`-1`, as in `new Version(1, 2)`) to a `Patch` of `0`
  instead of `-1`, so `ToSystemVersion` no longer throws on the result.
- `BuildNumberFromPreRelease` and `BuildNumberFromMetadata` return `0` instead of throwing `OverflowException` when
  the single number is larger than `int.MaxValue`. `ExtractBuildNumber` still throws in that case.

- Parsing accepts only ASCII digits. Non-ASCII Unicode digits (for example `1٣.0.0`) previously made `Parse` throw
  `FormatException`, or were accepted inside pre-release identifiers. Now `Parse` throws `ArgumentException` and
  `TryParse` returns `false`. `ExtractBuildNumber` and the build number properties ignore non-ASCII digits.

- Parsing rejects a trailing newline (for example `"1.0.0\n"`), which was previously accepted. `Parse` now throws
  `ArgumentException` and `TryParse` returns `false`.

### Deprecated

- The public parameterless constructor `SemVer()`. It always creates `0.0.0`; use `Parse`, `TryParse`,
  `FromSystemVersion` or `One` instead.

### Removed

- The .NET 8.0 and .NET 9.0 targets. The package now targets .NET 10.0 only.
- The unused `System.IO.Abstractions` package dependency.

---

## [1.0.0] — Initial Release

### Added

- `SemVer` sealed class implementing `ISpanParsable<SemVer>`, `IComparable<SemVer>`, and `IComparisonOperators<SemVer, SemVer, bool>`.
- Properties: `Major`, `Minor`, `Patch`, `PreRelease`, `Metadata`, `Prefix`, `IsPreRelease`.
- Static constant `SemVer.One` (`1.0.0`).
- `Parse(string)` — throwing parse from string.
- `TryParse(string?, out SemVer?)` — non-throwing parse from string.
- `Parse(ReadOnlySpan<char>, IFormatProvider?)` — throwing parse from span.
- `TryParse(ReadOnlySpan<char>, IFormatProvider?, out SemVer?)` — non-throwing parse from span.
- `TryParse(ReadOnlySpan<char>, out SemVer?)` — convenience span overload.
- Implicit conversions to/from `string`.
- `CompareTo(SemVer?)` — SemVer 2.0.0 compliant precedence comparison.
- Comparison operators: `==`, `!=`, `<`, `<=`, `>`, `>=`.
- `Equals(SemVer?)` and `Equals(object?)`.
- `GetHashCode()` consistent with `Equals`.
- `ToString()` — canonical `MAJOR.MINOR.PATCH[-PRERELEASE][+METADATA]` output.
- `IsBetween(SemVer, SemVer)` — exclusive, order-independent range check.
- `ToSystemVersion(bool, bool)` — convert to `System.Version` with optional guards.
- `FromSystemVersion(Version, bool)` — convert from `System.Version` with optional guard.
- `BuildNumberFromPreRelease` — extract single build number from pre-release.
- `BuildNumberFromMetadata` — extract single build number from metadata.
- `ExtractBuildNumber(string?)` — static helper to extract a single number from any string.
- `System.Text.Json` serialization support (all components serialized; computed properties excluded).
- Targets .NET 8.0, .NET 9.0, and .NET 10.0.

---

## See also

- [Introduction](introduction.md)
- [Semantic Versioning 2.0.0 specification](https://semver.org/)

[Unreleased]: https://github.com/Invex-Games/semantic-version/compare/v1.0.0...HEAD
[1.0.0]: https://github.com/Invex-Games/semantic-version/releases/tag/v1.0.0

