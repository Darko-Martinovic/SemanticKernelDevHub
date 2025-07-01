using System.ComponentModel;
using System.Text.Json.Serialization;

namespace SemanticKernelDevHub.Models;

/// <summary>
/// Represents an AI-generated predictive recommendation
/// </summary>
public class PredictiveRecommendation
{
    /// <summary>
    /// Unique identifier for this recommendation
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Title of the recommendation
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Detailed description of the recommendation
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Category of recommendation (Performance, Security, Quality, Process, etc.)
    /// </summary>
    public RecommendationCategory Category { get; set; }

    /// <summary>
    /// Predicted outcome if recommendation is followed
    /// </summary>
    public string PredictedOutcome { get; set; } = string.Empty;

    /// <summary>
    /// Predicted outcome if recommendation is ignored
    /// </summary>
    public string RiskIfIgnored { get; set; } = string.Empty;

    /// <summary>
    /// Confidence in this prediction (0.0 to 1.0)
    /// </summary>
    public double Confidence { get; set; }

    /// <summary>
    /// Priority level
    /// </summary>
    public RecommendationPriority Priority { get; set; }

    /// <summary>
    /// Estimated effort to implement
    /// </summary>
    public EffortLevel EstimatedEffort { get; set; }

    /// <summary>
    /// Expected impact if implemented
    /// </summary>
    public ImpactLevel ExpectedImpact { get; set; }

    /// <summary>
    /// Time frame for implementation
    /// </summary>
    public TimeFrame TimeFrame { get; set; }

    /// <summary>
    /// Specific action steps to implement this recommendation
    /// </summary>
    public List<ActionStep> ActionSteps { get; set; } = new();

    /// <summary>
    /// Success metrics to track
    /// </summary>
    public List<string> SuccessMetrics { get; set; } = new();

    /// <summary>
    /// Dependencies or prerequisites
    /// </summary>
    public List<string> Dependencies { get; set; } = new();

    /// <summary>
    /// Related recommendations
    /// </summary>
    public List<string> RelatedRecommendations { get; set; } = new();

    /// <summary>
    /// Historical data patterns that support this recommendation
    /// </summary>
    public List<string> SupportingPatterns { get; set; } = new();

    /// <summary>
    /// When this recommendation was generated
    /// </summary>
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Expiration date for this recommendation
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// Tags for categorization and filtering
    /// </summary>
    public List<string> Tags { get; set; } = new();
}

/// <summary>
/// Action step for implementing a recommendation
/// </summary>
public class ActionStep
{
    /// <summary>
    /// Step number in sequence
    /// </summary>
    public int StepNumber { get; set; }

    /// <summary>
    /// Description of the step
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Who should perform this step
    /// </summary>
    public string ResponsibleRole { get; set; } = string.Empty;

    /// <summary>
    /// Estimated time to complete
    /// </summary>
    public string EstimatedTime { get; set; } = string.Empty;

    /// <summary>
    /// Tools or resources needed
    /// </summary>
    public List<string> RequiredResources { get; set; } = new();

    /// <summary>
    /// Success criteria for this step
    /// </summary>
    public string SuccessCriteria { get; set; } = string.Empty;
}

/// <summary>
/// Category of recommendation
/// </summary>
public enum RecommendationCategory
{
    Performance,
    Security,
    CodeQuality,
    ProcessImprovement,
    Testing,
    Documentation,
    Architecture,
    TeamCollaboration,
    Automation,
    Monitoring,
    Deployment,
    Maintenance
}

/// <summary>
/// Priority level for recommendations
/// </summary>
public enum RecommendationPriority
{
    Low,
    Medium,
    High,
    Critical
}

/// <summary>
/// Effort level required
/// </summary>
public enum EffortLevel
{
    Minimal,
    Low,
    Medium,
    High,
    Extensive
}

/// <summary>
/// Expected impact level
/// </summary>
public enum ImpactLevel
{
    Minimal,
    Low,
    Medium,
    High,
    Transformative
}

/// <summary>
/// Time frame for implementation
/// </summary>
public enum TimeFrame
{
    Immediate,
    ThisSprint,
    NextSprint,
    ThisQuarter,
    LongTerm
}
