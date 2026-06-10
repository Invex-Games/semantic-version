# Parsing

`SemVer` accepts version strings in the canonical SemVer 2.0.0 format:

```
MAJOR.MINOR.PATCH[-PRERELEASE][+METADATA]
```

Where:
- `MAJOR`, `MINOR`, and `PATCH` are **non-negative integers** with no leading zeros (except `0` itself).
- `PRERELEASE` is an optional dot-separated sequence of alphanumeric identifiers and hyphens.
- `METADATA` is an optional dot-separated sequence of alphanumeric identifiers and hyphens following `+`.

---

## `Parse` — throwing overload

Use `Parse` when you expect the input to always be valid:

```csharp
SemVer v = SemVer.Parse("1.2.3");
SemVer v = SemVer.Parse("1.2.3-alpha");
SemVer v = SemVer.Parse("1.2.3+build.456");
SemVer v = SemVer.Parse("1.2.3-alpha.1+build.456");
```

Throws `ArgumentException` for any string that does not match the SemVer 2.0.0 grammar.

---

## `TryParse` — non-throwing overload

Use `TryParse` when the input may be invalid (user input, config files, etc.):

```csharp
if (SemVer.TryParse("1.0.0-rc.1", out var version))
{
    // version is valid — use it
    Console.WriteLine(version.PreRelease); // rc.1
}
else
{
    // string was not a valid SemVer
}
```

Returns `false` and sets `result` to `null` when parsing fails. Never throws.

---

## `ISpanParsable<SemVer>` — span overloads

All parsing methods also accept `ReadOnlySpan<char>`, avoiding allocations when slicing over larger buffers:

```csharp
ReadOnlySpan<char> span = "1.0.0-beta".AsSpan();

// Throwing
SemVer v = SemVer.Parse(span, null);

// Non-throwing
if (SemVer.TryParse(span, null, out var version))
{
    // ...
}

// Convenience overload without IFormatProvider
if (SemVer.TryParse(span, out var version2))
{
    // ...
}
```

---

## Valid and invalid examples

| Input | Valid? | Notes |
|-------|--------|-------|
| `1.0.0` | ✅ | Minimal release version |
| `0.0.0` | ✅ | Zero version |
| `10.20.30` | ✅ | Large version numbers |
| `1.0.0-alpha` | ✅ | Pre-release |
| `1.0.0+build` | ✅ | Build metadata only |
| `1.0.0-alpha+build` | ✅ | Pre-release + metadata |
| `1.0.0-alpha.1+exp.sha.5114f85` | ✅ | Complex pre-release and metadata |
| `1.0` | ❌ | Missing patch component |
| `1.0.0.0` | ❌ | Four components (not SemVer) |
| `1..0` | ❌ | Empty component |
| `1.0.0-` | ❌ | Empty pre-release |
| `1.0.0+` | ❌ | Empty metadata |
| `-1.0.0` | ❌ | Negative major |
| `a.0.0` | ❌ | Non-numeric major |

---

## Implicit parse via `string` assignment

Because `SemVer` defines an implicit conversion from `string`, you can also write:

```csharp
SemVer v = "1.0.0-rc.1";  // calls Parse internally
```

This uses the throwing `Parse` path — an invalid string will throw `ArgumentException`.  
See [String Conversion](string-conversion.md) for details.

---

## What the parser validates

Under the hood, `SemVer` uses the [official SemVer 2.0.0 regular expression](https://semver.org/#is-there-a-suggested-regular-expression-regex-to-check-a-semver-string):

```
^(0|[1-9]\d*)\.(0|[1-9]\d*)\.(0|[1-9]\d*)
  (?:-((?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*)(?:\.(?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*))*))?
  (?:\+([0-9a-zA-Z-]+(?:\.[0-9a-zA-Z-]+)*))?$
```

This ensures leading zeros are rejected (e.g. `01.0.0` is invalid) and all identifiers contain only the allowed character set.

