# Build Number Extraction

`SemVer` provides helpers to extract a single numeric value from a pre-release or metadata string.
This is useful when CI/CD systems embed a build counter in a pre-release tag (e.g. `1.0.0-rc.42`) and you need that number programmatically.

---

## `BuildNumberFromPreRelease`

Returns the build number embedded in the pre-release identifier.

```csharp
SemVer.Parse("1.0.0-rc.42").BuildNumberFromPreRelease;   // 42
SemVer.Parse("1.0.0-alpha.1227").BuildNumberFromPreRelease; // 1227
SemVer.Parse("1.0.0-SNAPSHOT-7").BuildNumberFromPreRelease; // 7
```

Returns `0` when:
- There is no pre-release identifier (`PreRelease == null`).
- The pre-release contains **no** numeric sequence.
- The pre-release contains **more than one** numeric sequence (ambiguous).

```csharp
SemVer.Parse("1.0.0").BuildNumberFromPreRelease;          // 0 — no pre-release
SemVer.Parse("1.0.0-alpha").BuildNumberFromPreRelease;    // 0 — no number
SemVer.Parse("1.0.0-1.2.3").BuildNumberFromPreRelease;   // 0 — multiple numbers
```

---

## `BuildNumberFromMetadata`

Identical behaviour, but reads from the **metadata** segment instead:

```csharp
SemVer.Parse("1.0.0+build.99").BuildNumberFromMetadata;  // 99
SemVer.Parse("1.0.0+build.1").BuildNumberFromMetadata;   // 1
```

Returns `0` in the same edge cases as `BuildNumberFromPreRelease`.

```csharp
SemVer.Parse("1.0.0").BuildNumberFromMetadata;           // 0 — no metadata
SemVer.Parse("1.0.0+sha.abc").BuildNumberFromMetadata;   // 0 — no number
SemVer.Parse("1.0.0+1.2").BuildNumberFromMetadata;       // 0 — multiple numbers
```

---

## `ExtractBuildNumber(string? input)` — static helper

The static `ExtractBuildNumber` method is the underlying implementation used by both properties above.  
You can call it directly on any arbitrary string:

```csharp
SemVer.ExtractBuildNumber("rc.42");          // 42
SemVer.ExtractBuildNumber("alpha.123");      // 123
SemVer.ExtractBuildNumber("SNAPSHOT-12");    // 12
SemVer.ExtractBuildNumber("7");              // 7

SemVer.ExtractBuildNumber("beta");           // 0 — no number
SemVer.ExtractBuildNumber("alpha.beta");     // 0 — no number
SemVer.ExtractBuildNumber("1.2.3");          // 0 — multiple numbers
SemVer.ExtractBuildNumber("alpha3.4valid");  // 0 — multiple numbers

SemVer.ExtractBuildNumber(null);             // 0
SemVer.ExtractBuildNumber("");               // 0
SemVer.ExtractBuildNumber("   ");            // 0
```

### Rules

| Condition | Return value |
|-----------|-------------|
| `null`, empty, or whitespace | `0` |
| Exactly one numeric sequence found | That number |
| Zero or more than one numeric sequence | `0` |

---

## Practical example

A CI pipeline writes a version like `1.0.0-rc.{BUILD_NUMBER}`. After reading the version from a config file you can extract the build counter back out:

```csharp
var version = SemVer.Parse("1.0.0-rc.42");

int buildNumber = version.BuildNumberFromPreRelease; // 42
Console.WriteLine($"This is release candidate build #{buildNumber}");
```

Or when the build number lives in metadata:

```csharp
var version = SemVer.Parse("2.3.0+build.1337");

int buildNumber = version.BuildNumberFromMetadata; // 1337
```

