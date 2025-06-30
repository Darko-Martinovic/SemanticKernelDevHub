using System.ComponentModel.DataAnnotations;

namespace SemanticKernelDevHub.Models;

/// <summary>
/// Represents a participant in a meeting
/// </summary>
public class MeetingParticipant
{
    /// <summary>
    /// Unique identifier for the participant
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Participant's name
    /// </summary>
    [Required]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Participant's role or title
    /// </summary>
    public string Role { get; set; } = string.Empty;

    /// <summary>
    /// Email address if available
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Department or team
    /// </summary>
    public string Department { get; set; } = string.Empty;

    /// <summary>
    /// How actively they participated in the meeting
    /// </summary>
    public ParticipationLevel ParticipationLevel { get; set; } = ParticipationLevel.Active;

    /// <summary>
    /// Number of times they spoke during the meeting
    /// </summary>
    public int SpeakingTurns { get; set; } = 0;

    /// <summary>
    /// Key topics or themes they discussed
    /// </summary>
    public List<string> KeyTopics { get; set; } = new();

    /// <summary>
    /// Action items assigned to this participant
    /// </summary>
    public List<string> AssignedActionItems { get; set; } = new();

    /// <summary>
    /// Notable quotes or contributions from this participant
    /// </summary>
    public List<string> KeyContributions { get; set; } = new();

    /// <summary>
    /// Whether this participant was the meeting organizer
    /// </summary>
    public bool IsOrganizer { get; set; } = false;

    /// <summary>
    /// Whether this participant was a presenter
    /// </summary>
    public bool IsPresenter { get; set; } = false;

    /// <summary>
    /// Additional metadata about the participant
    /// </summary>
    public Dictionary<string, string> Metadata { get; set; } = new();

    /// <summary>
    /// When this participant record was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    /// <summary>
    /// Gets a display string for the participant
    /// </summary>
    /// <returns>Formatted participant information</returns>
    public string GetDisplayString()
    {
        var roleInfo = !string.IsNullOrEmpty(Role) ? $" ({Role})" : "";
        var speakingInfo = SpeakingTurns > 0 ? $" - {SpeakingTurns} contributions" : "";
        
        var indicators = new List<string>();
        if (IsOrganizer) indicators.Add("📋 Organizer");
        if (IsPresenter) indicators.Add("🎤 Presenter");
        
        var indicatorText = indicators.Any() ? $" [{string.Join(", ", indicators)}]" : "";

        return $"👤 {Name}{roleInfo}{speakingInfo}{indicatorText}";
    }

    /// <summary>
    /// Extracts participant information from transcript content
    /// </summary>
    /// <param name="transcriptContent">Full meeting transcript</param>
    /// <returns>List of identified participants</returns>
    public static List<MeetingParticipant> ExtractParticipants(string transcriptContent)
    {
        var participants = new Dictionary<string, MeetingParticipant>();
        var lines = transcriptContent.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        foreach (var line in lines)
        {
            // Look for speaker patterns like "John Smith:" or "[John]:"
            var speakerPatterns = new[]
            {
                @"^([A-Za-z\s]+):\s*(.+)$",
                @"^\[([A-Za-z\s]+)\]:\s*(.+)$",
                @"^([A-Za-z\s]+)\s*-\s*(.+)$"
            };

            foreach (var pattern in speakerPatterns)
            {
                var match = System.Text.RegularExpressions.Regex.Match(line.Trim(), pattern);
                if (match.Success)
                {
                    var name = match.Groups[1].Value.Trim();
                    var content = match.Groups[2].Value.Trim();

                    // Skip common non-name patterns
                    if (IsValidParticipantName(name))
                    {
                        if (!participants.ContainsKey(name))
                        {
                            participants[name] = new MeetingParticipant
                            {
                                Name = name,
                                SpeakingTurns = 0
                            };
                        }

                        participants[name].SpeakingTurns++;
                        
                        // Analyze content for role indicators
                        AnalyzeParticipantRole(participants[name], content);
                    }
                    break;
                }
            }
        }

        // Determine participation levels based on speaking turns
        var allParticipants = participants.Values.ToList();
        DetermineParticipationLevels(allParticipants);

        return allParticipants;
    }

    /// <summary>
    /// Validates if a string is likely a participant name
    /// </summary>
    /// <param name="name">Potential participant name</param>
    /// <returns>True if it's likely a valid name</returns>
    private static bool IsValidParticipantName(string name)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Length < 2 || name.Length > 50)
            return false;

        // Skip common transcript artifacts
        var invalidPatterns = new[]
        {
            "meeting", "transcript", "recording", "start", "end", "time",
            "date", "agenda", "notes", "action", "summary", "moderator"
        };

        var lowerName = name.ToLowerInvariant();
        return !invalidPatterns.Any(pattern => lowerName.Contains(pattern)) &&
               System.Text.RegularExpressions.Regex.IsMatch(name, @"^[A-Za-z\s\.]+$");
    }

    /// <summary>
    /// Analyzes content to determine participant role
    /// </summary>
    /// <param name="participant">The participant to analyze</param>
    /// <param name="content">What they said</param>
    private static void AnalyzeParticipantRole(MeetingParticipant participant, string content)
    {
        var lowerContent = content.ToLowerInvariant();

        // Look for organizer indicators
        var organizerKeywords = new[] { "welcome everyone", "let's start", "agenda", "next item", "wrap up", "thank you all" };
        if (organizerKeywords.Any(keyword => lowerContent.Contains(keyword)))
        {
            participant.IsOrganizer = true;
        }

        // Look for presenter indicators
        var presenterKeywords = new[] { "i'll present", "my presentation", "next slide", "as you can see", "in conclusion" };
        if (presenterKeywords.Any(keyword => lowerContent.Contains(keyword)))
        {
            participant.IsPresenter = true;
        }

        // Extract key topics
        if (content.Length > 20) // Only meaningful contributions
        {
            participant.KeyContributions.Add(content);
        }
    }

    /// <summary>
    /// Determines participation levels based on speaking frequency
    /// </summary>
    /// <param name="participants">List of all participants</param>
    private static void DetermineParticipationLevels(List<MeetingParticipant> participants)
    {
        if (!participants.Any()) return;

        var maxTurns = participants.Max(p => p.SpeakingTurns);
        var avgTurns = participants.Average(p => p.SpeakingTurns);

        foreach (var participant in participants)
        {
            participant.ParticipationLevel = participant.SpeakingTurns switch
            {
                0 => ParticipationLevel.Silent,
                var turns when turns < avgTurns * 0.5 => ParticipationLevel.Minimal,
                var turns when turns > avgTurns * 1.5 => ParticipationLevel.Highly_Active,
                _ => ParticipationLevel.Active
            };
        }
    }
}

/// <summary>
/// Level of participation in the meeting
/// </summary>
public enum ParticipationLevel
{
    Silent,
    Minimal,
    Active,
    Highly_Active
}
