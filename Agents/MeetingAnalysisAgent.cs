using Microsoft.SemanticKernel;
using System.ComponentModel;
using SemanticKernelDevHub.Models;
using SemanticKernelDevHub.Plugins;
using System.Diagnostics;

namespace SemanticKernelDevHub.Agents;

/// <summary>
/// Agent responsible for analyzing meeting transcripts using Semantic Kernel
/// </summary>
public class MeetingAnalysisAgent : IAgent
{
    private readonly Kernel _kernel;
    private readonly FileSystemPlugin? _fileSystemPlugin;

    public string Name => "MeetingAnalysisAgent";
    
    public string Description => "Analyzes meeting transcripts to extract participants, action items, decisions, and key insights using AI-powered analysis";

    public MeetingAnalysisAgent(Kernel kernel, FileSystemPlugin? fileSystemPlugin = null)
    {
        _kernel = kernel;
        _fileSystemPlugin = fileSystemPlugin;
    }

    public Task InitializeAsync()
    {
        Console.WriteLine($"✅ {Name} initialized successfully");
        return Task.CompletedTask;
    }

    public Task RegisterFunctionsAsync(Kernel kernel)
    {
        kernel.ImportPluginFromObject(this, "MeetingAnalysis");
        Console.WriteLine($"🔧 {Name} functions registered with kernel");
        return Task.CompletedTask;
    }

    public IEnumerable<string> GetFunctionNames()
    {
        return new[]
        {
            "AnalyzeTranscript",
            "ExtractActionItems",
            "IdentifyParticipants",
            "SummarizeMeeting",
            "AnalyzeSentiment",
            "ProcessTranscriptFile",
            "AnalyzeSampleMeeting"
        };
    }

    /// <summary>
    /// Performs comprehensive analysis of a meeting transcript
    /// </summary>
    /// <param name="transcriptContent">The meeting transcript content</param>
    /// <param name="meetingTitle">Optional meeting title</param>
    /// <returns>Complete analysis results</returns>
    [KernelFunction("analyze_transcript")]
    [Description("Performs comprehensive analysis of a meeting transcript including participants, action items, and key insights")]
    public async Task<MeetingAnalysisResult> AnalyzeTranscript(
        [Description("The meeting transcript content to analyze")] string transcriptContent,
        [Description("Optional meeting title")] string meetingTitle = "")
    {
        var stopwatch = Stopwatch.StartNew();
        
        var result = new MeetingAnalysisResult
        {
            SourceTranscript = new MeetingTranscript
            {
                Title = string.IsNullOrEmpty(meetingTitle) ? "Meeting Analysis" : meetingTitle,
                Content = transcriptContent,
                MeetingDate = DateTime.Now
            }
        };

        try
        {
            Console.WriteLine("🔍 Starting comprehensive meeting analysis...");

            // Extract participants first
            result.Participants = MeetingParticipant.ExtractParticipants(transcriptContent);
            Console.WriteLine($"👥 Identified {result.Participants.Count} participants");

            // Generate meeting summary using AI
            result.Summary = await GenerateMeetingSummary(transcriptContent);
            Console.WriteLine("📝 Generated meeting summary");

            // Extract action items using AI
            result.ActionItems = await ExtractActionItemsWithAI(transcriptContent, result.Participants);
            Console.WriteLine($"✅ Extracted {result.ActionItems.Count} action items");

            // Extract key topics using AI
            result.KeyTopics = await ExtractKeyTopics(transcriptContent);
            Console.WriteLine($"🎯 Identified {result.KeyTopics.Count} key topics");

            // Extract decisions using AI
            result.Decisions = await ExtractDecisions(transcriptContent);
            Console.WriteLine($"🎯 Identified {result.Decisions.Count} decisions");

            // Extract open questions using AI
            result.OpenQuestions = await ExtractOpenQuestions(transcriptContent);
            Console.WriteLine($"❓ Identified {result.OpenQuestions.Count} open questions");

            // Analyze sentiment using AI
            result.Sentiment = await AnalyzeMeetingSentiment(transcriptContent);
            Console.WriteLine($"😊 Sentiment analysis: {result.Sentiment}");

            // Calculate scores and metrics
            result.ConfidenceScore = CalculateConfidenceScore(result);
            result.TranscriptQuality = AssessTranscriptQuality(transcriptContent);
            
            // Generate recommendations
            result.FollowUpRecommendations = await GenerateRecommendations(result);

            stopwatch.Stop();
            result.ProcessingTime = stopwatch.Elapsed;
            result.AnalyzedAt = DateTime.Now;

            Console.WriteLine($"✅ Analysis completed in {result.ProcessingTime.TotalSeconds:F1} seconds");
            return result;
        }
        catch (Exception ex)
        {
            result.AnalysisWarnings.Add($"Analysis failed: {ex.Message}");
            result.ConfidenceScore = 0;
            stopwatch.Stop();
            result.ProcessingTime = stopwatch.Elapsed;
            
            Console.WriteLine($"❌ Analysis failed: {ex.Message}");
            return result;
        }
    }

