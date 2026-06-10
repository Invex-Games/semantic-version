# Changelog

All notable changes to `Invex.SemanticVersion` are documented here.  
This project adheres to [Semantic Versioning](https://semver.org/).

---

## [Unreleased]

*No unreleased changes yet.*

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

[Unreleased]: https://github.com/invex/semantic-version/compare/v1.0.0...HEAD
[1.0.0]: https://github.com/invex/semantic-version/releases/tag/v1.0.0

