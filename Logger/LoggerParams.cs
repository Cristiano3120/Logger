namespace Cristiano3120.Logging;

[Flags]
public enum LoggerParams : byte
{
    /// <summary>
    /// Standard logging behavior: log to console and file with timestamp and new line.
    /// </summary>
    None = 0,

    /// <summary>
    /// Indicates that the log should only be written to the file.
    /// </summary>
    NoConsole = 1,

    /// <summary>
    /// Indicates that the log should only be written to the console.
    /// </summary>
    NoFile = 2,

    /// <summary>
    /// Specifies that no newline character is appended after the output.
    /// If a newline character is appended, there will be an empty line after the log message.
    /// </summary>
    NoNewLine = 4
}
