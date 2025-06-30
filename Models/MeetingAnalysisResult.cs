using System.ComponentModel.DataAnnotations;

namespace SemanticKernelDevHub.Models;

/// <summary>
/// Complete results of meeting transcript analysis
/// </summary>
public class MeetingAnalysisResult
{
    /// <summary>
    /// Unique identifier for this analysis result
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// The meeting transcript that was analyzed
    /// </summary>
    [Required]
    public MeetingTranscript SourceTranscript { get; set; } = new();

    /// <summary>
    /// Executive summary of the meeting
    /// </summary>
    public string Summary { get; set; } = string.Empty;

    /// <summary>
    /// Key topics discussed in the meeting
    /// </summary>
    public List<string> KeyTopics { get; set; } = new();

    /// <summary>
    /// Action items extracted from the meeting
    /// </summary>
    public List<ActionItem> ActionItems { get; set; } = new();

    /// <summary>
    /// Participants identified in the meeting
    /// </summary>
    public List<MeetingParticipant> Participants { get; set; } = new();

    /// <summary>
    /// Important decisions made during the meeting
    /// </summary>
    public List<string> Decisions { get; set; } = new();

    /// <summary>
    /// Key questions raised that need follow-up
    /// </summary>
    public List<string> OpenQuestions { get; set; } = new();

    /// <summary>
    /// Notable quotes or important statements
    /// </summary>
    public List<string> KeyQuotes { get; set; } = new();

    /// <summary>
    /// Overall sentiment analysis of the meeting
    /// </summary>
    public MeetingSentiment Sentiment { get; set; } = MeetingSentiment.Neutral;

    /// <summary>
    /// Analysis confidence score (0-100)
    /// </summary>
    public int ConfidenceScore { get; set; } = 0;

    /// <summary>
    /// When the analysis was performed
    /// </summary>
    public DateTime AnalyzedAt { get; set; } = DateTime.Now;

    /// <summary>
    /// How long the analysis took to complete
    /// </summary>
    public TimeSpan ProcessingTime { get; set; }

    /// <summary>
    /// Version of the analysis engine used
    /// </summary>
    public string AnalysisVersion { get; set; } = "1.0";

    /// <summary>
    /// Any warnings or issues encountered during analysis
    /// </summary>
    public List<string> AnalysisWarnings { get; set; } = new();

    /// <summary>
    /// Metadata and additional analysis details
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();

    /// <summary>
    /// Quality score of the original transcript (affects analysis reliability)
    /// </summary>
    public int TranscriptQuality { get; set; } = 0;

    /// <summary>
    /// Recommended follow-up actions for the meeting
    /// </summary>
    public List<string> FollowUpRecommendations { get; set; } = new();

    /// <summary>
    /// Gets a formatted summary of the analysis results
    /// </summary>
    /// <returns>Formatted analysis summary</returns>
    public string GetFormattedSummary()
    {
        var summary = $@"
📋 **Meeting Analysis Results**
🎤 **Meeting:** {SourceTranscript.Title}
📅 **Date:** {SourceTranscript.MeetingDate:yyyy-MM-dd HH:mm}
⏱️ **Duration:** {SourceTranscript.DurationMinutes} minutes
👥 **Participants:** {Participants.Count}

**📝 Summary:**
{Summary}

**🎯 Key Topics:**
{string.Join("\n", KeyTopics.Select(topic => $"• {topic}"))}

**✅ Action Items ({ActionItems.Count}):**
{string.Join("\n", ActionItems.Take(5).Select(item => $"  {item.GetDisplayString()}"))}
{(ActionItems.Count > 5 ? $"  ... and {ActionItems.Count - 5} more" : "")}

**👥 Participants:**
{string.Join("\n", Participants.Select(p => $"  {p.GetDisplayString()}"))}

**🎯 Decisions Made:**
{string.Join("\n", Decisions.Select(decision => $"• {decision}"))}

**❓ Open Questions:**
{string.Join("\n", OpenQuestions.Select(question => $"• {question}"))}

**📊 Analysis Metrics:**
• Confidence Score: {ConfidenceScore}%
• Transcript Quality: {TranscriptQuality}%
• Processing Time: {ProcessingTime.TotalSeconds:F1}s
• Sentiment: {Sentiment}
";

        if (AnalysisWarnings.Any())
        {
            summary += $@"

⚠️ **Warnings:**
{string.Join("\n", AnalysisWarnings.Select(warning => $"• {warning}"))}";
        }

        if (FollowUpRecommendations.Any())
        {
            summary += $@"

🔄 **Recommendations:**
{string.Join("\n", FollowUpRecommendations.Select(rec => $"• {rec}"))}";
        }

        return summary;
    }

