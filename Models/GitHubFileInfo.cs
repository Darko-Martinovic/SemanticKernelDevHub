namespace SemanticKernelDevHub.Models;

/// <summary>
/// Represents information about a file change in GitHub
/// </summary>
public class GitHubFileInfo
{
    /// <summary>
    /// The name/path of the file
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// The status of the file change (added, modified, removed, renamed)
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Number of lines added
    /// </summary>
    public int Additions { get; set; }

    /// <summary>
    /// Number of lines deleted
    /// </summary>
    public int Deletions { get; set; }

    /// <summary>
    /// Total number of changes (additions + deletions)
    /// </summary>
    public int Changes { get; set; }

    /// <summary>
    /// The patch content showing the diff
    /// </summary>
    public string? Patch { get; set; }

    /// <summary>
    /// Gets the file extension
    /// </summary>
    public string Extension => Path.GetExtension(FileName);

    /// <summary>
    /// Determines the programming language based on file extension
    /// </summary>
    public string DetectedLanguage => Extension.ToLower() switch
    {
        ".cs" => "C#",
        ".vb" => "VB.NET",
        ".sql" => "T-SQL",
        ".js" => "JavaScript",
        ".jsx" => "React",
        ".java" => "Java",
        ".ts" => "TypeScript",
        ".tsx" => "React",
        ".py" => "Python",
        ".cpp" or ".cc" or ".cxx" => "C++",
        ".c" => "C",
        ".h" or ".hpp" => "C/C++ Header",
        ".html" or ".htm" => "HTML",
        ".css" => "CSS",
        ".json" => "JSON",
        ".xml" => "XML",
        ".yml" or ".yaml" => "YAML",
        ".md" => "Markdown",
        ".txt" => "Text",
        _ => "Unknown"
    };

    /// <summary>
    /// Indicates if this is a supported language for code review
    /// </summary>
    public bool IsSupportedForReview => DetectedLanguage switch
    {
        "C#" or "VB.NET" or "T-SQL" or "JavaScript" or "React" or "Java" => true,
        _ => false
    };

    /// <summary>
    /// Gets a formatted status indicator
    /// </summary>
    public string StatusIcon => Status.ToLower() switch
    {
        "added" => "➕",
        "modified" => "📝",
        "removed" => "❌",
        "renamed" => "📋",
        _ => "📄"
    };

    /// <summary>
    /// Returns a formatted string representation of the file change
    /// </summary>
    public override string ToString()
    {
        return $"{StatusIcon} {FileName} (+{Additions}/-{Deletions}) [{DetectedLanguage}]";
    }

    /// <summary>
    /// Gets the actual code content from the patch (removes diff markers)
    /// </summary>
    public string GetCodeContent()
    {
        if (string.IsNullOrEmpty(Patch))
            return string.Empty;

        var lines = Patch.Split('\n');
        var codeLines = lines
            .Where(line => !line.StartsWith("@@") && !line.StartsWith("diff") && !line.StartsWith("index"))
            .Select(line => line.Length > 0 && (line[0] == '+' || line[0] == '-') ? line[1..] : line)
            .Where(line => !string.IsNullOrWhiteSpace(line));

        return string.Join('\n', codeLines);
    }

    /// <summary>
    /// Gets only the added lines from the patch
    /// </summary>
    public List<string> GetAddedLines()
    {
        if (string.IsNullOrEmpty(Patch))
            return new List<string>();

        return Patch.Split('\n')
            .Where(line => line.StartsWith('+') && !line.StartsWith("+++"))
            .Select(line => line[1..])
            .ToList();
    }

    /// <summary>
    /// Gets only the removed lines from the patch
    /// </summary>
    public List<string> GetRemovedLines()
    {
        if (string.IsNullOrEmpty(Patch))
            return new List<string>();

        return Patch.Split('\n')
            .Where(line => line.StartsWith('-') && !line.StartsWith("---"))
            .Select(line => line[1..])
            .ToList();
    }
}
