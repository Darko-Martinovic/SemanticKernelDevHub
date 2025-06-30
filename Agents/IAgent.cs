using Microsoft.SemanticKernel;

namespace SemanticKernelDevHub.Agents;

/// <summary>
/// Common interface that all agents must implement
/// </summary>
public interface IAgent
{
    /// <summary>
    /// Gets the name of the agent
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the description of what this agent does
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Registers the agent's functions with the Semantic Kernel
    /// </summary>
    /// <param name="kernel">The Semantic Kernel instance</param>
    /// <returns>Task representing the async operation</returns>
    Task RegisterFunctionsAsync(Kernel kernel);

    /// <summary>
    /// Initializes the agent with any required setup
    /// </summary>
    /// <returns>Task representing the async operation</returns>
    Task InitializeAsync();

    /// <summary>
    /// Gets a list of function names that this agent provides
    /// </summary>
    /// <returns>List of function names</returns>
    IEnumerable<string> GetFunctionNames();
}
