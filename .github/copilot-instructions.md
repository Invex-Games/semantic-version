# Copilot Instructions

Guidance for AI agents working in **Invex.SemanticVersion** — a small, dependency-free C# library
that provides a single immutable `SemVer` type for parsing, comparing, serializing, and converting
version strings that conform to the [Semantic Versioning 2.0.0](https://semver.org/) specification.
Keep changes focused and defer to the linked docs for detail.

## What's in the repo

| Project | Role | Target frameworks |
|---------|------|-------------------|
| `Invex.SemanticVersion` | The only library: `SemVer` class with parsing, comparison, JSON serialization, and `System.Version` interop | `net8.0;net9.0;net10.0` |
| `Invex.SemanticVersion.Tests` | NUnit test suite covering the full public API | `net8.0;net9.0;net10.0` |

Sources live under `src/`, tests under `tests/`, the Atom build definition under `_atom/`, and the
DocFX documentation site under `docs/` and `api/`.

## Build & language specifics

- **.NET 10 SDK** is required. Both projects multi-target `net8.0;net9.0;net10.0`.
- C# `LangVersion 14`, `ImplicitUsings` and `Nullable` enabled, `TreatWarningsAsErrors` on.
- Global usings live in each project's `_usings.cs` — add shared usings there, not per-file.
- `GenerateDocumentationFile` is on, so **all public members need XML doc comments**.
- Build and test the whole solution:

  ```shell
  dotnet build Invex.SemanticVersion.slnx
  dotnet test Invex.SemanticVersion.slnx
  ```

## Architecture overview

The library is a single sealed partial class — `SemVer` — in `src/Invex.SemanticVersion/SemVer.cs`.

### Key design rules

- **`SemVer` is immutable.** All properties are `{ get; private init; }`. Do not add mutable state.
- **No runtime dependencies.** The published package must remain dependency-free. Analyser-only or
  `PrivateAssets="all"` references are acceptable.
- **Spec compliance.** Parsing uses the official SemVer 2.0.0 regex. Comparison follows §11 of the
  spec. Do not relax either.
- **Build metadata as tiebreaker.** The spec says metadata is ignored for precedence; this
  implementation intentionally uses it as a final tiebreaker in `CompareTo`. Preserve this behaviour.
- **`[GeneratedRegex]`** is used for `SemVerRegex()` and `NumberRegex()` — keep them as
  source-generated partial methods.

### Public surface

Every public member must be annotated with `[PublicAPI]` (JetBrains.Annotations). The in-repo
analyser (`DecSm.Analyzers.PublicApiSurface`) flags anything missing, and warnings are errors.

## Atom workflows

- The GitHub Actions workflow YAML under `.github/workflows/` (`Validate.yml`, `Build.yml`,
  `Dependabot Enable auto-merge.yml`, `Cleanup Prereleases.yml`) is **generated** from the Atom
  build definition in `_atom/IBuild.cs`.
- **Whenever you change anything that affects the workflows** — targets, triggers, options, or
  params/secrets — regenerate the YAML:

  ```shell
  dotnet run --project _atom -- gen
  ```

  Commit the regenerated `.github/workflows/` files alongside your `_atom/` changes; **never
  hand-edit the generated YAML**.
- A drift between `_atom/IBuild.cs` and the committed YAML should be treated as a missing
  `atom gen` run.

## Conventions

- Annotate every new public member with `[PublicAPI]` — warnings are errors.
- Add XML doc comments to all public types and members. Match the existing `<summary>` / `<param>` /
  `<returns>` / `<remarks>` / `<exception>` / `<example>` style.
- Use [Conventional Commits](https://www.conventionalcommits.org/) — the prefix drives versioning:

  | Prefix | Version bump |
  |--------|-------------|
  | `breaking:` / `major:` | Major |
  | `feat:` / `feature:` / `minor:` | Minor |
  | `fix:` / `patch:` | Patch |
  | `semver-none` / `semver-skip` | No bump |

- When adding user-facing features, update the relevant `docs/` page and `README.md`.

## Testing & the Verify workflow

- Tests use **NUnit** with **Shouldly** for fluent assertions.
- A **public API surface snapshot test** (`PublicApiSurfaceTests.cs`) tracks the complete public
  API in `PublicApiTests.VerifyPublicApiSurface.verified.txt`. An unexpected diff there signals an
  unintentional API change — treat it as such and double-check before accepting.
- If the diff is valid (expected new output), accept it:
  1. Overwrite the `*.verified.txt` with the contents of the matching `*.received.txt`.
  2. Delete the `*.received.txt`.
  3. Re-run `dotnet test` to confirm the suite is green.
- The test matrix runs against all three target frameworks (`net8.0`, `net9.0`, `net10.0`). Ensure
  new code compiles and behaves identically across all three.

## Adding a new member to `SemVer`

1. Add the property or method to `SemVer.cs`.
2. Annotate it with `[PublicAPI]` and add XML doc comments.
3. Add `[JsonIgnore]` if it is a computed property that should not appear in JSON serialization.
4. Add unit tests in `SemVerTests.cs` covering happy paths and edge cases.
5. Update `PublicApiTests.VerifyPublicApiSurface.verified.txt` (see the Verify workflow above).
6. Update the relevant `docs/` page and `api/SemVer.md`.

## Defer to the docs

For anything beyond the above, prefer these over duplicating detail:

- [README.md](../README.md) — package overview and quick start.
- [docs/index.md](../docs/index.md) — documentation home with a full table of contents.
- [docs/getting-started.md](../docs/getting-started.md) — installation and first examples.
- [docs/parsing.md](../docs/parsing.md) — all parsing overloads and validation rules.
- [docs/comparison.md](../docs/comparison.md) — SemVer precedence rules and operators.
- [docs/range-checks.md](../docs/range-checks.md) — `IsBetween` in depth.
- [docs/string-conversion.md](../docs/string-conversion.md) — implicit `string ↔ SemVer` conversions.
- [docs/json-serialization.md](../docs/json-serialization.md) — `System.Text.Json` round-trip support.
- [docs/system-version-interop.md](../docs/system-version-interop.md) — converting to/from `System.Version`.
- [docs/build-numbers.md](../docs/build-numbers.md) — `BuildNumberFromPreRelease`, `BuildNumberFromMetadata`, `ExtractBuildNumber`.
- [api/SemVer.md](../api/SemVer.md) — complete member-by-member API reference.

