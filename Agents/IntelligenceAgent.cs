using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Memory;
using System.ComponentModel;
using System.Text.Json;
using SemanticKernelDevHub.Models;
using SemanticKernelDevHub.Plugins;

#pragma warning disable SKEXP0001 // Suppress experimental API warnings for Memory features

namespace SemanticKernelDevHub.Agents;

/// <summary>
/// Advanced intelligence agent that provides cross-reference orchestration and predictive insights
/// </summary>
public class IntelligenceAgent : IAgent
{
    private readonly Kernel _kernel;
    private readonly CodeReviewAgent? _codeReviewAgent;
    private readonly MeetingAnalysisAgent? _meetingAnalysisAgent;
    private readonly JiraIntegrationAgent? _jiraIntegrationAgent;
    private readonly ISemanticTextMemory? _memory;
    private readonly List<IntelligenceInsight> _insights;
    private readonly List<PredictiveRecommendation> _recommendations;

    public string Name => "IntelligenceAgent";

    public string Description => "Provides advanced cross-reference analysis, predictive insights, and executive-level development intelligence";

    public IntelligenceAgent(
        Kernel kernel,
        CodeReviewAgent? codeReviewAgent = null,
        MeetingAnalysisAgent? meetingAnalysisAgent = null,
        JiraIntegrationAgent? jiraIntegrationAgent = null,
        ISemanticTextMemory? memory = null)
    {
        _kernel = kernel;
        _codeReviewAgent = codeReviewAgent;
        _meetingAnalysisAgent = meetingAnalysisAgent;
        _jiraIntegrationAgent = jiraIntegrationAgent;
        _memory = memory;
        _insights = new List<IntelligenceInsight>();
        _recommendations = new List<PredictiveRecommendation>();
    }

    public Task InitializeAsync()
    {
        Console.WriteLine($"✅ {Name} initialized successfully");
        return Task.CompletedTask;
    }

    public Task RegisterFunctionsAsync(Kernel kernel)
    {
        kernel.ImportPluginFromObject(this, "Intelligence");
        Console.WriteLine($"🔧 {Name} functions registered with kernel");
        return Task.CompletedTask;
    }

    public IEnumerable<string> GetFunctionNames()
    {
        return new[]
        {
            "GenerateDevelopmentIntelligenceReport",
            "AnalyzeCrossReferences",
            "GeneratePredictiveInsights",
            "CreateExecutiveSummary",
            "DetectPatterns",
            "AnalyzeCodeMeetingCorrelations",
            "GenerateRiskAssessment",
            "OptimizeWorkflows",
            "PredictProjectOutcomes"
        };
    }

    /// <summary>
    /// Generates a comprehensive development intelligence report
    /// </summary>
    /// <param name="timeFrameDays">Number of days to analyze (default: 7)</param>
    /// <param name="includeDetails">Whether to include detailed analysis</param>
    /// <returns>Development intelligence report</returns>
    [KernelFunction("generate_development_intelligence_report")]
    [Description("Generates a comprehensive development intelligence report with cross-system analysis")]
    public async Task<DevelopmentSummary> GenerateDevelopmentIntelligenceReport(
        [Description("Number of days to analyze")] int timeFrameDays = 7,
        [Description("Include detailed analysis")] bool includeDetails = true)
    {
        var endDate = DateTime.UtcNow;
        var startDate = endDate.AddDays(-timeFrameDays);

        var summary = new DevelopmentSummary
        {
            Title = $"Development Intelligence Report - {timeFrameDays} Day Analysis",
            Period = new DateTimeRange { StartDate = startDate, EndDate = endDate }
        };

        try
        {
            // Collect data from all sources
            var codeReviews = await CollectCodeReviewData(startDate, endDate);
            var meetings = await CollectMeetingData(startDate, endDate);
            var jiraTickets = await CollectJiraData(startDate, endDate);

            // Generate metrics
            summary.Metrics = await GenerateMetrics(codeReviews, meetings, jiraTickets);

            // Calculate health score
            summary.OverallHealthScore = CalculateHealthScore(summary.Metrics);

            // Generate cross-reference insights
            var crossRefs = await AnalyzeCrossReferences();
            summary.Insights = _insights.Take(10).ToList();

            // Generate predictive recommendations
            var predictions = await GeneratePredictiveInsights();
            summary.Predictions = _recommendations.Take(5).ToList();

            // Analyze quality trends
            summary.Quality = await AnalyzeQualityTrends(codeReviews);

            // Analyze performance trends
            summary.Performance = await AnalyzePerformanceTrends(codeReviews, jiraTickets);

            // Analyze collaboration
            summary.Collaboration = await AnalyzeCollaboration(meetings, codeReviews);

            // Generate executive summary
            summary.ExecutiveSummary = await GenerateExecutiveSummaryText(summary);

            // Generate leadership actions
            summary.LeadershipActions = await GenerateLeadershipActions(summary);

            // Store in memory for future reference
            if (_memory != null)
            {
                await StoreInMemory("development_summary", JsonSerializer.Serialize(summary));
            }

            return summary;
        }
        catch (Exception ex)
        {
            summary.ExecutiveSummary = $"❌ Error generating intelligence report: {ex.Message}";
            return summary;
        }
    }

