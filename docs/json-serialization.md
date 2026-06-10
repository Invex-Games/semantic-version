# JSON Serialization

`SemVer` supports `System.Text.Json` out of the box. No custom converters or configuration are needed.

---

## Serialization

```csharp
using System.Text.Json;
using Invex.SemanticVersion;

var version = SemVer.Parse("1.2.3-alpha+build.1");

string json = JsonSerializer.Serialize(version);
```

Produces the following JSON object:

```json
{
  "major": 1,
  "minor": 2,
  "patch": 3,
  "preRelease": "alpha",
  "metadata": "build.1"
}
```

The JSON representation stores each component as a named field.  
`[JsonIgnore]`-decorated computed properties (`Prefix`, `IsPreRelease`, `BuildNumberFromPreRelease`, `BuildNumberFromMetadata`) are excluded.

---

## Deserialization

```csharp
string json = """{"major":1,"minor":2,"patch":3,"preRelease":"alpha","metadata":"build.1"}""";

SemVer restored = JsonSerializer.Deserialize<SemVer>(json)!;

Console.WriteLine(restored);           // 1.2.3-alpha+build.1
Console.WriteLine(restored.IsPreRelease); // True
```

---

## Round-trip guarantee

A `SemVer` serialized and then deserialized always produces the same version string:

```csharp
var versions = new[]
{
    "1.0.0",
    "1.0.0-alpha",
    "1.0.0+build",
    "1.0.0-alpha+build",
};

foreach (var v in versions)
{
    var original = SemVer.Parse(v);
    var json     = JsonSerializer.Serialize(original);
    var restored = JsonSerializer.Deserialize<SemVer>(json)!;

    Console.WriteLine(original == restored); // True for all
    Console.WriteLine(restored.ToString() == v); // True for all
}
```

---

## Serializing `null`

Nullable `SemVer?` properties serialize and deserialize as `null` in the standard JSON way:

```csharp
class MyConfig
{
    public SemVer? MinVersion { get; set; }
}

var config = new MyConfig { MinVersion = null };
string json = JsonSerializer.Serialize(config);
// {"MinVersion":null}

var restored = JsonSerializer.Deserialize<MyConfig>(json)!;
Console.WriteLine(restored.MinVersion is null); // True
```

---

## Embedding `SemVer` in larger objects

```csharp
class AppSettings
{
    public SemVer Version { get; set; } = SemVer.One;
    public SemVer? RequiredMinVersion { get; set; }
}

var settings = new AppSettings
{
    Version = "2.5.0",
    RequiredMinVersion = "2.0.0",
};

string json = JsonSerializer.Serialize(settings);
// {
//   "Version": {"major":2,"minor":5,"patch":0,"preRelease":null,"metadata":null},
//   "RequiredMinVersion": {"major":2,"minor":0,"patch":0,"preRelease":null,"metadata":null}
// }

AppSettings loaded = JsonSerializer.Deserialize<AppSettings>(json)!;
Console.WriteLine(loaded.Version > loaded.RequiredMinVersion); // True
```

