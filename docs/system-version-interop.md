# System.Version Interop

`SemVer` provides two methods for converting between `SemVer` and the built-in `System.Version` type.

---

## Mapping between the two types

| `SemVer` component | `System.Version` component |
|--------------------|---------------------------|
| `Major`            | `Major`                    |
| `Minor`            | `Minor`                    |
| `Patch`            | `Build`                    |
| `PreRelease`       | *(not representable)*      |
| `Metadata`         | *(not representable)*      |
| *(not present)*    | `Revision`                 |

`System.Version` uses `Build` where SemVer uses `Patch`. Pre-release labels and build metadata have no equivalent in `System.Version`.

---

## `ToSystemVersion`

Converts a `SemVer` instance to a `System.Version`:

```csharp
Version sysVer = SemVer.Parse("3.2.1").ToSystemVersion();
// sysVer.Major == 3, sysVer.Minor == 2, sysVer.Build == 1
```

### Guard flags

Pre-release and metadata are silently ignored by default. Pass the guard flags to throw instead:

```csharp
// throws ArgumentException — pre-release data would be lost
SemVer.Parse("1.0.0-alpha").ToSystemVersion(throwIfContainsPreRelease: true);

// throws ArgumentException — metadata would be lost
SemVer.Parse("1.0.0+build.1").ToSystemVersion(throwIfContainsMetadata: true);

// both guards at once
SemVer.Parse("1.0.0-alpha+build").ToSystemVersion(
    throwIfContainsPreRelease: true,
    throwIfContainsMetadata: true);

// no pre-release or metadata — succeeds
System.Version ok = SemVer.Parse("1.0.0").ToSystemVersion(
    throwIfContainsPreRelease: true,
    throwIfContainsMetadata: true);
```

| Parameter | Type | Default | Behavior when `true` |
|-----------|------|---------|----------------------|
| `throwIfContainsPreRelease` | `bool` | `false` | Throws `ArgumentException` if `PreRelease` is non-null |
| `throwIfContainsMetadata`   | `bool` | `false` | Throws `ArgumentException` if `Metadata` is non-null  |

---

## `FromSystemVersion`

Creates a `SemVer` from a `System.Version`:

```csharp
SemVer v = SemVer.FromSystemVersion(new Version(1, 2, 3));
// v.Major == 1, v.Minor == 2, v.Patch == 3
// v.PreRelease == null, v.Metadata == null
```

`System.Version` has a fourth `Revision` component that SemVer has no equivalent for. By default the revision is silently ignored:

```csharp
// revision (4) is ignored
SemVer v = SemVer.FromSystemVersion(new Version(1, 2, 3, 4));
// v.Patch == 3
```

### Guard flag

Pass `throwIfContainsRevision: true` to opt into strict mode:

```csharp
// throws ArgumentException — revision would be lost
SemVer.FromSystemVersion(new Version(1, 2, 3, 4), throwIfContainsRevision: true);

// revision is zero — succeeds
SemVer ok = SemVer.FromSystemVersion(new Version(1, 2, 3, 0), throwIfContainsRevision: true);
```

| Parameter | Type | Default | Behavior when `true` |
|-----------|------|---------|----------------------|
| `throwIfContainsRevision` | `bool` | `false` | Throws `ArgumentException` if `Revision > 0` |

---

## Common patterns

### Loading assembly version as `SemVer`

```csharp
using System.Reflection;

var asm = Assembly.GetExecutingAssembly();
var sysVer = asm.GetName().Version!;
SemVer semVer = SemVer.FromSystemVersion(sysVer);
Console.WriteLine(semVer); // e.g. "2.1.0"
```

### Comparing assembly version against a minimum

```csharp
SemVer current = SemVer.FromSystemVersion(Assembly.GetExecutingAssembly().GetName().Version!);
SemVer minimum = "2.0.0";

if (current < minimum)
    throw new InvalidOperationException($"Assembly version {current} is below the required minimum {minimum}.");
```

