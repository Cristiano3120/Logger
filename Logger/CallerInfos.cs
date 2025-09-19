using System.Runtime.CompilerServices;

namespace Cristiano3120.Logging;

/// <summary>
/// <c>CANT INSTANTIATE.</c> Call <see cref="Create"/> instead.
/// <para>
/// This class contains infos about a specific place in code. <br></br>
/// This is usually used to get infos about where an error accoured
/// </para>
/// </summary>
public sealed record CallerInfos
{
    /// <summary>
    /// Gets the name of the caller/method associated with the current context.
    /// </summary>
    public string CallerName { get; init; }
    
    /// <summary>
    /// Gets the full path to the file associated with this instance.
    /// </summary>
    public string FilePath { get; init; }
    
    /// <summary>
    /// Gets the line number associated with this instance.
    /// </summary>
    public int LineNum { get; init; }

    private CallerInfos(string callerName, string filePath, int lineNum)
    {
        CallerName = callerName;
        FilePath = filePath;
        LineNum = lineNum;
    }

    /// <summary>
    /// Creates a new instance of the CallerInfos class containing information about the calling member, source file,
    /// and line number.
    /// </summary>
    /// <remarks>This method is typically used for logging, diagnostics, or tracing purposes to capture caller
    /// context. The parameter values are provided by the compiler using Caller Information attributes and do not need
    /// to be specified manually.</remarks>
    /// <param name="callerName">The name of the method or property that invoked this method. This value is automatically supplied by the
    /// compiler and should not be set explicitly in most cases.</param>
    /// <param name="filePath">The full path of the source file that contains the caller. This value is automatically supplied by the compiler.</param>
    /// <param name="lineNum">The line number in the source file at which this method is called. This value is automatically supplied by the
    /// compiler.</param>
    /// <returns>A CallerInfos instance populated with the caller's member name, file path, and line number.</returns>
    public static CallerInfos Create([CallerMemberName] string callerName = "", [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNum = 0)
    => new(callerName, filePath, lineNum);
}