    /// <summary>
    /// Gets action items grouped by priority
    /// </summary>
    /// <returns>Dictionary of action items grouped by priority</returns>
    public Dictionary<ActionItemPriority, List<ActionItem>> GetActionItemsByPriority()
    {
        return ActionItems
            .GroupBy(item => item.Priority)
            .OrderByDescending(group => group.Key)
            .ToDictionary(group => group.Key, group => group.ToList());
    }

    /// <summary>
    /// Gets action items assigned to a specific person
    /// </summary>
    /// <param name="assignee">Name of the person</param>
    /// <returns>List of action items assigned to that person</returns>
    public List<ActionItem> GetActionItemsForAssignee(string assignee)
    {
        return ActionItems
            .Where(item => string.Equals(item.AssignedTo, assignee, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(item => item.Priority)
            .ToList();
    }

    /// <summary>
    /// Gets the most active participants
    /// </summary>
    /// <param name="count">Number of top participants to return</param>
    /// <returns>List of most active participants</returns>
    public List<MeetingParticipant> GetMostActiveParticipants(int count = 5)
    {
        return Participants
            .OrderByDescending(p => p.SpeakingTurns)
            .Take(count)
            .ToList();
    }

    /// <summary>
    /// Calculates overall meeting effectiveness score
    /// </summary>
    /// <returns>Effectiveness score (0-100)</returns>
    public int CalculateEffectivenessScore()
    {
        var factors = new List<int>();

        // Action items factor (more actionable items = higher score)
        var actionItemScore = Math.Min(100, ActionItems.Count * 20);
        factors.Add(actionItemScore);

        // Participation factor (more even participation = higher score)
        if (Participants.Any())
        {
            var participationVariance = CalculateParticipationVariance();
            var participationScore = Math.Max(0, 100 - (int)(participationVariance * 10));
            factors.Add(participationScore);
        }

        // Decisions factor (clear decisions = higher score)
        var decisionScore = Math.Min(100, Decisions.Count * 25);
        factors.Add(decisionScore);

        // Confidence factor
        factors.Add(ConfidenceScore);

        // Transcript quality factor
        factors.Add(TranscriptQuality);

        return factors.Any() ? (int)factors.Average() : 0;
    }

    /// <summary>
    /// Calculates variance in participation levels
    /// </summary>
    /// <returns>Participation variance score</returns>
    private double CalculateParticipationVariance()
    {
        if (!Participants.Any()) return 0;

        var speakingTurns = Participants.Select(p => (double)p.SpeakingTurns).ToList();
        var mean = speakingTurns.Average();
        var variance = speakingTurns.Sum(x => Math.Pow(x - mean, 2)) / speakingTurns.Count;
        
        return Math.Sqrt(variance);
    }

    /// <summary>
    /// Validates the analysis results for completeness
    /// </summary>
    /// <returns>List of validation issues</returns>
    public List<string> ValidateResults()
    {
        var issues = new List<string>();

        if (string.IsNullOrWhiteSpace(Summary))
            issues.Add("Missing meeting summary");

        if (!Participants.Any())
            issues.Add("No participants identified");

        if (!ActionItems.Any() && !Decisions.Any())
            issues.Add("No actionable outcomes identified");

        if (ConfidenceScore < 50)
            issues.Add("Low confidence in analysis results");

        if (TranscriptQuality < 60)
            issues.Add("Poor transcript quality may affect accuracy");

        return issues;
    }
}

/// <summary>
/// Overall sentiment of the meeting
/// </summary>
public enum MeetingSentiment
{
    Very_Negative = -2,
    Negative = -1,
    Neutral = 0,
    Positive = 1,
    Very_Positive = 2
}
