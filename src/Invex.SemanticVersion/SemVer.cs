namespace Invex.SemanticVersion;

/// <summary>
///     An immutable semantic version following the
///     <see href="https://semver.org/">Semantic Versioning 2.0.0</see> specification.
///     Supports parsing, comparison, and conversion operations for version strings in the format
///     <c>MAJOR.MINOR.PATCH[-PRERELEASE][+METADATA]</c>.
/// </summary>
/// <remarks>
///     <para>This class implements semantic version comparison where:</para>
///     <list type="bullet">
///         <item>Release versions (without pre-release) are greater than pre-release versions</item>
///         <item>Pre-release versions are compared lexicographically by dot-separated identifiers</item>
///         <item>
///             Numeric identifiers in pre-release are compared numerically (of any length) and have lower precedence
///             than alphanumeric identifiers
///         </item>
///         <item>Metadata is used only as a final tiebreaker when all other components are equal</item>
///     </list>
///     <para>
///         Create instances with <see cref="Parse(string)" />, <see cref="TryParse(string, out SemVer)" />,
///         <see cref="FromSystemVersion" /> or the implicit conversion from <see cref="string" />. The public
///         parameterless constructor is obsolete: it always creates version <c>0.0.0</c> and cannot set components.
///     </para>
///     <para>
///         The type implements <see cref="ISpanParsable{TSelf}" />, <see cref="IComparable{T}" /> and
///         <see cref="IComparisonOperators{TSelf, TOther, TResult}" />, so it works with generic parsing, sorting and
///         ordered collections.
///     </para>
///     <para>
///         <c>System.Text.Json</c> serializes a <see cref="SemVer" /> as an object with the <see cref="Major" />,
///         <see cref="Minor" />, <see cref="Patch" />, <see cref="PreRelease" /> and <see cref="Metadata" />
///         properties (null components are written as <c>null</c>) and deserializes it through a private
///         constructor, without a custom converter. The computed properties <see cref="Prefix" />,
///         <see cref="IsPreRelease" />, <see cref="BuildNumberFromPreRelease" /> and
///         <see cref="BuildNumberFromMetadata" /> are not serialized.
///     </para>
/// </remarks>
/// <example>
///     <code>
/// var version1 = SemVer.Parse("1.2.3-alpha.1+build.123");
/// SemVer version2 = "2.0.0";
/// bool isNewer = version2 > version1; // true
/// </code>
/// </example>
[PublicAPI]
public sealed partial class SemVer
    : ISpanParsable<SemVer>, IComparable<SemVer>, IComparisonOperators<SemVer, SemVer, bool>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="SemVer" /> class representing version <c>0.0.0</c>.
    /// </summary>
    /// <remarks>
    ///     This constructor was exposed unintentionally. All properties have private <see langword="init" />
    ///     accessors, so it cannot be used to set components; it always creates version <c>0.0.0</c>.
    /// </remarks>
    [Obsolete(
        "Use SemVer.Parse, SemVer.TryParse, SemVer.FromSystemVersion or SemVer.One instead. This constructor always creates version 0.0.0 and will be removed in a future major version.")]
    public SemVer() { }

    /// <summary>
    ///     Initializes a new instance of the <see cref="SemVer" /> class with the specified version components.
    ///     This constructor is used for JSON deserialization.
    /// </summary>
    /// <param name="major">The major version number.</param>
    /// <param name="minor">The minor version number.</param>
    /// <param name="patch">The patch version number.</param>
    /// <param name="preRelease">The pre-release version identifier, or null if not a pre-release.</param>
    /// <param name="metadata">The build metadata, or null if no metadata is present.</param>
    [JsonConstructor]
    private SemVer(int major, int minor, int patch, string? preRelease, string? metadata)
    {
        Major = major;
        Minor = minor;
        Patch = patch;
        PreRelease = preRelease;
        Metadata = metadata;
    }

    /// <summary>
    ///     Gets the major version number.
    /// </summary>
    /// <value>The major version number, indicating incompatible API changes.</value>
    public int Major { get; private init; }

    /// <summary>
    ///     Gets the minor version number.
    /// </summary>
    /// <value>The minor version number, indicating backwards-compatible functionality additions.</value>
    public int Minor { get; private init; }

    /// <summary>
    ///     Gets the patch version number.
    /// </summary>
    /// <value>The patch version number, indicating backwards-compatible bug fixes.</value>
    public int Patch { get; private init; }

    /// <summary>
    ///     Gets the version prefix in the format "MAJOR.MINOR.PATCH".
    /// </summary>
    /// <value>A string representing the core version without pre-release or metadata components.</value>
    /// <example>
    ///     <code>
    /// SemVer.Parse("1.2.3-alpha+build").Prefix; // "1.2.3"
    /// </code>
    /// </example>
    [JsonIgnore]
    public string Prefix => $"{Major}.{Minor}.{Patch}";

    /// <summary>
    ///     Gets the pre-release version identifier.
    /// </summary>
    /// <value>
    ///     The pre-release identifier string without the leading hyphen, or <see langword="null" /> if this is a
    ///     release version.
    /// </value>
    /// <remarks>
    ///     Pre-release identifiers are dot-separated and may contain alphanumeric characters and hyphens, for example
    ///     <c>alpha.1</c>, <c>rc.2</c> or <c>beta</c>.
    /// </remarks>
    public string? PreRelease { get; private init; }

    /// <summary>
    ///     Gets a value indicating whether this version is a pre-release version.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> if <see cref="PreRelease" /> is not <see langword="null" />; otherwise,
    ///     <see langword="false" />.
    /// </value>
    [JsonIgnore]
    public bool IsPreRelease => PreRelease != null;

    /// <summary>
    ///     Gets the build number extracted from the pre-release identifier.
    /// </summary>
    /// <value>
    ///     The numeric build number if exactly one number is found in the pre-release; otherwise, <c>0</c>
    ///     (including when <see cref="PreRelease" /> is <see langword="null" />).
    /// </value>
    /// <remarks>
    ///     This property applies the same rules as <see cref="ExtractBuildNumber" /> to the pre-release string.
    ///     Only returns a non-zero value if exactly one numeric sequence is present. Unlike
    ///     <see cref="ExtractBuildNumber" />, it never throws: a single numeric sequence greater than
    ///     <see cref="int.MaxValue" /> also returns <c>0</c>.
    /// </remarks>
    /// <example>
    ///     <code>
    /// SemVer.Parse("1.0.0-rc.42").BuildNumberFromPreRelease; // 42
    /// SemVer.Parse("1.0.0-alpha").BuildNumberFromPreRelease; // 0 (no number)
    /// SemVer.Parse("1.0.0-1.2").BuildNumberFromPreRelease;   // 0 (multiple numbers)
    /// SemVer.Parse("1.0.0").BuildNumberFromPreRelease;       // 0 (no pre-release)
    /// </code>
    /// </example>
    [JsonIgnore]
    public int BuildNumberFromPreRelease => ExtractBuildNumberOrZero(PreRelease);

    /// <summary>
    ///     Gets the build metadata.
    /// </summary>
    /// <value>
    ///     The build metadata string without the leading plus sign, or <see langword="null" /> if no metadata is
    ///     present.
    /// </value>
    /// <remarks>
    ///     The specification ignores metadata when determining version precedence. This implementation uses it as a
    ///     final tiebreaker (ordinal string comparison) in <see cref="CompareTo" /> when all other components are
    ///     equal, so versions that differ only in metadata are not equal.
    /// </remarks>
    public string? Metadata { get; private init; }

    /// <summary>
    ///     Gets the build number extracted from the metadata.
    /// </summary>
    /// <value>
    ///     The numeric build number if exactly one number is found in the metadata; otherwise, <c>0</c>
    ///     (including when <see cref="Metadata" /> is <see langword="null" />).
    /// </value>
    /// <remarks>
    ///     This property applies the same rules as <see cref="ExtractBuildNumber" /> to the metadata string.
    ///     Only returns a non-zero value if exactly one numeric sequence is present. Unlike
    ///     <see cref="ExtractBuildNumber" />, it never throws: a single numeric sequence greater than
    ///     <see cref="int.MaxValue" /> (for example a timestamp such as <c>+20261014123045</c>) also returns
    ///     <c>0</c>.
    /// </remarks>
    /// <example>
    ///     <code>
    /// SemVer.Parse("1.0.0+build.99").BuildNumberFromMetadata; // 99
    /// SemVer.Parse("1.0.0+build").BuildNumberFromMetadata;    // 0 (no number)
    /// SemVer.Parse("1.0.0").BuildNumberFromMetadata;          // 0 (no metadata)
    /// </code>
    /// </example>
    [JsonIgnore]
    public int BuildNumberFromMetadata => ExtractBuildNumberOrZero(Metadata);

    /// <summary>
    ///     Gets a semantic version representing version 1.0.0.
    /// </summary>
    /// <value>
    ///     A <see cref="SemVer" /> instance with <see cref="Major" /> set to 1, <see cref="Minor" /> and
    ///     <see cref="Patch" /> set to 0, and no pre-release or metadata.
    /// </value>
    public static SemVer One { get; } = new(1, 0, 0, null, null);

    /// <summary>
    ///     Compares the current instance with another <see cref="SemVer" /> and returns an integer that indicates
    ///     whether the current instance precedes, follows, or occurs in the same position in the sort order.
    /// </summary>
    /// <param name="other">The <see cref="SemVer" /> to compare with this instance.</param>
    /// <returns>
    ///     A value that indicates the relative order of the objects being compared:
    ///     Less than zero if this instance precedes <paramref name="other" />;
    ///     Zero if they are equal;
    ///     Greater than zero if this instance follows <paramref name="other" />.
    /// </returns>
    /// <remarks>
    ///     <para>Comparison follows Semantic Versioning precedence rules:</para>
    ///     <list type="number">
    ///         <item>Compare major, minor, and patch versions numerically</item>
    ///         <item>Pre-release versions have lower precedence than normal versions</item>
    ///         <item>
    ///             Pre-release identifiers are compared left to right: identifiers consisting only of ASCII digits
    ///             are compared numerically (of any length, without overflow), a numeric identifier has lower
    ///             precedence than an alphanumeric one, and alphanumeric identifiers are compared ordinally (ASCII)
    ///         </item>
    ///         <item>When all shared identifiers are equal, the version with more pre-release identifiers is greater</item>
    ///         <item>Metadata is used only as a final tiebreaker (ordinal comparison)</item>
    ///     </list>
    ///     <para>
    ///         Any instance is greater than <see langword="null" />, so this method returns a positive value when
    ///         <paramref name="other" /> is <see langword="null" />.
    ///     </para>
    /// </remarks>
    public int CompareTo(SemVer? other)
    {
        if (other is null)
            return 1;

        var majorComparison = Major.CompareTo(other.Major);

        if (majorComparison != 0)
            return majorComparison;

        var minorComparison = Minor.CompareTo(other.Minor);

        if (minorComparison != 0)
            return minorComparison;

        var patchComparison = Patch.CompareTo(other.Patch);

        if (patchComparison != 0)
            return patchComparison;

        switch (PreRelease, other.PreRelease)
        {
            case (null, not null):
                return 1;
            case (not null, null):
                return -1;
            case (null, null):
                return string.CompareOrdinal(Metadata, other.Metadata);
        }

        var preReleaseParts = PreRelease.Split('.');
        var otherPreReleaseParts = other.PreRelease.Split('.');

        for (var i = 0; i < Math.Min(preReleaseParts.Length, otherPreReleaseParts.Length); i++)
        {
            var preReleasePartComparison = ComparePreReleaseIdentifiers(preReleaseParts[i], otherPreReleaseParts[i]);

            if (preReleasePartComparison != 0)
                return preReleasePartComparison;
        }

        var preReleaseLengthComparison = preReleaseParts.Length.CompareTo(otherPreReleaseParts.Length);

        return preReleaseLengthComparison != 0
            ? preReleaseLengthComparison
            : string.CompareOrdinal(Metadata, other.Metadata);
    }

    /// <summary>
    ///     Determines whether <paramref name="left" /> has higher precedence than <paramref name="right" />.
    /// </summary>
    /// <param name="left">The version to compare, or <see langword="null" />.</param>
    /// <param name="right">The version to compare against, or <see langword="null" />.</param>
    /// <returns>
    ///     <see langword="true" /> if <paramref name="left" /> compared with <paramref name="right" /> using
    ///     <see cref="CompareTo" /> gives a value greater than zero; otherwise, <see langword="false" />.
    /// </returns>
    /// <remarks>
    ///     This operator is null-safe: <see langword="null" /> precedes every version and two
    ///     <see langword="null" /> operands are equal, consistent with <see cref="CompareTo" />.
    /// </remarks>
    public static bool operator >(SemVer? left, SemVer? right) =>
        Compare(left, right) > 0;

    /// <summary>
    ///     Determines whether <paramref name="left" /> has the same or higher precedence than <paramref name="right" />.
    /// </summary>
    /// <param name="left">The version to compare, or <see langword="null" />.</param>
    /// <param name="right">The version to compare against, or <see langword="null" />.</param>
    /// <returns>
    ///     <see langword="true" /> if <paramref name="left" /> compared with <paramref name="right" /> using
    ///     <see cref="CompareTo" /> gives a value greater than or equal to zero; otherwise, <see langword="false" />.
    /// </returns>
    /// <remarks>
    ///     This operator is null-safe: <see langword="null" /> precedes every version and two
    ///     <see langword="null" /> operands are equal, consistent with <see cref="CompareTo" />.
    /// </remarks>
    public static bool operator >=(SemVer? left, SemVer? right) =>
        Compare(left, right) >= 0;

    /// <summary>
    ///     Determines whether <paramref name="left" /> has lower precedence than <paramref name="right" />.
    /// </summary>
    /// <param name="left">The version to compare, or <see langword="null" />.</param>
    /// <param name="right">The version to compare against, or <see langword="null" />.</param>
    /// <returns>
    ///     <see langword="true" /> if <paramref name="left" /> compared with <paramref name="right" /> using
    ///     <see cref="CompareTo" /> gives a value less than zero; otherwise, <see langword="false" />.
    /// </returns>
    /// <remarks>
    ///     This operator is null-safe: <see langword="null" /> precedes every version and two
    ///     <see langword="null" /> operands are equal, consistent with <see cref="CompareTo" />.
    /// </remarks>
    public static bool operator <(SemVer? left, SemVer? right) =>
        Compare(left, right) < 0;

    /// <summary>
    ///     Determines whether <paramref name="left" /> has the same or lower precedence than <paramref name="right" />.
    /// </summary>
    /// <param name="left">The version to compare, or <see langword="null" />.</param>
    /// <param name="right">The version to compare against, or <see langword="null" />.</param>
    /// <returns>
    ///     <see langword="true" /> if <paramref name="left" /> compared with <paramref name="right" /> using
    ///     <see cref="CompareTo" /> gives a value less than or equal to zero; otherwise, <see langword="false" />.
    /// </returns>
    /// <remarks>
    ///     This operator is null-safe: <see langword="null" /> precedes every version and two
    ///     <see langword="null" /> operands are equal, consistent with <see cref="CompareTo" />.
    /// </remarks>
    public static bool operator <=(SemVer? left, SemVer? right) =>
        Compare(left, right) <= 0;

    /// <summary>
    ///     Determines whether two versions are equal.
    /// </summary>
    /// <param name="left">The first version to compare, or <see langword="null" />.</param>
    /// <param name="right">The second version to compare, or <see langword="null" />.</param>
    /// <returns>
    ///     <see langword="true" /> if both are <see langword="null" />, or if <paramref name="left" /> is not
    ///     <see langword="null" /> and <see cref="Equals(SemVer)" /> returns <see langword="true" />; otherwise,
    ///     <see langword="false" />.
    /// </returns>
    /// <remarks>This operator is null-safe and, like <see cref="Equals(SemVer)" />, takes metadata into account.</remarks>
    public static bool operator ==(SemVer? left, SemVer? right) =>
        (left is null && right is null) || left?.Equals(right) == true;

    /// <summary>
    ///     Determines whether two versions are not equal.
    /// </summary>
    /// <param name="left">The first version to compare, or <see langword="null" />.</param>
    /// <param name="right">The second version to compare, or <see langword="null" />.</param>
    /// <returns>
    ///     <see langword="true" /> if the versions are not equal according to the equality operator; otherwise,
    ///     <see langword="false" />.
    /// </returns>
    /// <remarks>This operator is null-safe.</remarks>
    public static bool operator !=(SemVer? left, SemVer? right) =>
        !(left == right);

    /// <summary>
    ///     Parses a string representation of a semantic version.
    /// </summary>
    /// <param name="s">The string to parse.</param>
    /// <param name="provider">Ignored; parsing is culture-independent.</param>
    /// <returns>A <see cref="SemVer" /> equivalent to the version contained in <paramref name="s" />.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="s" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="s" /> is not a valid semantic version string.</exception>
    /// <exception cref="OverflowException">
    ///     Thrown when the major, minor or patch component is greater than <see cref="int.MaxValue" />.
    /// </exception>
    /// <remarks>
    ///     This overload implements <see cref="IParsable{TSelf}" />. Input is validated
    ///     with the regular expression suggested by the Semantic Versioning 2.0.0 specification, so leading zeros in
    ///     numeric components and empty pre-release or metadata segments are rejected.
    /// </remarks>
    public static SemVer Parse(string s, IFormatProvider? provider)
    {
        var match = SemVerRegex()
            .Match(s);

        // ReSharper disable once LocalizableElement
        if (!match.Success)
            throw new ArgumentException($"Invalid version string '{s}'.", nameof(s));

        return new(int.Parse(match.Groups[1].Value),
            int.Parse(match.Groups[2].Value),
            int.Parse(match.Groups[3].Value),
            string.IsNullOrWhiteSpace(match.Groups[4].Value)
                ? null
                : match.Groups[4].Value,
            string.IsNullOrWhiteSpace(match.Groups[5].Value)
                ? null
                : match.Groups[5].Value);
    }

    /// <summary>
    ///     Tries to parse a string representation of a semantic version.
    /// </summary>
    /// <param name="s">The string to parse.</param>
    /// <param name="provider">Ignored; parsing is culture-independent.</param>
    /// <param name="result">
    ///     When this method returns, contains the <see cref="SemVer" /> equivalent to the version contained in
    ///     <paramref name="s" /> if the conversion succeeded, or <see langword="null" /> if it failed.
    /// </param>
    /// <returns>
    ///     <see langword="true" /> if <paramref name="s" /> was converted successfully; otherwise,
    ///     <see langword="false" />.
    /// </returns>
    /// <remarks>
    ///     This overload implements <see cref="IParsable{TSelf}" /> and
    ///     never throws: it returns <see langword="false" /> when <paramref name="s" /> is <see langword="null" />,
    ///     is not a valid semantic version string, or has a major, minor or patch component greater than
    ///     <see cref="int.MaxValue" />.
    /// </remarks>
    public static bool TryParse(
        [NotNullWhen(true)] string? s,
        IFormatProvider? provider,
        [MaybeNullWhen(false)] out SemVer result)
    {
        if (s is null)
        {
            result = null;

            return false;
        }

        var match = SemVerRegex()
            .Match(s);

        if (!match.Success ||
            !int.TryParse(match.Groups[1].Value, out var major) ||
            major < 0 ||
            !int.TryParse(match.Groups[2].Value, out var minor) ||
            minor < 0 ||
            !int.TryParse(match.Groups[3].Value, out var patch) ||
            patch < 0)
        {
            result = null;

            return false;
        }

        var preRelease = match.Groups[4].Value;
        var metadata = match.Groups[5].Value;

        result = new(major,
            minor,
            patch,
            string.IsNullOrWhiteSpace(preRelease)
                ? null
                : preRelease,
            string.IsNullOrWhiteSpace(metadata)
                ? null
                : metadata);

        return true;
    }

    /// <summary>
    ///     Parses a span of characters representing a semantic version.
    /// </summary>
    /// <param name="s">The span of characters to parse.</param>
    /// <param name="provider">Ignored; parsing is culture-independent.</param>
    /// <returns>A <see cref="SemVer" /> equivalent to the version contained in <paramref name="s" />.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="s" /> is not a valid semantic version string.</exception>
    /// <exception cref="OverflowException">
    ///     Thrown when the major, minor or patch component is greater than <see cref="int.MaxValue" />.
    /// </exception>
    /// <remarks>
    ///     This overload implements <see cref="ISpanParsable{TSelf}" />.
    ///     The span is converted to a string and parsed with <see cref="Parse(string, IFormatProvider)" />.
    /// </remarks>
    public static SemVer Parse(ReadOnlySpan<char> s, IFormatProvider? provider) =>
        Parse(s.ToString(), provider);

    /// <summary>
    ///     Tries to parse a span of characters representing a semantic version.
    /// </summary>
    /// <param name="s">The span of characters to parse.</param>
    /// <param name="provider">Ignored; parsing is culture-independent.</param>
    /// <param name="result">
    ///     When this method returns, contains the <see cref="SemVer" /> equivalent to the version contained in
    ///     <paramref name="s" /> if the conversion succeeded, or <see langword="null" /> if it failed.
    /// </param>
    /// <returns>
    ///     <see langword="true" /> if <paramref name="s" /> was converted successfully; otherwise,
    ///     <see langword="false" />.
    /// </returns>
    /// <remarks>
    ///     This overload implements <see cref="ISpanParsable{TSelf}" />. The span is converted to a string and parsed
    ///     with <see cref="TryParse(string, IFormatProvider, out SemVer)" />.
    /// </remarks>
    public static bool TryParse(
        ReadOnlySpan<char> s,
        IFormatProvider? provider,
        [MaybeNullWhen(false)] out SemVer result) =>
        TryParse(s.ToString(), provider, out result);

    /// <summary>
    ///     Determines whether the current version is between two specified versions (exclusive).
    /// </summary>
    /// <param name="firstBound">The first boundary version.</param>
    /// <param name="secondBound">The second boundary version.</param>
    /// <returns>
    ///     <see langword="true" /> if this version is strictly between the two bounds; otherwise,
    ///     <see langword="false" />.
    /// </returns>
    /// <remarks>
    ///     <para>
    ///         Both bounds are exclusive and may be supplied in any order; the lower one is determined with the same
    ///         precedence rules as <see cref="CompareTo" />, so pre-release and metadata in the bounds are respected.
    ///     </para>
    ///     <para>
    ///         If both bounds are equal, returns <see langword="true" /> only if this version equals the bounds.
    ///     </para>
    /// </remarks>
    /// <example>
    ///     <code>
    /// var version = SemVer.Parse("1.5.0");
    /// version.IsBetween("1.0.0", "2.0.0"); // true
    /// version.IsBetween("2.0.0", "1.0.0"); // true (bounds in any order)
    /// version.IsBetween("1.0.0", "1.5.0"); // false (bounds are exclusive)
    /// </code>
    /// </example>
    public bool IsBetween(SemVer firstBound, SemVer secondBound) =>
        firstBound == secondBound
            ? Equals(firstBound)
            : firstBound < secondBound
                ? firstBound < this && this < secondBound
                : secondBound < this && this < firstBound;

    /// <summary>
    ///     Converts this semantic version to a <see cref="System.Version" /> instance.
    /// </summary>
    /// <param name="throwIfContainsPreRelease">
    ///     If <see langword="true" />, throws an exception when pre-release data is present. Defaults to
    ///     <see langword="false" />.
    /// </param>
    /// <param name="throwIfContainsMetadata">
    ///     If <see langword="true" />, throws an exception when metadata is present. Defaults to
    ///     <see langword="false" />.
    /// </param>
    /// <returns>A <see cref="System.Version" /> with Major, Minor, and Build (from Patch) components.</returns>
    /// <exception cref="ArgumentException">
    ///     Thrown when <paramref name="throwIfContainsPreRelease" /> is <see langword="true" /> and pre-release data
    ///     is present, or when <paramref name="throwIfContainsMetadata" /> is <see langword="true" /> and metadata is
    ///     present.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    ///     Thrown when <see cref="Major" />, <see cref="Minor" /> or <see cref="Patch" /> is negative, which can only
    ///     happen for an instance deserialized from JSON containing negative components.
    /// </exception>
    /// <remarks>
    ///     Maps SemVer Major.Minor.Patch to System.Version Major.Minor.Build.
    ///     Pre-release and metadata information is lost in the conversion.
    /// </remarks>
    public Version ToSystemVersion(bool throwIfContainsPreRelease = false, bool throwIfContainsMetadata = false) =>
        throwIfContainsPreRelease && PreRelease is not null
            ? throw new ArgumentException(
                "The SemVer contains a pre-release tag, which is not supported by System.Version.")
            : throwIfContainsMetadata && Metadata is not null
                ? throw new ArgumentException("The SemVer contains metadata, which is not supported by System.Version.")
                : new(Major, Minor, Patch);

    /// <summary>
    ///     Creates a <see cref="SemVer" /> instance from a <see cref="System.Version" />.
    /// </summary>
    /// <param name="version">The <see cref="System.Version" /> to convert.</param>
    /// <param name="throwIfContainsRevision">
    ///     If <see langword="true" />, throws an exception when the version has a revision greater than zero.
    ///     Defaults to <see langword="false" />.
    /// </param>
    /// <returns>
    ///     A <see cref="SemVer" /> with Major, Minor, and Patch (from Build) components and no pre-release or
    ///     metadata.
    /// </returns>
    /// <exception cref="ArgumentException">
    ///     Thrown when <paramref name="throwIfContainsRevision" /> is <see langword="true" /> and
    ///     <see cref="Version.Revision" /> is greater than zero.
    /// </exception>
    /// <remarks>
    ///     <para>
    ///         Maps System.Version Major.Minor.Build to SemVer Major.Minor.Patch.
    ///         The revision component is ignored unless <paramref name="throwIfContainsRevision" /> is
    ///         <see langword="true" />.
    ///     </para>
    ///     <para>
    ///         An undefined <see cref="Version.Build" /> (<c>-1</c>, as in <c>new Version(1, 2)</c>) maps to a
    ///         <see cref="Patch" /> of <c>0</c>, so the result is always a valid version that
    ///         <see cref="ToSystemVersion" /> can convert back.
    ///     </para>
    /// </remarks>
    public static SemVer FromSystemVersion(Version version, bool throwIfContainsRevision = false) =>
        throwIfContainsRevision && version.Revision > 0
            ? throw new ArgumentException("The version contains a revision number, which is not supported by SemVer.")
            : new(version.Major, version.Minor, Math.Max(version.Build, 0), null, null);

    /// <summary>
    ///     Extracts a single numeric value from the specified input string.
    /// </summary>
    /// <param name="input">The string to extract a number from.</param>
    /// <returns>
    ///     The numeric value if exactly one sequence of digits is found; otherwise, <c>0</c>.
    ///     Returns <c>0</c> if the input is <see langword="null" />, empty, whitespace, or contains zero or multiple
    ///     numbers.
    /// </returns>
    /// <exception cref="OverflowException">
    ///     Thrown when the single sequence of digits is greater than <see cref="int.MaxValue" />.
    /// </exception>
    /// <remarks>
    ///     This method is used by <see cref="BuildNumberFromPreRelease" /> and <see cref="BuildNumberFromMetadata" />
    ///     to extract build numbers from version components. It can be called on any string.
    /// </remarks>
    /// <example>
    ///     <code>
    /// SemVer.ExtractBuildNumber("alpha.123");   // 123
    /// SemVer.ExtractBuildNumber("SNAPSHOT-12"); // 12
    /// SemVer.ExtractBuildNumber("beta");        // 0
    /// SemVer.ExtractBuildNumber("1.2.3");       // 0 (multiple numbers)
    /// SemVer.ExtractBuildNumber(null);          // 0
    /// </code>
    /// </example>
    public static int ExtractBuildNumber(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return 0;

        var matches = NumberRegex()
            .Matches(input);

        return matches.Count is 1
            ? int.Parse(matches[0].Value)
            : 0;
    }

    /// <summary>
    ///     Parses a string representation of a semantic version.
    /// </summary>
    /// <param name="s">The string to parse.</param>
    /// <returns>A <see cref="SemVer" /> equivalent to the version contained in <paramref name="s" />.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="s" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="s" /> is not a valid semantic version string.</exception>
    /// <exception cref="OverflowException">
    ///     Thrown when the major, minor or patch component is greater than <see cref="int.MaxValue" />.
    /// </exception>
    /// <remarks>
    ///     Accepts version strings in the format <c>MAJOR.MINOR.PATCH[-PRERELEASE][+METADATA]</c>
    ///     where MAJOR, MINOR, and PATCH are non-negative integers without leading zeros. Use
    ///     <see cref="TryParse(string, out SemVer)" /> when the input may be invalid.
    /// </remarks>
    /// <example>
    ///     <code>
    /// var version = SemVer.Parse("1.2.3-alpha.1+build.456");
    /// </code>
    /// </example>
    public static SemVer Parse(string s) =>
        Parse(s, null);

    /// <summary>
    ///     Tries to parse a string representation of a semantic version.
    /// </summary>
    /// <param name="s">The string to parse.</param>
    /// <param name="result">
    ///     When this method returns, contains the <see cref="SemVer" /> equivalent to the version contained in
    ///     <paramref name="s" /> if the conversion succeeded, or <see langword="null" /> if it failed.
    /// </param>
    /// <returns>
    ///     <see langword="true" /> if <paramref name="s" /> was converted successfully; otherwise,
    ///     <see langword="false" />.
    /// </returns>
    /// <remarks>
    ///     Never throws; returns <see langword="false" /> for invalid input, including components greater than
    ///     <see cref="int.MaxValue" />.
    /// </remarks>
    public static bool TryParse([NotNullWhen(true)] string? s, [MaybeNullWhen(false)] out SemVer result) =>
        TryParse(s, null, out result);

    /// <summary>
    ///     Tries to parse a span of characters representing a semantic version.
    /// </summary>
    /// <param name="s">The span of characters to parse.</param>
    /// <param name="result">
    ///     When this method returns, contains the <see cref="SemVer" /> equivalent to the version contained in
    ///     <paramref name="s" /> if the conversion succeeded, or <see langword="null" /> if it failed.
    /// </param>
    /// <returns>
    ///     <see langword="true" /> if <paramref name="s" /> was converted successfully; otherwise,
    ///     <see langword="false" />.
    /// </returns>
    /// <remarks>
    ///     Never throws; returns <see langword="false" /> for invalid input, including components greater than
    ///     <see cref="int.MaxValue" />.
    /// </remarks>
    public static bool TryParse(ReadOnlySpan<char> s, [MaybeNullWhen(false)] out SemVer result) =>
        TryParse(s.ToString(), out result);

    /// <summary>
    ///     Determines whether the specified <see cref="SemVer" /> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="SemVer" /> to compare with the current instance.</param>
    /// <returns>
    ///     <see langword="true" /> if <see cref="CompareTo" /> returns zero, meaning all components including
    ///     <see cref="Metadata" /> are equal; otherwise, <see langword="false" /> (including when
    ///     <paramref name="other" /> is <see langword="null" />).
    /// </returns>
    public bool Equals(SemVer? other) =>
        CompareTo(other) == 0;

    /// <summary>
    ///     Determines whether the specified object is equal to the current instance.
    /// </summary>
    /// <param name="obj">The object to compare with the current instance.</param>
    /// <returns>
    ///     <see langword="true" /> if <paramref name="obj" /> is a <see cref="SemVer" /> equal to the current instance
    ///     according to <see cref="Equals(SemVer)" />; otherwise, <see langword="false" />.
    /// </returns>
    public override bool Equals(object? obj) =>
        obj is SemVer semVer && Equals(semVer);

    /// <summary>
    ///     Serves as the default hash function.
    /// </summary>
    /// <returns>
    ///     A hash code computed from <see cref="Major" />, <see cref="Minor" />, <see cref="Patch" />,
    ///     <see cref="PreRelease" /> and <see cref="Metadata" />, consistent with <see cref="Equals(SemVer)" />.
    /// </returns>
    public override int GetHashCode() =>
        HashCode.Combine(Major, Minor, Patch, PreRelease, Metadata);

    /// <summary>
    ///     Converts the string representation of a semantic version to its <see cref="SemVer" /> equivalent.
    /// </summary>
    /// <param name="s">The string representation of the semantic version.</param>
    /// <returns>A <see cref="SemVer" /> object equivalent to the version contained in <paramref name="s" />.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="s" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="s" /> is not a valid semantic version string.</exception>
    /// <exception cref="OverflowException">
    ///     Thrown when the major, minor or patch component is greater than <see cref="int.MaxValue" />.
    /// </exception>
    /// <remarks>
    ///     Calls <see cref="Parse(string)" />, so invalid input throws. Use <see cref="TryParse(string, out SemVer)" />
    ///     when the input may be untrusted.
    /// </remarks>
    /// <example>
    ///     <code>
    /// SemVer version = "1.2.3-rc.1";
    /// </code>
    /// </example>
    public static implicit operator SemVer(string s) =>
        Parse(s);

    /// <summary>
    ///     Converts a <see cref="SemVer" /> to its string representation.
    /// </summary>
    /// <param name="semVer">The <see cref="SemVer" /> to convert. Must not be <see langword="null" />.</param>
    /// <returns>The string representation of the semantic version, as returned by <see cref="ToString" />.</returns>
    /// <example>
    ///     <code>
    /// string text = SemVer.Parse("1.2.3"); // "1.2.3"
    /// </code>
    /// </example>
    public static implicit operator string(SemVer semVer) =>
        semVer.ToString();

    /// <summary>
    ///     Returns the string representation of this semantic version.
    /// </summary>
    /// <returns>
    ///     A string in the format <c>MAJOR.MINOR.PATCH[-PRERELEASE][+METADATA]</c>,
    ///     where the pre-release and metadata components are included only if present.
    /// </returns>
    /// <remarks>
    ///     For a version obtained by parsing, the result round-trips: parsing it again with
    ///     <see cref="Parse(string)" /> produces an equal version.
    /// </remarks>
    /// <example>
    ///     <code>
    /// SemVer.Parse("1.2.3").ToString();               // "1.2.3"
    /// SemVer.Parse("1.2.3-alpha").ToString();         // "1.2.3-alpha"
    /// SemVer.Parse("1.2.3+build.1").ToString();       // "1.2.3+build.1"
    /// SemVer.Parse("1.2.3-alpha+build.1").ToString(); // "1.2.3-alpha+build.1"
    /// </code>
    /// </example>
    public override string ToString() =>
        (PreRelease, Metadata) switch
        {
            (not null, not null) => $"{Major}.{Minor}.{Patch}-{PreRelease}+{Metadata}",
            (not null, null) => $"{Major}.{Minor}.{Patch}-{PreRelease}",
            (null, not null) => $"{Major}.{Minor}.{Patch}+{Metadata}",
            _ => $"{Major}.{Minor}.{Patch}",
        };

    /// <summary>
    ///     Compares two versions, treating <see langword="null" /> as lower than every version.
    /// </summary>
    private static int Compare(SemVer? left, SemVer? right) =>
        left is null
            ? right is null
                ? 0
                : -1
            : left.CompareTo(right);

    /// <summary>
    ///     Compares two pre-release identifiers following Semantic Versioning 2.0.0 §11.4: identifiers consisting only
    ///     of ASCII digits are compared numerically (by length, then ordinally, so any length works without
    ///     overflow), numeric identifiers have lower precedence than alphanumeric ones, and alphanumeric identifiers
    ///     are compared ordinally.
    /// </summary>
    private static int ComparePreReleaseIdentifiers(string identifier, string otherIdentifier)
    {
        var isNumeric = IsNumericIdentifier(identifier);
        var otherIsNumeric = IsNumericIdentifier(otherIdentifier);

        return (isNumeric, otherIsNumeric) switch
        {
            (true, true) => identifier.Length != otherIdentifier.Length
                ? identifier.Length.CompareTo(otherIdentifier.Length)
                : string.CompareOrdinal(identifier, otherIdentifier),
            (true, false) => -1,
            (false, true) => 1,
            _ => string.CompareOrdinal(identifier, otherIdentifier),
        };
    }

    private static bool IsNumericIdentifier(string identifier) =>
        identifier.Length > 0 && identifier.All(char.IsAsciiDigit);

    /// <summary>
    ///     Extracts a single numeric value like <see cref="ExtractBuildNumber" />, but returns <c>0</c> instead of
    ///     throwing when the value is greater than <see cref="int.MaxValue" />.
    /// </summary>
    private static int ExtractBuildNumberOrZero(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return 0;

        var matches = NumberRegex()
            .Matches(input);

        return matches.Count is 1 && int.TryParse(matches[0].Value, out var buildNumber)
            ? buildNumber
            : 0;
    }

    [GeneratedRegex(
        @"^(0|[1-9]\d*)\.(0|[1-9]\d*)\.(0|[1-9]\d*)(?:-((?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*)(?:\.(?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*))*))?(?:\+([0-9a-zA-Z-]+(?:\.[0-9a-zA-Z-]+)*))?$")]
    private static partial Regex SemVerRegex();

    [GeneratedRegex(@"\d+")]
    private static partial Regex NumberRegex();
}
