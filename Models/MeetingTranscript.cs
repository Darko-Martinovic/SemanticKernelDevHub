using System.ComponentModel.DataAnnotations;

namespace SemanticKernelDevHub.Models;

/// <summary>
/// Represents a meeting transcript with metadata and content
/// </summary>
public class MeetingTranscript
{
    /// <summary>
    /// Unique identifier for the meeting transcript
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Meeting title or subject
    /// </summary>
    [Required]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Date and time when the meeting occurred
    /// </summary>
    [Required]
    public DateTime MeetingDate { get; set; }

    /// <summary>
    /// Duration of the meeting in minutes
    /// </summary>
    public int DurationMinutes { get; set; }

    /// <summary>
    /// Full transcript content
    /// </summary>
    [Required]
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// File path where the transcript is stored
    /// </summary>
    public string FilePath { get; set; } = string.Empty;

    /// <summary>
    /// List of participants in the meeting
    /// </summary>
    public List<MeetingParticipant> Participants { get; set; } = new();

    /// <summary>
    /// Processing status of the transcript
    /// </summary>
    public TranscriptStatus Status { get; set; } = TranscriptStatus.Pending;

    /// <summary>
    /// When the transcript was created or uploaded
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    /// <summary>
    /// When the transcript was last processed
    /// </summary>
    public DateTime? ProcessedAt { get; set; }

    /// <summary>
    /// Meeting type or category
    /// </summary>
    public string MeetingType { get; set; } = "General";

    /// <summary>
    /// Additional metadata or notes
    /// </summary>
    public Dictionary<string, string> Metadata { get; set; } = new();

    /// <summary>
    /// Extracts the meeting date from filename if possible
    /// </summary>
    /// <param name="fileName">The transcript filename</param>
    /// <returns>Parsed date or current date if parsing fails</returns>
    public static DateTime ExtractDateFromFileName(string fileName)
    {
        try
        {
            // Look for patterns like YYYYMMDD_HHMM in filename
            var dateMatch = System.Text.RegularExpressions.Regex.Match(fileName, @"(\d{8})_(\d{4})");
            if (dateMatch.Success)
            {
                var dateStr = dateMatch.Groups[1].Value;
                var timeStr = dateMatch.Groups[2].Value;
                
                if (DateTime.TryParseExact(dateStr, "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out var date) &&
                    TimeSpan.TryParseExact(timeStr, @"hhmm", null, out var time))
                {
                    return date.Add(time);
                }
            }
        }
        catch
        {
            // Fall through to default
        }

        return DateTime.Now;
    }

    /// <summary>
    /// Extracts meeting title from filename
    /// </summary>
    /// <param name="fileName">The transcript filename</param>
    /// <returns>Extracted title or filename if extraction fails</returns>
    public static string ExtractTitleFromFileName(string fileName)
    {
        try
        {
            var nameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
            
            // Remove common prefixes
            var title = nameWithoutExtension
                .Replace("meeting_transcript_", "")
                .Replace("transcript_", "")
                .Replace("_", " ");

            // Remove date/time patterns
            title = System.Text.RegularExpressions.Regex.Replace(title, @"\d{8} \d{4}", "").Trim();
            
            return string.IsNullOrWhiteSpace(title) ? nameWithoutExtension : title;
        }
        catch
        {
            return Path.GetFileNameWithoutExtension(fileName);
        }
    }
}

/// <summary>
/// Processing status of a meeting transcript
/// </summary>
public enum TranscriptStatus
{
    Pending,
    Processing,
    Completed,
    Failed,
    Archived
}
