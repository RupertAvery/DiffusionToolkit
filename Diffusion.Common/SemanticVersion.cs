using System;
using System.IO;
using System.Text.RegularExpressions;

namespace Diffusion.Common;

/// <summary>
/// A class for parsing and comparing semantic versions
/// </summary>

public class SemanticVersion : IComparable<SemanticVersion>
{
    private static Regex versionRegex = new Regex("v(?<major>\\d+)\\.(?<minor>\\d+)(?:\\.(?<build>\\d+))?");

    public int Major { get; set; }

    public int Minor { get; set; }

    public int Build { get; set; }

    public static bool IsSemanticVersion(string text)
    {
        return versionRegex.IsMatch(text);
    }

    public static SemanticVersion Parse(string text)
    {
        var match = versionRegex.Match(text);

        var version = new SemanticVersion()
        {
            Major = int.Parse(match.Groups["major"].Value),
            Minor = int.Parse(match.Groups["minor"].Value)
        };
        
        if (match.Groups["build"].Success)
        {
            version.Build = int.Parse(match.Groups["build"].Value);
        }

        return version;
    }

    public static bool TryParse(string? text, out SemanticVersion version)
    {
        if (text == null)
        {
            version = new SemanticVersion();
            return false;
        }

        var match = versionRegex.Match(text);
       
        if (match.Success)
        {
            version = new SemanticVersion()
            {
                Major = int.Parse(match.Groups["major"].Value),
                Minor = int.Parse(match.Groups["minor"].Value)
            };

            if (match.Groups["build"].Success)
            {
                version.Build = int.Parse(match.Groups["build"].Value);
            }

            return true;
        }

        version = new SemanticVersion();

        return false;
    }

    public override string ToString()
    {
        return $"v{Major}.{Minor}" + (Build > 0 ? $".{Build}" : "");
    }

    public int CompareTo(SemanticVersion? other)
    {
        if (ReferenceEquals(this, other)) return 0;
        if (ReferenceEquals(null, other)) return 1;
        var majorComparison = Major.CompareTo(other.Major);
        if (majorComparison != 0) return majorComparison;
        var minorComparison = Minor.CompareTo(other.Minor);
        if (minorComparison != 0) return minorComparison;
        return Build.CompareTo(other.Build);
    }

    public static bool operator >(SemanticVersion a, SemanticVersion b)
    {
        return a.CompareTo(b) > 0;
    }

    public static bool operator <(SemanticVersion a, SemanticVersion b)
    {
        return a.CompareTo(b) < 0;
    }
}