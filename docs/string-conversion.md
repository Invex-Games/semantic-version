# String Conversion

`SemVer` provides bidirectional implicit conversions with `string`, and a deterministic `ToString()` implementation.

---

## `ToString()`

Returns the canonical SemVer 2.0.0 string representation of the version.

The output format is:

```
MAJOR.MINOR.PATCH[-PRERELEASE][+METADATA]
```

Pre-release and metadata segments are included **only** when they are non-null.

```csharp
SemVer.Parse("1.2.3").ToString();               // "1.2.3"
SemVer.Parse("1.2.3-alpha").ToString();         // "1.2.3-alpha"
SemVer.Parse("1.2.3+build.1").ToString();       // "1.2.3+build.1"
SemVer.Parse("1.2.3-alpha+build.1").ToString(); // "1.2.3-alpha+build.1"
```

`ToString()` is the inverse of `Parse` — a round-trip through `Parse` → `ToString` always returns the original string.

---

## Implicit conversion from `string`

Assigning a `string` literal (or variable) to a `SemVer` variable calls `Parse` implicitly:

```csharp
SemVer version = "1.2.3-rc.1";
```

This is equivalent to:

```csharp
SemVer version = SemVer.Parse("1.2.3-rc.1");
```

> ⚠️ The implicit conversion uses the **throwing** `Parse` path. An invalid string will throw `ArgumentException`. Use `TryParse` if the input may be untrusted.

### Practical uses

```csharp
// Method parameters
void SetMinVersion(SemVer min) { /* ... */ }
SetMinVersion("2.0.0"); // works

// Collections
var allowed = new HashSet<SemVer> { "1.0.0", "1.1.0", "2.0.0" };

// Comparison operators with literals
SemVer current = SemVer.Parse("1.5.0");
bool tooOld = current < "2.0.0"; // true
```

---

## Implicit conversion to `string`

Assigning a `SemVer` to a `string` variable calls `ToString()` implicitly:

```csharp
SemVer version = SemVer.Parse("1.2.3-alpha+build.1");
string text = version; // "1.2.3-alpha+build.1"
```

This is equivalent to:

```csharp
string text = version.ToString();
```

### Practical uses

```csharp
// Storing in a string field
string currentVersion = SemVer.Parse("3.0.0");

// Passing to string-accepting APIs
Console.WriteLine(SemVer.Parse("1.0.0")); // ToString() called automatically
File.WriteAllText("version.txt", SemVer.Parse("1.0.0")); // implicit to string
```

---

## `Prefix`

The `Prefix` property returns only the `MAJOR.MINOR.PATCH` part, omitting any pre-release or metadata:

```csharp
SemVer.Parse("1.2.3-alpha+build.1").Prefix; // "1.2.3"
SemVer.Parse("1.2.3").Prefix;               // "1.2.3"
```

Useful when you need to compare or display only the numeric version core.

