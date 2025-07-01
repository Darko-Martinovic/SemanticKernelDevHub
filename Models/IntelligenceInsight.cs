using System.ComponentModel;
using System.Text.Json.Serialization;

namespace SemanticKernelDevHub.Models;

/// <summary>
/// Represents a cross-referenced insight that connects information from multiple sources
/// </summary>
public class IntelligenceInsight
{
    /// <summary>
    /// Unique identifier for this insight
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Title of the insight
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Detailed description of the insight
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Type of insight (correlation, pattern, anomaly, recommendation)
    /// </summary>
    public InsightType Type { get; set; }

    /// <summary>
    /// Confidence level of this insight (0.0 to 1.0)
    /// </summary>
    public double Confidence { get; set; }

    /// <summary>
    /// Priority level of this insight
    /// </summary>
    public InsightPriority Priority { get; set; }

    /// <summary>
    /// Sources that contributed to this insight
    /// </summary>
    public List<InsightSource> Sources { get; set; } = new();

    /// <summary>
    /// Related entities (tickets, commits, meetings, etc.)
    /// </summary>
    public List<string> RelatedEntities { get; set; } = new();

    /// <summary>
    /// Tags for categorization
    /// </summary>
    public List<string> Tags { get; set; } = new();

    /// <summary>
    /// When this insight was generated
    /// </summary>
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Impact assessment of this insight
    /// </summary>
    public string Impact { get; set; } = string.Empty;

    /// <summary>
    /// Recommended actions based on this insight
    /// </summary>
    public List<string> RecommendedActions { get; set; } = new();

    /// <summary>
    /// Cross-reference connections to other insights
    /// </summary>
    public List<string> CrossReferences { get; set; } = new();
}

/// <summary>
/// Source that contributed to an insight
/// </summary>
public class InsightSource
{
    /// <summary>
    /// Type of source (CodeReview, Meeting, JiraTicket, etc.)
    /// </summary>
    public string SourceType { get; set; } = string.Empty;

    /// <summary>
    /// Identifier of the source
    /// </summary>
    public string SourceId { get; set; } = string.Empty;

    /// <summary>
    /// Human-readable reference
    /// </summary>
    public string SourceReference { get; set; } = string.Empty;

    /// <summary>
    /// Relevance weight (0.0 to 1.0)
    /// </summary>
    public double Relevance { get; set; }

    /// <summary>
    /// Specific content that contributed to the insight
    /// </summary>
    public string ContributingContent { get; set; } = string.Empty;
}

/// <summary>
/// Type of intelligence insight
/// </summary>
public enum InsightType
{
    Correlation,
    Pattern,
    Anomaly,
    Recommendation,
    Prediction,
    Risk,
    Opportunity,
    Trend
}

/// <summary>
/// Priority level for insights
/// </summary>
public enum InsightPriority
{
    Low,
    Medium,
    High,
    Critical
}
