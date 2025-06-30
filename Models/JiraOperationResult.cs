namespace SemanticKernelDevHub.Models;

/// <summary>
/// Represents the result of a Jira operation
/// </summary>
public class JiraOperationResult
{
    /// <summary>
    /// Whether the operation was successful
    /// </summary>
    public bool Success { get; set; } = false;

    /// <summary>
    /// Result message describing the operation outcome
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Jira ticket key involved in the operation
    /// </summary>
    public string? TicketKey { get; set; }

    /// <summary>
    /// Direct URL to the ticket
    /// </summary>
    public string? TicketUrl { get; set; }

    /// <summary>
    /// Type of operation performed
    /// </summary>
    public string OperationType { get; set; } = string.Empty;

    /// <summary>
    /// Operation timestamp
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.Now;

    /// <summary>
    /// Additional data returned from the operation
    /// </summary>
    public Dictionary<string, object> Data { get; set; } = new();

    /// <summary>
    /// Error details if operation failed
    /// </summary>
    public string? ErrorDetails { get; set; }

    /// <summary>
    /// HTTP status code if applicable
    /// </summary>
    public int? StatusCode { get; set; }

    /// <summary>
    /// Duration of the operation
    /// </summary>
    public TimeSpan? Duration { get; set; }

    /// <summary>
    /// Creates a successful operation result
    /// </summary>
    /// <param name="message">Success message</param>
    /// <param name="ticketKey">Associated ticket key</param>
    /// <param name="operationType">Type of operation</param>
    /// <returns>Success result</returns>
    public static JiraOperationResult SuccessResult(string message, string? ticketKey = null, string operationType = "")
    {
        return new JiraOperationResult
        {
            Success = true,
            Message = message,
            TicketKey = ticketKey,
            OperationType = operationType,
            Timestamp = DateTime.Now
        };
    }

    /// <summary>
    /// Creates a failed operation result
    /// </summary>
    /// <param name="message">Error message</param>
    /// <param name="errorDetails">Detailed error information</param>
    /// <param name="operationType">Type of operation</param>
    /// <returns>Failure result</returns>
    public static JiraOperationResult Failure(string message, string? errorDetails = null, string operationType = "")
    {
        return new JiraOperationResult
        {
            Success = false,
            Message = message,
            ErrorDetails = errorDetails,
            OperationType = operationType,
            Timestamp = DateTime.Now
        };
    }

    /// <summary>
    /// Creates a result for ticket creation operation
    /// </summary>
    /// <param name="success">Whether creation was successful</param>
    /// <param name="ticketKey">Created ticket key</param>
    /// <param name="ticketUrl">URL to the created ticket</param>
    /// <param name="message">Operation message</param>
    /// <returns>Ticket creation result</returns>
    public static JiraOperationResult CreateTicketResult(bool success, string? ticketKey, string? ticketUrl, string message)
    {
        return new JiraOperationResult
        {
            Success = success,
            Message = message,
            TicketKey = ticketKey,
            TicketUrl = ticketUrl,
            OperationType = "CREATE_TICKET"
        };
    }

    /// <summary>
    /// Creates a result for comment addition operation
    /// </summary>
    /// <param name="success">Whether addition was successful</param>
    /// <param name="ticketKey">Target ticket key</param>
    /// <param name="message">Operation message</param>
    /// <returns>Comment addition result</returns>
    public static JiraOperationResult AddCommentResult(bool success, string ticketKey, string message)
    {
        return new JiraOperationResult
        {
            Success = success,
            Message = message,
            TicketKey = ticketKey,
            OperationType = "ADD_COMMENT"
        };
    }

    /// <summary>
    /// Creates a result for ticket update operation
    /// </summary>
    /// <param name="success">Whether update was successful</param>
    /// <param name="ticketKey">Updated ticket key</param>
    /// <param name="message">Operation message</param>
    /// <returns>Ticket update result</returns>
    public static JiraOperationResult UpdateTicketResult(bool success, string ticketKey, string message)
    {
        return new JiraOperationResult
        {
            Success = success,
            Message = message,
            TicketKey = ticketKey,
            OperationType = "UPDATE_TICKET"
        };
    }

