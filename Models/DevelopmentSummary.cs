using System.ComponentModel;
using System.Text.Json.Serialization;

namespace SemanticKernelDevHub.Models;

/// <summary>
/// High-level development activity summary with executive insights
/// </summary>
public class DevelopmentSummary
{
    /// <summary>
    /// Unique identifier for this summary
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Title of the summary
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Time period covered by this summary
    /// </summary>
    public DateTimeRange Period { get; set; } = new();

    /// <summary>
    /// Overall health score (0-100)
    /// </summary>
    public int OverallHealthScore { get; set; }

    /// <summary>
    /// Key metrics and statistics
    /// </summary>
    public DevelopmentMetrics Metrics { get; set; } = new();

    /// <summary>
    /// Key achievements during this period
    /// </summary>
    public List<string> KeyAchievements { get; set; } = new();

    /// <summary>
    /// Main challenges and concerns
    /// </summary>
    public List<string> KeyChallenges { get; set; } = new();

    /// <summary>
    /// Top risks identified
    /// </summary>
    public List<RiskItem> Risks { get; set; } = new();

    /// <summary>
    /// Opportunities for improvement
    /// </summary>
    public List<string> Opportunities { get; set; } = new();

    /// <summary>
    /// Quality assessment
    /// </summary>
    public QualityAssessment Quality { get; set; } = new();

    /// <summary>
    /// Performance trends
    /// </summary>
    public PerformanceTrends Performance { get; set; } = new();

    /// <summary>
    /// Team collaboration insights
    /// </summary>
    public CollaborationInsights Collaboration { get; set; } = new();

    /// <summary>
    /// Predictive insights for next period
    /// </summary>
    public List<PredictiveRecommendation> Predictions { get; set; } = new();

    /// <summary>
    /// Cross-referenced insights
    /// </summary>
    public List<IntelligenceInsight> Insights { get; set; } = new();

    /// <summary>
    /// Executive summary text
    /// </summary>
    public string ExecutiveSummary { get; set; } = string.Empty;

    /// <summary>
    /// Recommended actions for leadership
    /// </summary>
    public List<string> LeadershipActions { get; set; } = new();

    /// <summary>
    /// When this summary was generated
    /// </summary>
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Data sources used for this summary
    /// </summary>
    public List<string> DataSources { get; set; } = new();
}

/// <summary>
/// Date and time range
/// </summary>
public class DateTimeRange
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    public TimeSpan Duration => EndDate - StartDate;
    public string FriendlyDescription => $"{StartDate:MMM dd} - {EndDate:MMM dd, yyyy}";
}

/// <summary>
/// Development metrics and statistics
/// </summary>
public class DevelopmentMetrics
{
    public int TotalCommits { get; set; }
    public int TotalPullRequests { get; set; }
    public int TotalCodeReviews { get; set; }
    public int TotalMeetings { get; set; }
    public int TotalJiraTickets { get; set; }
    public int LinesOfCodeAdded { get; set; }
    public int LinesOfCodeRemoved { get; set; }
    public int BugsFixed { get; set; }
    public int FeaturesCompleted { get; set; }
    public double AverageCodeQuality { get; set; }
    public double AverageMeetingEngagement { get; set; }
    public int ActionItemsCreated { get; set; }
    public int ActionItemsCompleted { get; set; }
    public double ActionItemCompletionRate => ActionItemsCreated > 0 ?
        (double)ActionItemsCompleted / ActionItemsCreated * 100 : 0;
}

/// <summary>
/// Risk item with details
/// </summary>
public class RiskItem
{
    public string Description { get; set; } = string.Empty;
    public RiskLevel Level { get; set; }
    public string Impact { get; set; } = string.Empty;
    public string Mitigation { get; set; } = string.Empty;
    public string Owner { get; set; } = string.Empty;
}

/// <summary>
/// Quality assessment details
/// </summary>
public class QualityAssessment
{
    public double OverallScore { get; set; }
    public double CodeQualityTrend { get; set; }
    public List<string> QualityImprovements { get; set; } = new();
    public List<string> QualityConcerns { get; set; } = new();
    public int SecurityIssuesFound { get; set; }
    public int PerformanceIssuesFound { get; set; }
    public double TestCoverage { get; set; }
    public string CodeReviewEffectiveness { get; set; } = string.Empty;
}

/// <summary>
/// Performance trends analysis
/// </summary>
public class PerformanceTrends
{
    public double VelocityScore { get; set; }
    public string VelocityTrend { get; set; } = string.Empty;
    public double DeliveryPredictability { get; set; }
    public double TechnicalDebtTrend { get; set; }
    public List<string> PerformanceWins { get; set; } = new();
    public List<string> PerformanceConcerns { get; set; } = new();
    public string NextSprintProjection { get; set; } = string.Empty;
}

/// <summary>
/// Team collaboration insights
/// </summary>
public class CollaborationInsights
{
    public double CollaborationScore { get; set; }
    public List<string> CollaborationStrengths { get; set; } = new();
    public List<string> CollaborationGaps { get; set; } = new();
    public int MeetingEffectiveness { get; set; }
    public double CommunicationQuality { get; set; }
    public List<string> TeamDynamicsObservations { get; set; } = new();
    public List<string> ImprovementSuggestions { get; set; } = new();
}

/// <summary>
/// Risk level enumeration
/// </summary>
public enum RiskLevel
{
    Low,
    Medium,
    High,
    Critical
}