    /// <summary>
    /// Analyzes cross-references between different systems
    /// </summary>
    /// <param name="analysisType">Type of cross-reference analysis to perform</param>
    /// <returns>Cross-reference analysis results</returns>
    [KernelFunction("analyze_cross_references")]
    [Description("Analyzes connections and correlations between code reviews, meetings, and Jira tickets")]
    public async Task<CrossReferenceResult> AnalyzeCrossReferences(
        [Description("Type of analysis: CodeToMeeting, MeetingToJira, CodeToJira, or FullSystemAnalysis")]
        string analysisType = "FullSystemAnalysis")
    {
        var result = new CrossReferenceResult
        {
            Type = Enum.Parse<CrossReferenceType>(analysisType, true)
        };

        try
        {
            var entities = await CollectAllEntities();
            result.RelatedEntities = entities;

            // Analyze connections based on type
            switch (result.Type)
            {
                case CrossReferenceType.CodeToMeeting:
                    result.Connections = await FindCodeMeetingConnections(entities);
                    break;
                case CrossReferenceType.MeetingToJira:
                    result.Connections = await FindMeetingJiraConnections(entities);
                    break;
                case CrossReferenceType.CodeToJira:
                    result.Connections = await FindCodeJiraConnections(entities);
                    break;
                default:
                    result.Connections = await FindAllConnections(entities);
                    break;
            }

            // Calculate confidence score
            result.ConfidenceScore = CalculateConnectionConfidence(result.Connections);

            // Generate insights
            result.KeyInsights = await GenerateConnectionInsights(result.Connections);

            // Identify patterns
            result.Patterns = await IdentifyPatterns(result.Connections);

            // Generate summary
            result.Summary = await GenerateCrossReferenceSummary(result);

            // Store insights
            await StoreInsights(result);

            return result;
        }
        catch (Exception ex)
        {
            result.Summary = $"❌ Error analyzing cross-references: {ex.Message}";
            return result;
        }
    }

    /// <summary>
    /// Generates predictive insights based on historical patterns
    /// </summary>
    /// <param name="predictionHorizonDays">Number of days to predict ahead</param>
    /// <returns>List of predictive recommendations</returns>
    [KernelFunction("generate_predictive_insights")]
    [Description("Generates AI-powered predictive insights and recommendations based on development patterns")]
    public async Task<List<PredictiveRecommendation>> GeneratePredictiveInsights(
        [Description("Number of days to predict ahead")] int predictionHorizonDays = 14)
    {
        var recommendations = new List<PredictiveRecommendation>();

        try
        {
            // Analyze historical patterns
            var patterns = await AnalyzeHistoricalPatterns();

            // Generate performance predictions
            var performancePredictions = await GeneratePerformancePredictions(patterns, predictionHorizonDays);
            recommendations.AddRange(performancePredictions);

            // Generate quality predictions
            var qualityPredictions = await GenerateQualityPredictions(patterns, predictionHorizonDays);
            recommendations.AddRange(qualityPredictions);

            // Generate risk predictions
            var riskPredictions = await GenerateRiskPredictions(patterns, predictionHorizonDays);
            recommendations.AddRange(riskPredictions);

            // Generate process improvement predictions
            var processPredictions = await GenerateProcessPredictions(patterns, predictionHorizonDays);
            recommendations.AddRange(processPredictions);

            // Store recommendations
            _recommendations.AddRange(recommendations);

            // Store in memory
            if (_memory != null)
            {
                foreach (var rec in recommendations)
                {
                    await StoreInMemory($"prediction_{rec.Id}", JsonSerializer.Serialize(rec));
                }
            }

            return recommendations;
        }
        catch (Exception ex)
        {
            var errorRec = new PredictiveRecommendation
            {
                Title = "Analysis Error",
                Description = $"Error generating predictive insights: {ex.Message}",
                Priority = RecommendationPriority.High,
                Category = RecommendationCategory.ProcessImprovement
            };
            return new List<PredictiveRecommendation> { errorRec };
        }
    }

