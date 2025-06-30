using Microsoft.SemanticKernel;
using System.ComponentModel;
using System.Text;
using System.Text.Json;
using System.Net.Http.Headers;
using SemanticKernelDevHub.Models;

namespace SemanticKernelDevHub.Plugins;

/// <summary>
/// Plugin for integrating with Jira REST API
/// </summary>
public class JiraPlugin
{
    private readonly HttpClient _httpClient;
    private readonly string _jiraUrl;
    private readonly string _email;
    private readonly string _apiToken;
    private readonly string _projectKey;
    private readonly string _baseApiUrl;

    public JiraPlugin(string jiraUrl, string email, string apiToken, string projectKey)
    {
        _jiraUrl = jiraUrl.TrimEnd('/');
        _email = email;
        _apiToken = apiToken;
        _projectKey = projectKey;
        _baseApiUrl = $"{_jiraUrl}/rest/api/3";

        _httpClient = new HttpClient();
        
        // Set up basic authentication
        var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_email}:{_apiToken}"));
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    /// <summary>
    /// Tests connection to Jira
    /// </summary>
    /// <returns>Connection test result</returns>
    [KernelFunction("test_jira_connection")]
    [Description("Tests the connection to Jira and validates credentials")]
    public async Task<string> TestConnection()
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_baseApiUrl}/myself");
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var user = JsonSerializer.Deserialize<JsonElement>(content);
                var displayName = user.GetProperty("displayName").GetString();
                return $"✅ Jira connection successful! Connected as: {displayName}";
            }
            else
            {
                return $"❌ Jira connection failed: {response.StatusCode} - {response.ReasonPhrase}";
            }
        }
        catch (Exception ex)
        {
            return $"❌ Jira connection error: {ex.Message}";
        }
    }

    /// <summary>
    /// Creates a new Jira ticket
    /// </summary>
    /// <param name="title">Ticket title/summary</param>
    /// <param name="description">Ticket description</param>
    /// <param name="priority">Priority level (High, Medium, Low)</param>
    /// <param name="issueType">Issue type (Task, Bug, Story)</param>
    /// <returns>Created ticket information</returns>
    [KernelFunction("create_jira_ticket")]
    [Description("Creates a new Jira ticket with the specified details")]
    public async Task<JiraOperationResult> CreateTicket(
        [Description("Title/summary of the ticket")] string title,
        [Description("Detailed description of the ticket")] string description,
        [Description("Priority level: High, Medium, Low")] string priority = "Medium",
        [Description("Issue type: Task, Bug, Story")] string issueType = "Task")
    {
        var result = new JiraOperationResult();

        try
        {
            // Map priority to Jira priority ID
            var priorityId = priority.ToUpper() switch
            {
                "HIGH" => "1",
                "MEDIUM" => "3", 
                "LOW" => "4",
                _ => "3" // Default to Medium
            };

            // Map issue type to Jira issue type ID
            var issueTypeId = issueType.ToUpper() switch
            {
                "BUG" => "10004",
                "STORY" => "10001",
                "TASK" => "10003",
                _ => "10003" // Default to Task
            };

            var ticketData = new
            {
                fields = new
                {
                    project = new { key = _projectKey },
                    summary = title,
                    description = new
                    {
                        type = "doc",
                        version = 1,
                        content = new[]
                        {
                            new
                            {
                                type = "paragraph",
                                content = new[]
                                {
                                    new
                                    {
                                        type = "text",
                                        text = description
                                    }
                                }
                            }
                        }
                    },
                    issuetype = new { id = issueTypeId },
                    priority = new { id = priorityId }
                }
            };

            var json = JsonSerializer.Serialize(ticketData, new JsonSerializerOptions { WriteIndented = true });
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_baseApiUrl}/issue", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var createdIssue = JsonSerializer.Deserialize<JsonElement>(responseContent);
                var ticketKey = createdIssue.GetProperty("key").GetString();
                var ticketUrl = $"{_jiraUrl}/browse/{ticketKey}";

                result.Success = true;
                result.TicketKey = ticketKey;
                result.Message = $"✅ Ticket {ticketKey} created successfully";
                result.TicketUrl = ticketUrl;
                result.OperationType = "CREATE";
            }
            else
            {
                result.Success = false;
                result.Message = $"❌ Failed to create ticket: {response.StatusCode} - {responseContent}";
            }
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.Message = $"❌ Error creating ticket: {ex.Message}";
        }

        return result;
    }

    /// <summary>
    /// Adds a comment to an existing Jira ticket
    /// </summary>
    /// <param name="ticketKey">Jira ticket key (e.g., OPS-123)</param>
    /// <param name="comment">Comment text to add</param>
    /// <returns>Operation result</returns>
    [KernelFunction("add_jira_comment")]
    [Description("Adds a comment to an existing Jira ticket")]
    public async Task<JiraOperationResult> AddComment(
        [Description("Jira ticket key (e.g., OPS-123)")] string ticketKey,
        [Description("Comment text to add to the ticket")] string comment)
    {
        var result = new JiraOperationResult();

        try
        {
            var commentData = new
            {
                body = new
                {
                    type = "doc",
                    version = 1,
                    content = new[]
                    {
                        new
                        {
                            type = "paragraph",
                            content = new[]
                            {
                                new
                                {
                                    type = "text",
                                    text = comment
                                }
                            }
                        }
                    }
                }
            };

            var json = JsonSerializer.Serialize(commentData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_baseApiUrl}/issue/{ticketKey}/comment", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                result.Success = true;
                result.TicketKey = ticketKey;
                result.Message = $"✅ Comment added to ticket {ticketKey}";
                result.TicketUrl = $"{_jiraUrl}/browse/{ticketKey}";
                result.OperationType = "COMMENT";
            }
            else
            {
                result.Success = false;
                result.Message = $"❌ Failed to add comment to {ticketKey}: {response.StatusCode} - {responseContent}";
            }
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.Message = $"❌ Error adding comment to {ticketKey}: {ex.Message}";
        }

        return result;
    }

    /// <summary>
    /// Searches for Jira tickets by key or title
    /// </summary>
    /// <param name="searchTerm">Search term (ticket key or title keywords)</param>
    /// <returns>List of matching tickets</returns>
    [KernelFunction("search_jira_tickets")]
    [Description("Searches for Jira tickets by key or title keywords")]
    public async Task<List<JiraTicket>> SearchTickets(
        [Description("Search term - ticket key (OPS-123) or title keywords")] string searchTerm)
    {
        var tickets = new List<JiraTicket>();

        try
        {
            string jql;
            
            // Check if search term looks like a ticket key
            if (System.Text.RegularExpressions.Regex.IsMatch(searchTerm, @"^[A-Z]+-\d+$"))
            {
                jql = $"key = \"{searchTerm}\"";
            }
            else
            {
                jql = $"project = \"{_projectKey}\" AND summary ~ \"{searchTerm}\"";
            }

            var encodedJql = Uri.EscapeDataString(jql);
            var response = await _httpClient.GetAsync($"{_baseApiUrl}/search?jql={encodedJql}&fields=key,summary,description,priority,status,issuetype");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var searchResult = JsonSerializer.Deserialize<JsonElement>(content);
                var issues = searchResult.GetProperty("issues").EnumerateArray();

                foreach (var issue in issues)
                {
                    var fields = issue.GetProperty("fields");
                    
                    var ticket = new JiraTicket
                    {
                        Key = issue.GetProperty("key").GetString() ?? "",
                        Title = fields.GetProperty("summary").GetString() ?? "",
                        Priority = fields.TryGetProperty("priority", out var priority) 
                            ? priority.GetProperty("name").GetString() ?? "Medium"
                            : "Medium",
                        Status = fields.TryGetProperty("status", out var status)
                            ? status.GetProperty("name").GetString() ?? "Unknown"
                            : "Unknown",
                        IssueType = fields.TryGetProperty("issuetype", out var issueType)
                            ? issueType.GetProperty("name").GetString() ?? "Task"
                            : "Task",
                        Url = $"{_jiraUrl}/browse/{issue.GetProperty("key").GetString()}"
                    };

                    // Extract description if available
                    if (fields.TryGetProperty("description", out var desc) && desc.ValueKind != JsonValueKind.Null)
                    {
                        ticket.Description = ExtractTextFromADF(desc);
                    }

                    tickets.Add(ticket);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error searching tickets: {ex.Message}");
        }

        return tickets;
    }

    /// <summary>
    /// Gets details of a specific Jira ticket
    /// </summary>
    /// <param name="ticketKey">Jira ticket key (e.g., OPS-123)</param>
    /// <returns>Ticket details</returns>
    [KernelFunction("get_jira_ticket")]
    [Description("Gets detailed information about a specific Jira ticket")]
    public async Task<JiraTicket?> GetTicket(
        [Description("Jira ticket key (e.g., OPS-123)")] string ticketKey)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_baseApiUrl}/issue/{ticketKey}?fields=key,summary,description,priority,status,issuetype,assignee,reporter,created,updated");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var issue = JsonSerializer.Deserialize<JsonElement>(content);
                var fields = issue.GetProperty("fields");

                var ticket = new JiraTicket
                {
                    Key = issue.GetProperty("key").GetString() ?? "",
                    Title = fields.GetProperty("summary").GetString() ?? "",
                    Priority = fields.TryGetProperty("priority", out var priority)
                        ? priority.GetProperty("name").GetString() ?? "Medium"
                        : "Medium",
                    Status = fields.TryGetProperty("status", out var status)
                        ? status.GetProperty("name").GetString() ?? "Unknown"
                        : "Unknown",
                    IssueType = fields.TryGetProperty("issuetype", out var issueType)
                        ? issueType.GetProperty("name").GetString() ?? "Task"
                        : "Task",
                    Url = $"{_jiraUrl}/browse/{ticketKey}"
                };

                // Extract description
                if (fields.TryGetProperty("description", out var desc) && desc.ValueKind != JsonValueKind.Null)
                {
                    ticket.Description = ExtractTextFromADF(desc);
                }

                // Extract assignee and reporter
                if (fields.TryGetProperty("assignee", out var assignee) && assignee.ValueKind != JsonValueKind.Null)
                {
                    ticket.Assignee = assignee.GetProperty("displayName").GetString();
                }

                if (fields.TryGetProperty("reporter", out var reporter) && reporter.ValueKind != JsonValueKind.Null)
                {
                    ticket.Reporter = reporter.GetProperty("displayName").GetString();
                }

                // Extract dates
                if (fields.TryGetProperty("created", out var created))
                {
                    if (DateTime.TryParse(created.GetString(), out var createdDate))
                        ticket.CreatedDate = createdDate;
                }

                if (fields.TryGetProperty("updated", out var updated))
                {
                    if (DateTime.TryParse(updated.GetString(), out var updatedDate))
                        ticket.UpdatedDate = updatedDate;
                }

                return ticket;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error getting ticket {ticketKey}: {ex.Message}");
        }

        return null;
    }

    /// <summary>
    /// Updates an existing Jira ticket
    /// </summary>
    /// <param name="ticketKey">Jira ticket key (e.g., OPS-123)</param>
    /// <param name="title">New title (optional)</param>
    /// <param name="description">New description (optional)</param>
    /// <param name="priority">New priority (optional)</param>
    /// <returns>Operation result</returns>
    [KernelFunction("update_jira_ticket")]
    [Description("Updates an existing Jira ticket with new information")]
    public async Task<JiraOperationResult> UpdateTicket(
        [Description("Jira ticket key (e.g., OPS-123)")] string ticketKey,
        [Description("New title/summary (optional)")] string? title = null,
        [Description("New description (optional)")] string? description = null,
        [Description("New priority: High, Medium, Low (optional)")] string? priority = null)
    {
        var result = new JiraOperationResult();

        try
        {
            var updateFields = new Dictionary<string, object>();

            if (!string.IsNullOrEmpty(title))
            {
                updateFields["summary"] = title;
            }

            if (!string.IsNullOrEmpty(description))
            {
                updateFields["description"] = new
                {
                    type = "doc",
                    version = 1,
                    content = new[]
                    {
                        new
                        {
                            type = "paragraph",
                            content = new[]
                            {
                                new
                                {
                                    type = "text",
                                    text = description
                                }
                            }
                        }
                    }
                };
            }

            if (!string.IsNullOrEmpty(priority))
            {
                var priorityId = priority.ToUpper() switch
                {
                    "HIGH" => "1",
                    "MEDIUM" => "3",
                    "LOW" => "4",
                    _ => "3"
                };
                updateFields["priority"] = new { id = priorityId };
            }

            if (updateFields.Any())
            {
                var updateData = new { fields = updateFields };
                var json = JsonSerializer.Serialize(updateData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync($"{_baseApiUrl}/issue/{ticketKey}", content);

                if (response.IsSuccessStatusCode)
                {
                    result.Success = true;
                    result.TicketKey = ticketKey;
                    result.Message = $"✅ Ticket {ticketKey} updated successfully";
                    result.TicketUrl = $"{_jiraUrl}/browse/{ticketKey}";
                    result.OperationType = "UPDATE";
                }
                else
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    result.Success = false;
                    result.Message = $"❌ Failed to update ticket {ticketKey}: {response.StatusCode} - {responseContent}";
                }
            }
            else
            {
                result.Success = false;
                result.Message = "❌ No fields specified for update";
            }
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.Message = $"❌ Error updating ticket {ticketKey}: {ex.Message}";
        }

        return result;
    }

    /// <summary>
    /// Extracts plain text from Atlassian Document Format (ADF)
    /// </summary>
    /// <param name="adfContent">ADF JSON content</param>
    /// <returns>Plain text extracted from ADF</returns>
    private string ExtractTextFromADF(JsonElement adfContent)
    {
        var text = new StringBuilder();

        try
        {
            if (adfContent.TryGetProperty("content", out var content))
            {
                ExtractTextFromADFNode(content, text);
            }
        }
        catch
        {
            // If ADF parsing fails, try to get raw text
            return adfContent.ToString();
        }

        return text.ToString().Trim();
    }

    /// <summary>
    /// Recursively extracts text from ADF nodes
    /// </summary>
    private void ExtractTextFromADFNode(JsonElement node, StringBuilder text)
    {
        if (node.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in node.EnumerateArray())
            {
                ExtractTextFromADFNode(item, text);
            }
        }
        else if (node.ValueKind == JsonValueKind.Object)
        {
            if (node.TryGetProperty("type", out var type) && type.GetString() == "text")
            {
                if (node.TryGetProperty("text", out var textNode))
                {
                    text.Append(textNode.GetString());
                }
            }

            if (node.TryGetProperty("content", out var content))
            {
                ExtractTextFromADFNode(content, text);
                text.Append(" "); // Add space between content blocks
            }
        }
    }

    /// <summary>
    /// Disposes of HTTP client resources
    /// </summary>
    public void Dispose()
    {
        _httpClient?.Dispose();
    }
}