    /// <summary>
    /// Extracts action items from meeting transcript using AI
    /// </summary>
    /// <param name="transcriptContent">Meeting transcript content</param>
    /// <returns>List of extracted action items</returns>
    [KernelFunction("extract_action_items")]
    [Description("Extracts action items and tasks from meeting transcript content")]
    public async Task<List<ActionItem>> ExtractActionItems(
        [Description("The meeting transcript content")] string transcriptContent)
    {
        var participants = MeetingParticipant.ExtractParticipants(transcriptContent);
        return await ExtractActionItemsWithAI(transcriptContent, participants);
    }

    /// <summary>
    /// Identifies and analyzes participants in the meeting
    /// </summary>
    /// <param name="transcriptContent">Meeting transcript content</param>
    /// <returns>List of meeting participants</returns>
    [KernelFunction("identify_participants")]
    [Description("Identifies participants and analyzes their roles and contributions")]
    public async Task<List<MeetingParticipant>> IdentifyParticipants(
        [Description("The meeting transcript content")] string transcriptContent)
    {
        return await Task.FromResult(MeetingParticipant.ExtractParticipants(transcriptContent));
    }

    /// <summary>
    /// Generates a comprehensive summary of the meeting
    /// </summary>
    /// <param name="transcriptContent">Meeting transcript content</param>
    /// <returns>Meeting summary</returns>
    [KernelFunction("summarize_meeting")]
    [Description("Generates a comprehensive summary of the meeting discussion")]
    public async Task<string> SummarizeMeeting(
        [Description("The meeting transcript content")] string transcriptContent)
    {
        return await GenerateMeetingSummary(transcriptContent);
    }

    /// <summary>
    /// Analyzes the sentiment and tone of the meeting
    /// </summary>
    /// <param name="transcriptContent">Meeting transcript content</param>
    /// <returns>Overall meeting sentiment</returns>
    [KernelFunction("analyze_sentiment")]
    [Description("Analyzes the overall sentiment and tone of the meeting")]
    public async Task<MeetingSentiment> AnalyzeSentiment(
        [Description("The meeting transcript content")] string transcriptContent)
    {
        return await AnalyzeMeetingSentiment(transcriptContent);
    }