    /// <summary>
    /// Adds additional data to the result
    /// </summary>
    /// <param name="key">Data key</param>
    /// <param name="value">Data value</param>
    /// <returns>Current instance for method chaining</returns>
    public JiraOperationResult WithData(string key, object value)
    {
        Data[key] = value;
        return this;
    }

    /// <summary>
    /// Sets the operation duration
    /// </summary>
    /// <param name="duration">Operation duration</param>
    /// <returns>Current instance for method chaining</returns>
    public JiraOperationResult WithDuration(TimeSpan duration)
    {
        Duration = duration;
        return this;
    }

    /// <summary>
    /// Sets the HTTP status code
    /// </summary>
    /// <param name="statusCode">HTTP status code</param>
    /// <returns>Current instance for method chaining</returns>
    public JiraOperationResult WithStatusCode(int statusCode)
    {
        StatusCode = statusCode;
        return this;
    }

    /// <summary>
    /// Gets a formatted summary of the operation result
    /// </summary>
    /// <returns>Formatted result summary</returns>
    public string GetFormattedSummary()
    {
        var icon = Success ? "✅" : "❌";
        var summary = $@"{icon} **Jira Operation Result**

**Operation**: {OperationType}
**Status**: {(Success ? "SUCCESS" : "FAILED")}
**Message**: {Message}
**Timestamp**: {Timestamp:yyyy-MM-dd HH:mm:ss}";

        if (!string.IsNullOrEmpty(TicketKey))
        {
            summary += $"\n**Ticket**: {TicketKey}";
        }

        if (!string.IsNullOrEmpty(TicketUrl))
        {
            summary += $"\n**URL**: {TicketUrl}";
        }

        if (Duration.HasValue)
        {
            summary += $"\n**Duration**: {Duration.Value.TotalMilliseconds:F0}ms";
        }

        if (StatusCode.HasValue)
        {
            summary += $"\n**HTTP Status**: {StatusCode}";
        }

        if (!Success && !string.IsNullOrEmpty(ErrorDetails))
        {
            summary += $"\n\n**Error Details**:\n{ErrorDetails}";
        }

        if (Data.Any())
        {
            summary += $"\n\n**Additional Data**:";
            foreach (var kvp in Data)
            {
                summary += $"\n• **{kvp.Key}**: {kvp.Value}";
            }
        }

        return summary;
    }

    /// <summary>
    /// Gets a short status text
    /// </summary>
    /// <returns>Short status representation</returns>
    public string GetShortStatus()
    {
        var status = Success ? "✅ SUCCESS" : "❌ FAILED";
        var operation = string.IsNullOrEmpty(OperationType) ? "" : $" ({OperationType})";
        var ticket = string.IsNullOrEmpty(TicketKey) ? "" : $" - {TicketKey}";
        
        return $"{status}{operation}{ticket}";
    }

    /// <summary>
    /// Checks if the operation was a specific type
    /// </summary>
    /// <param name="operationType">Operation type to check</param>
    /// <returns>True if operation matches the type</returns>
    public bool IsOperationType(string operationType)
    {
        return string.Equals(OperationType, operationType, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Gets the ticket key if operation was successful, null otherwise
    /// </summary>
    /// <returns>Ticket key or null</returns>
    public string? GetTicketKeyIfSuccessful()
    {
        return Success ? TicketKey : null;
    }

    /// <summary>
    /// Throws an exception if the operation failed
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if operation failed</exception>
    public void ThrowIfFailed()
    {
        if (!Success)
        {
            var error = string.IsNullOrEmpty(ErrorDetails) ? Message : $"{Message}\nDetails: {ErrorDetails}";
            throw new InvalidOperationException($"Jira operation failed: {error}");
        }
    }

    /// <summary>
    /// Converts the result to a dictionary for serialization
    /// </summary>
    /// <returns>Dictionary representation</returns>
    public Dictionary<string, object?> ToDictionary()
    {
        return new Dictionary<string, object?>
        {
            ["success"] = Success,
            ["message"] = Message,
            ["ticketKey"] = TicketKey,
            ["ticketUrl"] = TicketUrl,
            ["operationType"] = OperationType,
            ["timestamp"] = Timestamp,
            ["errorDetails"] = ErrorDetails,
            ["statusCode"] = StatusCode,
            ["duration"] = Duration?.TotalMilliseconds,
            ["data"] = Data
        };
    }

    public override string ToString() => GetShortStatus();
}
