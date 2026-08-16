namespace ABP.Domain.Result;

/// <summary>
/// Represents an error that prevented an application from performing an operation successfully. 
/// </summary>
/// <remarks>
/// A constructor of an `Error` object with an already created set of problems. 
/// </remarks>
/// <param name="title">A title of an error.</param>
/// <param name="problems">A dictionary of problems.</param>
public class Error(string title, IDictionary<string, string[]>? problems)
{
    
    /// <summary>
    /// Gets and initializes a title of an `Error` object.
    /// </summary>
    /// <value>What is the subject of an error.</value>
    public string? Title { get; protected init; } = title;

    /// <summary>
    /// Gets and initializes a set of problems. 
    /// </summary>
    /// <value>Set of string pairs describing what are the problems occured during a specific operation.</value>
    public virtual IDictionary<string, string[]>? Problems { get; protected init; } = problems;
}