    /// <summary>
    /// Processes a transcript file from the file system
    /// </summary>
    /// <param name="filePath">Path to the transcript file</param>
    /// <returns>Analysis results</returns>
    [KernelFunction("process_transcript_file")]
    [Description("Processes a meeting transcript file from the file system")]
    public async Task<MeetingAnalysisResult> ProcessTranscriptFile(
        [Description("Path to the transcript file to process")] string filePath)
    {
        if (_fileSystemPlugin == null)
        {
            throw new InvalidOperationException("FileSystemPlugin not available for file processing");
        }

        try
        {
            Console.WriteLine($"📂 Reading transcript file: {Path.GetFileName(filePath)}");
            
            var content = await _fileSystemPlugin.ReadTranscriptFile(filePath);
            if (content.StartsWith("❌"))
            {
                throw new InvalidOperationException($"Failed to read file: {content}");
            }

            var fileName = Path.GetFileName(filePath);
            var meetingTitle = MeetingTranscript.ExtractTitleFromFileName(fileName);
            
            var result = await AnalyzeTranscript(content, meetingTitle);
            result.SourceTranscript.FilePath = filePath;
            result.SourceTranscript.MeetingDate = MeetingTranscript.ExtractDateFromFileName(fileName);

            Console.WriteLine($"✅ Successfully processed transcript: {meetingTitle}");
            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error processing transcript file: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Analyzes a sample meeting from templates
    /// </summary>
    /// <param name="templateIndex">Index of the template to analyze (0-based)</param>
    /// <returns>Analysis results</returns>
    [KernelFunction("analyze_sample_meeting")]
    [Description("Analyzes one of the sample meeting transcripts from templates")]
    public async Task<MeetingAnalysisResult> AnalyzeSampleMeeting(
        [Description("Index of the sample meeting to analyze (0, 1, or 2)")] int templateIndex = 0)
    {
        if (_fileSystemPlugin == null)
        {
            throw new InvalidOperationException("FileSystemPlugin not available for sample analysis");
        }

        try
        {
            var templateFiles = await _fileSystemPlugin.ListTemplateFiles();
            
            if (!templateFiles.Any())
            {
                throw new InvalidOperationException("No sample meeting templates found");
            }

            templateIndex = Math.Max(0, Math.Min(templateIndex, templateFiles.Count - 1));
            var selectedFile = templateFiles[templateIndex];
            
            Console.WriteLine($"📋 Analyzing sample meeting: {Path.GetFileName(selectedFile)}");
            
            return await ProcessTranscriptFile(selectedFile);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error analyzing sample meeting: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Generates a meeting summary using AI
    /// </summary>
    private async Task<string> GenerateMeetingSummary(string transcriptContent)
    {
        var prompt = $@"
Analyze this meeting transcript and provide a concise but comprehensive summary.

Focus on:
1. Main purpose and objectives of the meeting
2. Key discussion points and topics covered
3. Important outcomes and conclusions
4. Overall meeting flow and dynamics

Meeting Transcript:
{transcriptContent}

Provide a well-structured summary in 3-5 paragraphs that captures the essence of the meeting.";

        var response = await _kernel.InvokePromptAsync(prompt);
        return response.ToString();
    }

    /// <summary>
    /// Extracts action items using AI analysis
    /// </summary>
    private async Task<List<ActionItem>> ExtractActionItemsWithAI(string transcriptContent, List<MeetingParticipant> participants)
    {
        var participantNames = string.Join(", ", participants.Select(p => p.Name));
        
        var prompt = $@"
Analyze this meeting transcript and extract all action items, tasks, and commitments made.

For each action item, identify:
1. What needs to be done (clear description)
2. Who is responsible (if mentioned)
3. Any deadlines or timeframes
4. Priority level based on urgency indicators

Known participants: {participantNames}

Meeting Transcript:
{transcriptContent}

Format each action item as:
ITEM: [description]
ASSIGNED: [person name or 'Unassigned']
PRIORITY: [Urgent/High/Medium/Low]
NOTES: [additional context if any]
---

If no action items are found, respond with 'NO_ACTION_ITEMS'.";

        var response = await _kernel.InvokePromptAsync(prompt);
        var responseText = response.ToString();

        if (responseText.Contains("NO_ACTION_ITEMS"))
        {
            return new List<ActionItem>();
        }

        return ParseActionItemsFromResponse(responseText, participants);
    }

    /// <summary>
    /// Extracts key topics from the meeting
    /// </summary>
    private async Task<List<string>> ExtractKeyTopics(string transcriptContent)
    {
        var prompt = $@"
Analyze this meeting transcript and identify the key topics and themes discussed.

Extract 5-10 main topics that were central to the discussion. Focus on:
- Main agenda items
- Important discussion themes
- Technical topics or project areas
- Decisions that were made
- Problems or issues discussed

Meeting Transcript:
{transcriptContent}

List each topic on a separate line without numbering or bullets.";

        var response = await _kernel.InvokePromptAsync(prompt);
        var topics = response.ToString()
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(line => line.Trim().TrimStart('-', '*', '•').Trim())
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Take(10)
            .ToList();

        return topics;
    }

    /// <summary>
    /// Extracts decisions made during the meeting
    /// </summary>
    private async Task<List<string>> ExtractDecisions(string transcriptContent)
    {
        var prompt = $@"
Analyze this meeting transcript and identify all decisions that were made.

Look for:
- Explicit decisions (""we decided to..."", ""it was agreed that..."")
- Approved proposals or recommendations
- Chosen directions or approaches
- Resolved issues or conflicts

Meeting Transcript:
{transcriptContent}

List each decision on a separate line. If no clear decisions were made, respond with 'NO_DECISIONS'.";

        var response = await _kernel.InvokePromptAsync(prompt);
        var responseText = response.ToString();

        if (responseText.Contains("NO_DECISIONS"))
        {
            return new List<string>();
        }

        return responseText
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(line => line.Trim().TrimStart('-', '*', '•').Trim())
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Take(10)
            .ToList();
    }

    /// <summary>
    /// Extracts open questions from the meeting
    /// </summary>
    private async Task<List<string>> ExtractOpenQuestions(string transcriptContent)
    {
        var prompt = $@"
Analyze this meeting transcript and identify questions that remain unanswered or require follow-up.

Look for:
- Direct questions that weren't answered
- Issues that need investigation
- Unclear points that need clarification
- Topics deferred for later discussion

Meeting Transcript:
{transcriptContent}

List each open question on a separate line. If no open questions were identified, respond with 'NO_QUESTIONS'.";

        var response = await _kernel.InvokePromptAsync(prompt);
        var responseText = response.ToString();

        if (responseText.Contains("NO_QUESTIONS"))
        {
            return new List<string>();
        }

        return responseText
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(line => line.Trim().TrimStart('-', '*', '•').Trim())
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Take(10)
            .ToList();
    }

    /// <summary>
    /// Analyzes meeting sentiment using AI
    /// </summary>
    private async Task<MeetingSentiment> AnalyzeMeetingSentiment(string transcriptContent)
    {
        var prompt = $@"
Analyze the overall sentiment and tone of this meeting transcript.

Consider:
- General mood and atmosphere
- Level of collaboration vs conflict
- Productivity and engagement
- Stress levels and satisfaction
- Resolution vs frustration

Meeting Transcript:
{transcriptContent}

Respond with only one of these sentiment levels:
VERY_POSITIVE - Highly collaborative, productive, positive
POSITIVE - Generally good mood, collaborative
NEUTRAL - Balanced, professional, neither positive nor negative
NEGATIVE - Some tension, disagreements, frustration
VERY_NEGATIVE - High conflict, very unproductive, negative

Response:";

        var response = await _kernel.InvokePromptAsync(prompt);
        var sentimentText = response.ToString().Trim().ToUpperInvariant();

        return sentimentText switch
        {
            "VERY_POSITIVE" => MeetingSentiment.Very_Positive,
            "POSITIVE" => MeetingSentiment.Positive,
            "NEGATIVE" => MeetingSentiment.Negative,
            "VERY_NEGATIVE" => MeetingSentiment.Very_Negative,
            _ => MeetingSentiment.Neutral
        };
    }

    /// <summary>
    /// Generates follow-up recommendations
    /// </summary>
    private Task<List<string>> GenerateRecommendations(MeetingAnalysisResult result)
    {
        var recommendations = new List<string>();

        if (!result.ActionItems.Any())
        {
            recommendations.Add("Schedule follow-up meeting to define clear action items");
        }

        if (result.ActionItems.Count(ai => string.IsNullOrEmpty(ai.AssignedTo)) > 0)
        {
            recommendations.Add("Assign ownership to unassigned action items");
        }

        if (!result.Decisions.Any())
        {
            recommendations.Add("Document and communicate key decisions made");
        }

        if (result.OpenQuestions.Any())
        {
            recommendations.Add($"Follow up on {result.OpenQuestions.Count} unanswered questions");
        }

        if (result.Sentiment == MeetingSentiment.Negative || result.Sentiment == MeetingSentiment.Very_Negative)
        {
            recommendations.Add("Address concerns and conflicts raised in the meeting");
        }

        return Task.FromResult(recommendations.Any() ? recommendations : new List<string> { "Continue with planned next steps" });
    }

    /// <summary>
    /// Parses action items from AI response
    /// </summary>
    private List<ActionItem> ParseActionItemsFromResponse(string response, List<MeetingParticipant> participants)
    {
        var actionItems = new List<ActionItem>();
        var items = response.Split("---", StringSplitOptions.RemoveEmptyEntries);

        foreach (var item in items)
        {
            try
            {
                var lines = item.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                var actionItem = new ActionItem();

                foreach (var line in lines)
                {
                    var trimmedLine = line.Trim();
                    if (trimmedLine.StartsWith("ITEM:", StringComparison.OrdinalIgnoreCase))
                    {
                        actionItem.Description = trimmedLine.Substring(5).Trim();
                    }
                    else if (trimmedLine.StartsWith("ASSIGNED:", StringComparison.OrdinalIgnoreCase))
                    {
                        var assignee = trimmedLine.Substring(9).Trim();
                        actionItem.AssignedTo = assignee == "Unassigned" ? "" : assignee;
                    }
                    else if (trimmedLine.StartsWith("PRIORITY:", StringComparison.OrdinalIgnoreCase))
                    {
                        var priorityText = trimmedLine.Substring(9).Trim();
                        actionItem.Priority = Enum.TryParse<ActionItemPriority>(priorityText, true, out var priority) 
                            ? priority 
                            : ActionItemPriority.Medium;
                    }
                    else if (trimmedLine.StartsWith("NOTES:", StringComparison.OrdinalIgnoreCase))
                    {
                        actionItem.Notes = trimmedLine.Substring(6).Trim();
                    }
                }

                if (!string.IsNullOrWhiteSpace(actionItem.Description))
                {
                    actionItems.Add(actionItem);
                }
            }
            catch
            {
                // Skip malformed items
                continue;
            }
        }

        return actionItems;
    }

    /// <summary>
    /// Calculates confidence score based on analysis quality
    /// </summary>
    private int CalculateConfidenceScore(MeetingAnalysisResult result)
    {
        var factors = new List<int>();

        // Content quality factor
        var contentLength = result.SourceTranscript.Content.Length;
        var contentScore = contentLength switch
        {
            < 500 => 40,      // Very short transcript
            < 2000 => 70,     // Short transcript
            < 10000 => 90,    // Good length
            _ => 95           // Long transcript
        };
        factors.Add(contentScore);

        // Participants factor
        var participantScore = result.Participants.Count switch
        {
            0 => 20,
            1 => 50,
            >= 2 and <= 8 => 90,
            _ => 70  // Too many participants might be noisy
        };
        factors.Add(participantScore);

        // Action items factor
        var actionItemScore = result.ActionItems.Any() ? 80 : 60;
        factors.Add(actionItemScore);

        // Structure factor (based on whether we found organized content)
        var structureScore = (result.KeyTopics.Any() && result.Decisions.Any()) ? 85 : 70;
        factors.Add(structureScore);

        return (int)factors.Average();
    }

    /// <summary>
    /// Assesses the quality of the transcript content
    /// </summary>
    private int AssessTranscriptQuality(string content)
    {
        var score = 50; // Base score

        // Length factor
        if (content.Length > 1000) score += 20;
        if (content.Length > 5000) score += 10;

        // Structure indicators
        if (content.Contains(":")) score += 15; // Likely speaker indicators
        if (System.Text.RegularExpressions.Regex.Matches(content, @"\b[A-Z][a-z]+:").Count > 2) score += 15; // Multiple speakers

        // Content quality indicators
        var sentences = content.Split('.', '!', '?').Length;
        if (sentences > 10) score += 10;

        return Math.Min(100, score);
    }
}
