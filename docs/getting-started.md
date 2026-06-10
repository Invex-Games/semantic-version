# Getting Started

This guide walks you from installation to your first working code in five minutes.

---

## Requirements

| Target Framework | Minimum SDK |
|-----------------|-------------|
| .NET 8.0        | .NET SDK 8  |
| .NET 9.0        | .NET SDK 9  |
| .NET 10.0       | .NET SDK 10 |

No additional runtime dependencies are required.

---

## Installation

### .NET CLI (recommended)

```sh
dotnet add package Invex.SemanticVersion
```

### Package Manager Console (Visual Studio)

```powershell
Install-Package Invex.SemanticVersion
```

### PackageReference (csproj)

```xml
<PackageReference Include="Invex.SemanticVersion" Version="*" />
```

> Replace `*` with the specific version you want to pin, e.g. `1.0.0`.

---

## Namespace

Everything lives in a single namespace:

```csharp
using Invex.SemanticVersion;
```

---

## Quick Start

```csharp
using Invex.SemanticVersion;

// ── Parsing ──────────────────────────────────────────────────────────────
var version = SemVer.Parse("1.2.3-alpha.1+build.123");

Console.WriteLine(version.Major);        // 1
Console.WriteLine(version.Minor);        // 2
Console.WriteLine(version.Patch);        // 3
Console.WriteLine(version.PreRelease);   // alpha.1
Console.WriteLine(version.Metadata);     // build.123
Console.WriteLine(version.Prefix);       // 1.2.3
Console.WriteLine(version.IsPreRelease); // True
Console.WriteLine(version);             // 1.2.3-alpha.1+build.123

// ── Safe (non-throwing) parse ─────────────────────────────────────────────
if (SemVer.TryParse("2.0.0-rc.1", out var candidate))
    Console.WriteLine(candidate); // 2.0.0-rc.1

// ── Comparison ───────────────────────────────────────────────────────────
var stable  = SemVer.Parse("2.0.0");
var prerelease = SemVer.Parse("2.0.0-rc.1");

Console.WriteLine(stable > prerelease); // True — release beats pre-release

var versions = new List<SemVer> { "2.0.0", "1.0.0", "1.0.0-rc.1" };
versions.Sort();
// Result: 1.0.0-rc.1, 1.0.0, 2.0.0

// ── Implicit string conversion ────────────────────────────────────────────
SemVer v       = "3.1.0";   // string → SemVer
string text    = v;         // SemVer → string ("3.1.0")

// ── Range check ───────────────────────────────────────────────────────────
var v150 = SemVer.Parse("1.5.0");
Console.WriteLine(v150.IsBetween("1.0.0", "2.0.0")); // True (exclusive)

// ── System.Version interop ────────────────────────────────────────────────
System.Version sysVer = SemVer.Parse("3.2.1").ToSystemVersion(); // 3.2.1
SemVer fromSys = SemVer.FromSystemVersion(new System.Version(1, 2, 3)); // 1.2.3
```

---

## Next steps

- **[Parsing](parsing.md)** — all parsing overloads and validation rules.
- **[Comparison & Precedence](comparison.md)** — how SemVer ordering works.
- **[API Reference](../api/SemVer.md)** — every property and method documented.