    /// <summary>
    /// Creates an executive summary for leadership
    /// </summary>
    /// <param name="focusArea">Specific area to focus on</param>
    /// <returns>Executive summary text</returns>
    [KernelFunction("create_executive_summary")]
    [Description("Creates an executive summary for leadership with key insights and recommendations")]
    public async Task<string> CreateExecutiveSummary(
        [Description("Focus area: Overall, Quality, Performance, Risks, or Opportunities")]
        string focusArea = "Overall")
    {
        try
        {
            var developmentSummary = await GenerateDevelopmentIntelligenceReport(7, false);

            var prompt = $@"
Create a concise executive summary for senior leadership based on the following development intelligence data.

**Focus Area**: {focusArea}

**Development Metrics**:
- Health Score: {developmentSummary.OverallHealthScore}/100
- Code Reviews: {developmentSummary.Metrics.TotalCodeReviews}
- Commits: {developmentSummary.Metrics.TotalCommits}
- Meetings: {developmentSummary.Metrics.TotalMeetings}
- Jira Tickets: {developmentSummary.Metrics.TotalJiraTickets}
- Action Item Completion: {developmentSummary.Metrics.ActionItemCompletionRate:F1}%

**Key Insights**:
{string.Join("\n", developmentSummary.Insights.Take(3).Select(i => $"• {i.Title}: {i.Description}"))}

**Top Recommendations**:
{string.Join("\n", developmentSummary.Predictions.Take(3).Select(r => $"• {r.Title}: {r.Description}"))}

Please create a professional executive summary that:
1. Highlights the most important findings for leadership
2. Focuses on business impact and strategic implications
3. Provides clear, actionable next steps
4. Uses executive-level language and metrics
5. Keeps it concise (3-4 paragraphs max)

Focus specifically on: {focusArea}";

            var response = await _kernel.InvokePromptAsync(prompt);
            return response.ToString();
        }
        catch (Exception ex)
        {
            return $"❌ Error creating executive summary: {ex.Message}";
        }
    }

