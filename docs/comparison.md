# Comparison & Precedence

`SemVer` implements full SemVer 2.0.0 precedence rules via `IComparable<SemVer>` and `IComparisonOperators<SemVer, SemVer, bool>`.

---

## Precedence rules (summary)

The ordering follows the [SemVer 2.0.0 specification §11](https://semver.org/#spec-item-11):

1. **Major** is compared numerically — higher major wins.
2. **Minor** is compared numerically when major is equal.
3. **Patch** is compared numerically when major and minor are equal.
4. **Release > Pre-release** — when core versions are identical a release version (no pre-release) has higher precedence than any pre-release.
5. **Pre-release identifiers** are compared dot-by-dot left-to-right:
   - If both identifiers are purely numeric they are compared **numerically**.
   - Otherwise they are compared **lexically** (ordinal).
   - A shorter identifier list is *less than* a longer one when all common identifiers are equal.
6. **Build metadata** is **not** considered for precedence by the spec, but this implementation uses it as a **final tiebreaker** (ordinal comparison) when everything else is equal.

---

## `CompareTo`

```csharp
int result = SemVer.Parse("2.0.0").CompareTo(SemVer.Parse("1.0.0")); // > 0
int result = SemVer.Parse("1.0.0").CompareTo(SemVer.Parse("1.0.0")); // 0
int result = SemVer.Parse("1.0.0").CompareTo(SemVer.Parse("2.0.0")); // < 0

// null is always less than any instance
int result = SemVer.Parse("1.0.0").CompareTo(null); // > 0
```

This is compatible with `List<T>.Sort()`, `Array.Sort()`, `SortedSet<T>`, `OrderBy`, etc.

---

## Comparison operators

All six standard comparison operators are supported:

```csharp
SemVer a = "2.0.0";
SemVer b = "1.0.0";

bool gt  = a > b;   // true
bool gte = a >= b;  // true
bool lt  = a < b;   // false
bool lte = a <= b;  // false
bool eq  = a == b;  // false
bool neq = a != b;  // true
```

Null comparisons with `==` and `!=` are safe:

```csharp
SemVer? x = null;
SemVer? y = null;
bool bothNull = (x == y); // true

SemVer? z = SemVer.Parse("1.0.0");
bool leftNull = (x == z);  // false
bool rightNull = (z == x); // false
```

---

## Ordering examples

### Release beats pre-release

```csharp
SemVer.Parse("1.0.0") > SemVer.Parse("1.0.0-alpha")   // true
SemVer.Parse("1.0.0-alpha") < SemVer.Parse("1.0.0")   // true
```

### Numeric pre-release comparison

```csharp
// 10 > 9 numerically (not "10" < "9" lexically)
SemVer.Parse("1.0.0-alpha.10") > SemVer.Parse("1.0.0-alpha.9"); // true
```

### Lexicographic pre-release comparison

```csharp
// 'b' > 'a' — beta is newer than alpha
SemVer.Parse("1.0.0-beta") > SemVer.Parse("1.0.0-alpha"); // true
```

### Longer pre-release wins on tie

```csharp
// "alpha.1.2" has more identifiers than "alpha.1"
SemVer.Parse("1.0.0-alpha.1.2") > SemVer.Parse("1.0.0-alpha.1"); // true
```

### Build metadata as tiebreaker

```csharp
// All version components and pre-release are equal — metadata breaks the tie
SemVer.Parse("1.0.0+build.2") > SemVer.Parse("1.0.0+build.1"); // true
```

---

## Sorting a collection

```csharp
var versions = new List<SemVer>
{
    "2.0.0",
    "1.0.0",
    "1.0.0-rc.1",
    "1.0.0-alpha",
    "1.0.0-alpha.1",
};

versions.Sort();
// Result (ascending): 1.0.0-alpha, 1.0.0-alpha.1, 1.0.0-rc.1, 1.0.0, 2.0.0
```

---

## `Equals` and `GetHashCode`

`Equals` uses `CompareTo` under the hood, so equality respects SemVer rules (including metadata as tiebreaker):

```csharp
SemVer.Parse("1.0.0").Equals(SemVer.Parse("1.0.0"));         // true
SemVer.Parse("1.0.0+a").Equals(SemVer.Parse("1.0.0+b"));     // false (metadata differs)
SemVer.Parse("1.0.0").Equals(SemVer.Parse("1.0.0-alpha"));    // false
```

`GetHashCode` is consistent with `Equals` — two equal `SemVer` instances always produce the same hash code.

