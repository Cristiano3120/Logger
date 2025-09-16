namespace Cristiano3120.Logging;

/// <summary>
/// Specifies the level of logging.<br></br>
/// Ranges from None (0) to Debug (4).
/// </summary>
public enum LogLevel : byte
{
    /// <summary>
    /// No logging
    /// </summary>
    None = 0,

    /// <summary>
    /// Only logs errors
    /// </summary>
    Error = 1,

    /// <summary>
    /// Only logs warnings and errors
    /// </summary>
    Warning = 2,

    /// <summary>
    /// Logs informational messages, warnings and errors
    /// </summary>
    Information = 3,

    /// <summary>
    /// Logs all messages, including debug messages this is the most verbose level
    /// </summary>
    Debug = 4
}
