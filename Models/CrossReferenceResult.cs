using System.ComponentModel;
using System.Text.Json.Serialization;

namespace SemanticKernelDevHub.Models;

/// <summary>
/// Represents connected information between different systems and data sources
/// </summary>
public class CrossReferenceResult
{
    /// <summary>
    /// Unique identifier for this cross-reference result
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Type of cross-reference analysis performed
    /// </summary>
    public CrossReferenceType Type { get; set; }

    /// <summary>
    /// Primary entity being cross-referenced
    /// </summary>
    public CrossReferenceEntity PrimaryEntity { get; set; } = new();

    /// <summary>
    /// Related entities found through cross-referencing
    /// </summary>
    public List<CrossReferenceEntity> RelatedEntities { get; set; } = new();

    /// <summary>
    /// Connections found between entities
    /// </summary>
    public List<EntityConnection> Connections { get; set; } = new();

    /// <summary>
    /// Confidence score of cross-reference analysis (0.0 to 1.0)
    /// </summary>
    public double ConfidenceScore { get; set; }

    /// <summary>
    /// Summary of findings
    /// </summary>
    public string Summary { get; set; } = string.Empty;

    /// <summary>
    /// Key insights from cross-referencing
    /// </summary>
    public List<string> KeyInsights { get; set; } = new();

    /// <summary>
    /// Patterns identified across systems
    /// </summary>
    public List<CrossReferencePattern> Patterns { get; set; } = new();

    /// <summary>
    /// Gaps or missing connections identified
    /// </summary>
    public List<string> IdentifiedGaps { get; set; } = new();

    /// <summary>
    /// Recommendations based on cross-reference analysis
    /// </summary>
    public List<string> Recommendations { get; set; } = new();

    /// <summary>
    /// When this analysis was performed
    /// </summary>
    public DateTime AnalyzedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Analysis method used
    /// </summary>
    public string AnalysisMethod { get; set; } = string.Empty;

    /// <summary>
    /// Quality metrics for this cross-reference
    /// </summary>
    public CrossReferenceQuality Quality { get; set; } = new();
}

/// <summary>
/// Represents an entity in cross-reference analysis
/// </summary>
public class CrossReferenceEntity
{
    /// <summary>
    /// Unique identifier of the entity
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Type of entity (CodeReview, Meeting, JiraTicket, Commit, etc.)
    /// </summary>
    public string EntityType { get; set; } = string.Empty;

    /// <summary>
    /// Title or name of the entity
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Description or content summary
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// When the entity was created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Who created or owns the entity
    /// </summary>
    public string Owner { get; set; } = string.Empty;

    /// <summary>
    /// Status of the entity
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Tags associated with the entity
    /// </summary>
    public List<string> Tags { get; set; } = new();

    /// <summary>
    /// Key content extracted for cross-referencing
    /// </summary>
    public List<string> KeyContent { get; set; } = new();

    /// <summary>
    /// Relevance score for cross-referencing (0.0 to 1.0)
    /// </summary>
    public double RelevanceScore { get; set; }

    /// <summary>
    /// Metadata specific to the entity type
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Represents a connection between two entities
/// </summary>
public class EntityConnection
{
    /// <summary>
    /// ID of the source entity
    /// </summary>
    public string SourceEntityId { get; set; } = string.Empty;

    /// <summary>
    /// ID of the target entity
    /// </summary>
    public string TargetEntityId { get; set; } = string.Empty;

    /// <summary>
    /// Type of connection
    /// </summary>
    public ConnectionType ConnectionType { get; set; }

    /// <summary>
    /// Strength of the connection (0.0 to 1.0)
    /// </summary>
    public double Strength { get; set; }

    /// <summary>
    /// Description of how they are connected
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Evidence supporting this connection
    /// </summary>
    public List<string> Evidence { get; set; } = new();

    /// <summary>
    /// Confidence in this connection (0.0 to 1.0)
    /// </summary>
    public double Confidence { get; set; }

    /// <summary>
    /// Direction of the connection
    /// </summary>
    public ConnectionDirection Direction { get; set; }

    /// <summary>
    /// When this connection was identified
    /// </summary>
    public DateTime IdentifiedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Pattern identified across multiple cross-references
/// </summary>
public class CrossReferencePattern
{
    /// <summary>
    /// Name of the pattern
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Description of the pattern
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// How frequently this pattern occurs
    /// </summary>
    public int Frequency { get; set; }

    /// <summary>
    /// Confidence in this pattern (0.0 to 1.0)
    /// </summary>
    public double Confidence { get; set; }

    /// <summary>
    /// Examples of this pattern
    /// </summary>
    public List<string> Examples { get; set; } = new();

    /// <summary>
    /// Implications of this pattern
    /// </summary>
    public List<string> Implications { get; set; } = new();

    /// <summary>
    /// Recommended actions for this pattern
    /// </summary>
    public List<string> RecommendedActions { get; set; } = new();
}

/// <summary>
/// Quality metrics for cross-reference analysis
/// </summary>
public class CrossReferenceQuality
{
    /// <summary>
    /// Overall quality score (0.0 to 1.0)
    /// </summary>
    public double OverallScore { get; set; }

    /// <summary>
    /// Completeness of the analysis (0.0 to 1.0)
    /// </summary>
    public double Completeness { get; set; }

    /// <summary>
    /// Accuracy of connections (0.0 to 1.0)
    /// </summary>
    public double Accuracy { get; set; }

    /// <summary>
    /// Coverage of available data sources (0.0 to 1.0)
    /// </summary>
    public double Coverage { get; set; }

    /// <summary>
    /// Timeliness of the analysis
    /// </summary>
    public double Timeliness { get; set; }

    /// <summary>
    /// Data sources analyzed
    /// </summary>
    public List<string> AnalyzedSources { get; set; } = new();

    /// <summary>
    /// Data sources that were missing or inaccessible
    /// </summary>
    public List<string> MissingSources { get; set; } = new();

    /// <summary>
    /// Analysis limitations
    /// </summary>
    public List<string> Limitations { get; set; } = new();
}

/// <summary>
/// Type of cross-reference analysis
/// </summary>
public enum CrossReferenceType
{
    CodeToMeeting,
    MeetingToJira,
    CodeToJira,
    FullSystemAnalysis,
    TimeBasedAnalysis,
    PersonBasedAnalysis,
    TopicBasedAnalysis,
    ImpactAnalysis,
    TrendAnalysis
}

/// <summary>
/// Type of connection between entities
/// </summary>
public enum ConnectionType
{
    DirectReference,
    TopicSimilarity,
    PersonInvolvement,
    TimeCorrelation,
    ImpactRelation,
    DependencyRelation,
    CausationRelation,
    CompletionRelation,
    DiscussionRelation,
    ImplementationRelation
}

/// <summary>
/// Direction of connection
/// </summary>
public enum ConnectionDirection
{
    Bidirectional,
    SourceToTarget,
    TargetToSource
}
