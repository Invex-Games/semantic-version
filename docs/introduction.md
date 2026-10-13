# Introduction

Invex.SemanticVersion gives .NET code a single immutable type for [Semantic Versioning 2.0.0](https://semver.org/)
versions. This page explains when it fits, the concepts it is built on, and the design choices that affect how
it behaves.

## What it is

Version strings such as `1.4.0-rc.2+build.77` look simple, but comparing them as strings is wrong
(`"1.10.0" < "1.9.0"` lexically) and `System.Version` cannot hold the pre-release or build-metadata parts.
`SemVer` parses these strings once into a typed, immutable value that knows the specification's precedence
rules and fits into the base class library like a built-in type: it is parsable, comparable, sortable,
convertible to and from `string`, and serializable with `System.Text.Json`.

## When to use it

- You read versions from CI variables, GitVersion output, tags, manifests or configuration and need to compare
  them correctly (minimum supported version, "is this newer", sorting release lists).
- You need the pre-release label or a build counter embedded in it, for example `1.0.0-rc.42`.
- You store versions in JSON settings or pass them through APIs that expect `string` or `System.Version`.

## When not to use it

- **Version ranges and floating versions.** There is no range syntax such as `[1.0, 2.0)` or `1.*`;
  [`IsBetween`](range-checks.md) only checks an exclusive range between two concrete versions.
- **Loose or non-SemVer input.** Strings like `v1.2.3`, `1.2` or `1.2.3.4` are rejected. Normalize them first,
  or use [`FromSystemVersion`](system-version-interop.md) for four-part .NET versions.
- **Metadata-insensitive equality.** The specification ignores build metadata for precedence; this library
  uses it as a final tiebreaker, so `1.0.0+a` and `1.0.0+b` are not equal. Compare `Prefix` and `PreRelease`
  yourself if you need the strict specification behavior.

## Key concepts at a glance

| Concept | In one sentence | Read more |
|---|---|---|
| Components | `Major`, `Minor` and `Patch` are integers; `PreRelease` and `Metadata` are optional strings without their `-` / `+` prefix. | [Parsing](parsing.md) |
| Precedence | Core numbers first, then release over pre-release, then pre-release identifiers dot by dot, then metadata. | [Comparison & Precedence](comparison.md) |
| Conversions | Implicit `string` conversions in both directions, plus explicit `System.Version` conversions with optional guards. | [String Conversion](string-conversion.md), [System.Version Interop](system-version-interop.md) |
| Build numbers | A single number embedded in the pre-release or metadata can be read back as an `int`. | [Build Number Extraction](build-numbers.md) |

## Design principles

- **One immutable type.** Everything lives on `SemVer`; all properties are read-only after creation.
- **Strict parsing.** Input is validated with the regular expression suggested by the specification, so
  leading zeros and empty identifiers are rejected rather than silently accepted.
- **BCL-native.** `SemVer` implements `ISpanParsable<SemVer>`, `IComparable<SemVer>` and
  `IComparisonOperators<SemVer, SemVer, bool>`, so generic parsing, `List<T>.Sort()`, `SortedSet<T>` and
  operators work without adapters.
- **Explicit about lossy conversions.** `ToSystemVersion` and `FromSystemVersion` drop data silently by default
  but accept flags that throw instead.
- **Stable public API.** The public surface is locked by a snapshot test, so changes are always deliberate.

## Next steps

- [Getting Started](getting-started.md): install the package and parse your first version.
- [Parsing](parsing.md)
- [Comparison & Precedence](comparison.md)
- [API Reference](../api/index.md)