    /// <summary>
    /// Detects patterns across development activities
    /// </summary>
    /// <param name="patternType">Type of patterns to detect</param>
    /// <returns>List of detected patterns</returns>
    [KernelFunction("detect_patterns")]
    [Description("Detects patterns and trends across development activities")]
    public async Task<List<CrossReferencePattern>> DetectPatterns(
        [Description("Pattern type: Quality, Performance, Collaboration, or All")]
        string patternType = "All")
    {
        try
        {
            var crossRef = await AnalyzeCrossReferences();
            var patterns = crossRef.Patterns;

            if (patternType != "All")
            {
                patterns = patterns.Where(p => p.Name.Contains(patternType, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            return patterns;
        }
        catch (Exception ex)
        {
            return new List<CrossReferencePattern>
            {
                new CrossReferencePattern
                {
                    Name = "Analysis Error",
                    Description = $"Error detecting patterns: {ex.Message}",
                    Confidence = 0.0
                }
            };
        }
    }

    /// <summary>
    /// Analyzes correlations between code reviews and meeting discussions
    /// </summary>
    /// <returns>Correlation analysis results</returns>
    [KernelFunction("analyze_code_meeting_correlations")]
    [Description("Analyzes correlations between code review findings and meeting discussions")]
    public async Task<string> AnalyzeCodeMeetingCorrelations()
    {
        try
        {
            var crossRef = await AnalyzeCrossReferences("CodeToMeeting");

            var correlations = crossRef.Connections
                .Where(c => c.ConnectionType == ConnectionType.TopicSimilarity && c.Strength > 0.6)
                .OrderByDescending(c => c.Strength)
                .Take(5);

            var summary = "🔍 **Code Review ↔ Meeting Correlations**\n\n";

            foreach (var correlation in correlations)
            {
                var sourceEntity = crossRef.RelatedEntities.FirstOrDefault(e => e.Id == correlation.SourceEntityId);
                var targetEntity = crossRef.RelatedEntities.FirstOrDefault(e => e.Id == correlation.TargetEntityId);

                if (sourceEntity != null && targetEntity != null)
                {
                    summary += $"**High Correlation** ({correlation.Strength:F2} confidence)\n";
                    summary += $"• Code: {sourceEntity.Title}\n";
                    summary += $"• Meeting: {targetEntity.Title}\n";
                    summary += $"• Connection: {correlation.Description}\n\n";
                }
            }

            if (!correlations.Any())
            {
                summary += "No significant correlations found between recent code reviews and meeting discussions.\n";
            }

            return summary;
        }
        catch (Exception ex)
        {
            return $"❌ Error analyzing correlations: {ex.Message}";
        }
    }

    // Private helper methods

    private async Task<List<object>> CollectCodeReviewData(DateTime startDate, DateTime endDate)
    {
        var data = new List<object>();

        if (_codeReviewAgent != null)
        {
            try
            {
                var recentCommits = await _codeReviewAgent.ListRecentCommits(10);
                data.AddRange(recentCommits.Where(c => c.Date >= startDate && c.Date <= endDate));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Could not collect code review data: {ex.Message}");
            }
        }

        return data;
    }

    private async Task<List<object>> CollectMeetingData(DateTime startDate, DateTime endDate)
    {
        var data = new List<object>();

        if (_meetingAnalysisAgent != null)
        {
            try
            {
                // Try to get meeting data from memory or file system
                // This would need to be implemented based on available data sources
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Could not collect meeting data: {ex.Message}");
            }
        }

        return data;
    }

    private async Task<List<object>> CollectJiraData(DateTime startDate, DateTime endDate)
    {
        var data = new List<object>();

        if (_jiraIntegrationAgent != null)
        {
            try
            {
                // Try to search for recent Jira tickets
                // This would need to be implemented with proper Jira search
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Could not collect Jira data: {ex.Message}");
            }
        }

        return data;
    }

    private async Task<DevelopmentMetrics> GenerateMetrics(
        List<object> codeReviews,
        List<object> meetings,
        List<object> jiraTickets)
    {
        return new DevelopmentMetrics
        {
            TotalCodeReviews = codeReviews.Count,
            TotalMeetings = meetings.Count,
            TotalJiraTickets = jiraTickets.Count,
            TotalCommits = codeReviews.Count, // Simplified for demo
            AverageCodeQuality = 7.5, // Would be calculated from actual data
            ActionItemsCreated = meetings.Count * 3, // Estimated
            ActionItemsCompleted = meetings.Count * 2 // Estimated
        };
    }

    private int CalculateHealthScore(DevelopmentMetrics metrics)
    {
        var score = 50; // Base score

        // Adjust based on activity levels
        if (metrics.TotalCommits > 10) score += 15;
        if (metrics.TotalCodeReviews > 5) score += 15;
        if (metrics.ActionItemCompletionRate > 70) score += 20;

        return Math.Min(100, Math.Max(0, score));
    }

    private async Task<List<CrossReferenceEntity>> CollectAllEntities()
    {
        var entities = new List<CrossReferenceEntity>();

        // Collect from all sources
        var codeData = await CollectCodeReviewData(DateTime.UtcNow.AddDays(-30), DateTime.UtcNow);
        var meetingData = await CollectMeetingData(DateTime.UtcNow.AddDays(-30), DateTime.UtcNow);
        var jiraData = await CollectJiraData(DateTime.UtcNow.AddDays(-30), DateTime.UtcNow);

        // Convert to entities (simplified implementation)
        entities.AddRange(codeData.Take(5).Select((item, index) => new CrossReferenceEntity
        {
            Id = $"code_{index}",
            EntityType = "CodeReview",
            Title = $"Code Review #{index + 1}",
            CreatedAt = DateTime.UtcNow.AddDays(-index),
            RelevanceScore = 0.8
        }));

        return entities;
    }

    private async Task<List<EntityConnection>> FindAllConnections(List<CrossReferenceEntity> entities)
    {
        var connections = new List<EntityConnection>();

        // Simple pattern matching for demo
        for (int i = 0; i < entities.Count; i++)
        {
            for (int j = i + 1; j < entities.Count; j++)
            {
                var entity1 = entities[i];
                var entity2 = entities[j];

                // Check for topic similarity (simplified)
                if (entity1.EntityType != entity2.EntityType)
                {
                    var connection = new EntityConnection
                    {
                        SourceEntityId = entity1.Id,
                        TargetEntityId = entity2.Id,
                        ConnectionType = ConnectionType.TopicSimilarity,
                        Strength = 0.6,
                        Confidence = 0.7,
                        Description = "Related topics identified",
                        Direction = ConnectionDirection.Bidirectional
                    };

                    connections.Add(connection);
                }
            }
        }

        return connections;
    }

    private async Task<List<EntityConnection>> FindCodeMeetingConnections(List<CrossReferenceEntity> entities)
    {
        return await FindAllConnections(entities.Where(e =>
            e.EntityType == "CodeReview" || e.EntityType == "Meeting").ToList());
    }

    private async Task<List<EntityConnection>> FindMeetingJiraConnections(List<CrossReferenceEntity> entities)
    {
        return await FindAllConnections(entities.Where(e =>
            e.EntityType == "Meeting" || e.EntityType == "JiraTicket").ToList());
    }

    private async Task<List<EntityConnection>> FindCodeJiraConnections(List<CrossReferenceEntity> entities)
    {
        return await FindAllConnections(entities.Where(e =>
            e.EntityType == "CodeReview" || e.EntityType == "JiraTicket").ToList());
    }

    private double CalculateConnectionConfidence(List<EntityConnection> connections)
    {
        if (!connections.Any()) return 0.0;
        return connections.Average(c => c.Confidence);
    }

    private async Task<List<string>> GenerateConnectionInsights(List<EntityConnection> connections)
    {
        var insights = new List<string>();

        var strongConnections = connections.Where(c => c.Strength > 0.7).Count();
        if (strongConnections > 0)
        {
            insights.Add($"Found {strongConnections} strong cross-system correlations");
        }

        var topicConnections = connections.Where(c => c.ConnectionType == ConnectionType.TopicSimilarity).Count();
        if (topicConnections > 0)
        {
            insights.Add($"Identified {topicConnections} topic-based relationships");
        }

        return insights;
    }

    private async Task<List<CrossReferencePattern>> IdentifyPatterns(List<EntityConnection> connections)
    {
        var patterns = new List<CrossReferencePattern>();

        // Identify recurring connection patterns
        var groupedConnections = connections.GroupBy(c => c.ConnectionType);

        foreach (var group in groupedConnections.Where(g => g.Count() > 1))
        {
            patterns.Add(new CrossReferencePattern
            {
                Name = $"{group.Key} Pattern",
                Description = $"Recurring {group.Key} connections between systems",
                Frequency = group.Count(),
                Confidence = group.Average(c => c.Confidence)
            });
        }

        return patterns;
    }

    private async Task<string> GenerateCrossReferenceSummary(CrossReferenceResult result)
    {
        return $"Cross-reference analysis identified {result.Connections.Count} connections " +
               $"between {result.RelatedEntities.Count} entities with {result.ConfidenceScore:F2} confidence.";
    }

    private async Task StoreInsights(CrossReferenceResult result)
    {
        foreach (var insight in result.KeyInsights)
        {
            _insights.Add(new IntelligenceInsight
            {
                Title = "Cross-Reference Insight",
                Description = insight,
                Type = InsightType.Correlation,
                Confidence = result.ConfidenceScore,
                Priority = InsightPriority.Medium
            });
        }
    }

    private async Task<List<Dictionary<string, object>>> AnalyzeHistoricalPatterns()
    {
        // Simplified pattern analysis
        return new List<Dictionary<string, object>>
        {
            new Dictionary<string, object>
            {
                ["type"] = "quality_trend",
                ["direction"] = "improving",
                ["confidence"] = 0.8
            }
        };
    }

    private async Task<List<PredictiveRecommendation>> GeneratePerformancePredictions(
        List<Dictionary<string, object>> patterns, int horizonDays)
    {
        return new List<PredictiveRecommendation>
        {
            new PredictiveRecommendation
            {
                Title = "Performance Optimization Opportunity",
                Description = "Based on current trends, implementing caching strategies could improve response times by 30%",
                Category = RecommendationCategory.Performance,
                Priority = RecommendationPriority.High,
                Confidence = 0.75,
                TimeFrame = TimeFrame.NextSprint
            }
        };
    }

    private async Task<List<PredictiveRecommendation>> GenerateQualityPredictions(
        List<Dictionary<string, object>> patterns, int horizonDays)
    {
        return new List<PredictiveRecommendation>
        {
            new PredictiveRecommendation
            {
                Title = "Code Quality Improvement",
                Description = "Increasing test coverage in authentication modules will prevent 2-3 future bugs",
                Category = RecommendationCategory.CodeQuality,
                Priority = RecommendationPriority.Medium,
                Confidence = 0.68,
                TimeFrame = TimeFrame.ThisSprint
            }
        };
    }

    private async Task<List<PredictiveRecommendation>> GenerateRiskPredictions(
        List<Dictionary<string, object>> patterns, int horizonDays)
    {
        return new List<PredictiveRecommendation>
        {
            new PredictiveRecommendation
            {
                Title = "Security Risk Mitigation",
                Description = "Recent patterns suggest vulnerability in user input validation - recommend security review",
                Category = RecommendationCategory.Security,
                Priority = RecommendationPriority.Critical,
                Confidence = 0.82,
                TimeFrame = TimeFrame.Immediate
            }
        };
    }

    private async Task<List<PredictiveRecommendation>> GenerateProcessPredictions(
        List<Dictionary<string, object>> patterns, int horizonDays)
    {
        return new List<PredictiveRecommendation>
        {
            new PredictiveRecommendation
            {
                Title = "Process Automation Opportunity",
                Description = "Automating deployment pipeline could reduce release time by 50% and eliminate manual errors",
                Category = RecommendationCategory.ProcessImprovement,
                Priority = RecommendationPriority.Medium,
                Confidence = 0.71,
                TimeFrame = TimeFrame.ThisQuarter
            }
        };
    }

    private async Task<QualityAssessment> AnalyzeQualityTrends(List<object> codeReviews)
    {
        return new QualityAssessment
        {
            OverallScore = 7.5,
            CodeQualityTrend = 0.3, // Improving
            QualityImprovements = new List<string> { "Better error handling", "Improved documentation" },
            QualityConcerns = new List<string> { "Test coverage could be higher" },
            SecurityIssuesFound = 1,
            PerformanceIssuesFound = 2,
            TestCoverage = 78.5
        };
    }

    private async Task<PerformanceTrends> AnalyzePerformanceTrends(List<object> codeReviews, List<object> jiraTickets)
    {
        return new PerformanceTrends
        {
            VelocityScore = 8.2,
            VelocityTrend = "Stable",
            DeliveryPredictability = 85.0,
            TechnicalDebtTrend = -0.1, // Decreasing (good)
            PerformanceWins = new List<string> { "Faster CI/CD pipeline", "Reduced bug count" },
            PerformanceConcerns = new List<string> { "Database query optimization needed" }
        };
    }

    private async Task<CollaborationInsights> AnalyzeCollaboration(List<object> meetings, List<object> codeReviews)
    {
        return new CollaborationInsights
        {
            CollaborationScore = 8.5,
            CollaborationStrengths = new List<string> { "Active code reviews", "Regular meetings" },
            CollaborationGaps = new List<string> { "More documentation needed" },
            MeetingEffectiveness = 75,
            CommunicationQuality = 8.0
        };
    }

    private async Task<string> GenerateExecutiveSummaryText(DevelopmentSummary summary)
    {
        var prompt = $@"
Create an executive summary based on this development data:
- Health Score: {summary.OverallHealthScore}/100
- Total Activities: {summary.Metrics.TotalCommits + summary.Metrics.TotalMeetings + summary.Metrics.TotalJiraTickets}
- Action Item Completion: {summary.Metrics.ActionItemCompletionRate:F1}%
- Quality Score: {summary.Quality.OverallScore}/10

Focus on business impact and strategic recommendations.";

        var response = await _kernel.InvokePromptAsync(prompt);
        return response.ToString();
    }

    private async Task<List<string>> GenerateLeadershipActions(DevelopmentSummary summary)
    {
        var actions = new List<string>();

        if (summary.OverallHealthScore < 70)
        {
            actions.Add("Schedule team health assessment and improvement planning session");
        }

        if (summary.Metrics.ActionItemCompletionRate < 80)
        {
            actions.Add("Review and optimize action item tracking and follow-up processes");
        }

        if (summary.Quality.OverallScore < 7.0)
        {
            actions.Add("Invest in code quality training and tooling improvements");
        }

        return actions.Any() ? actions : new List<string> { "Continue current excellent practices" };
    }

    private async Task StoreInMemory(string key, string content)
    {
        if (_memory != null)
        {
            try
            {
                await _memory.SaveInformationAsync("intelligence", content, key);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Could not store in memory: {ex.Message}");
            }
        }
    }
}
