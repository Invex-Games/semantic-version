# Range Checks

`SemVer` provides the `IsBetween` method for checking whether a version falls within an exclusive range.

---

## `IsBetween(SemVer firstBound, SemVer secondBound)`

Returns `true` if the current version is **strictly between** the two bounds (both bounds are exclusive).

```csharp
bool IsBetween(SemVer firstBound, SemVer secondBound)
```

### Basic usage

```csharp
var v = SemVer.Parse("1.5.0");

v.IsBetween("1.0.0", "2.0.0");  // true  — 1.0.0 < 1.5.0 < 2.0.0
v.IsBetween("1.0.0", "1.4.0");  // false — 1.5.0 is not below 1.4.0
v.IsBetween("1.5.0", "2.0.0");  // false — lower bound is exclusive
v.IsBetween("1.0.0", "1.5.0");  // false — upper bound is exclusive
```

### Order-independent bounds

The method accepts bounds in **any order** — the smaller value is automatically treated as the lower bound:

```csharp
var v = SemVer.Parse("1.5.0");

v.IsBetween("2.0.0", "1.0.0");  // true  — same as IsBetween("1.0.0", "2.0.0")
v.IsBetween("1.4.0", "1.0.0");  // false — 1.5.0 > 1.4.0 (upper bound)
```

### Equal bounds

When both bounds are the same version, `IsBetween` returns `true` only if the version exactly equals the bounds:

```csharp
var v = SemVer.Parse("1.0.0");

v.IsBetween("1.0.0", "1.0.0");  // true  — version == both bounds
SemVer.Parse("2.0.0").IsBetween("1.0.0", "1.0.0"); // false
```

---

## Edge cases

| Scenario | Result |
|----------|--------|
| Version equals lower bound | `false` (exclusive) |
| Version equals upper bound | `false` (exclusive) |
| Version between bounds (ascending) | `true` |
| Version between bounds (descending) | `true` |
| Version below both bounds | `false` |
| Version above both bounds | `false` |
| Both bounds equal, version matches | `true` |
| Both bounds equal, version differs | `false` |

---

## Implicit string conversion in range checks

Because `SemVer` supports implicit conversion from `string`, you can write range checks concisely:

```csharp
SemVer current = SemVer.Parse("1.5.0");

if (current.IsBetween("1.0.0", "2.0.0"))
{
    Console.WriteLine("Version is in the 1.x range.");
}
```

---

## Notes

- Both bounds use the same SemVer precedence rules described in [Comparison & Precedence](comparison.md).
- Pre-release and metadata in the bounds are fully respected:

```csharp
var v = SemVer.Parse("1.0.0-beta");

// 1.0.0-alpha < 1.0.0-beta < 1.0.0-rc.1
v.IsBetween("1.0.0-alpha", "1.0.0-rc.1"); // true
```

