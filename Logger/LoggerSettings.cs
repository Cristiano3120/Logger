namespace Cristiano3120.Logging;

/// <summary>
/// Represents the configuration settings used to control logging behavior, output destinations, log file management,
/// and message formatting for a logger.
/// </summary>
/// <remarks>Use this record to specify options such as the log directory path, maximum number of log files to
/// retain, log level filtering, and console color customization for different message types. Disabling console or file
/// logging, or adjusting reflection usage, can be configured to suit application requirements. All properties are
/// immutable and should be set at initialization.</remarks>
public sealed record LoggerSettings
{
    /// <summary>
    /// Gets the path to the directory where log files are stored. <br></br>
    /// <c>Default:</c> .../Logs <br></br>
    /// If you just use the default value the "Logs" folder will be placed in the current working directory <br></br>
    /// <c>Example for Vs:</c> "C:\Users\Crist\source\repos\Logger\LoggerTest\bin\Debug\net10.0\Logs"
    /// </summary>
    public string PathToLogDirectory { get; init; } = "Logs";

    /// <summary>
    /// Gets the maximum number of log files that can be saved before old ones get deleted. <br></br>
    /// <c>Default:</c> 10
    /// </summary>
    public int MaxAmmountOfLoggingFiles { get; init; } = 10;

    /// <summary>
    /// Gets a value indicating whether console logging is deactivated.
    /// </summary>
    public bool DeactivateConsoleLogging { get; init; }

    /// <summary>
    /// Gets a value indicating whether file logging is deactivated.
    /// </summary>
    public bool DeactivateFileLogging { get; init; }

    /// <summary>
    /// When set to <c>true</c>, properties will not be read via reflection. <br />
    /// This may provide a slight performance improvement, 
    /// but in most cases the effect is negligible.
    /// </summary>
    public bool DeactivateReflection { get; init; }

    /// <summary>
    /// Gets the minimum log level for messages to be recorded by the logger.
    /// </summary>
    /// <remarks>Messages with a severity lower than this level are ignored. Adjust this property to control
    /// the verbosity of log output.</remarks>
    public LogLevel LogLevel { get; init; } = LogLevel.Information;

    /// <summary>
    /// Gets the console color used to display informational messages.
    /// </summary>
    public ConsoleColor InformationColor { get; init; } = ConsoleColor.Green;

    /// <summary>
    /// Gets the console color used to display warning messages.
    /// </summary>
    public ConsoleColor WarningColor { get; init; } = ConsoleColor.DarkYellow;

    /// <summary>
    /// Gets the color used to display error messages in the console.
    /// </summary>
    public ConsoleColor ErrorColor { get; init; } = ConsoleColor.Red;

    /// <summary>
    /// Gets the console color used to display debug messages.
    /// </summary>
    public ConsoleColor DebugColor { get; init; } = ConsoleColor.DarkMagenta;

    /// <summary>
    /// Gets the color used to display received data in the console. <br></br>
    /// This color is used when for example data is received via Http or a socket.
    /// </summary>
    public ConsoleColor ReceivedDataColor { get; init; } = ConsoleColor.Blue;

    /// <summary>
    /// Gets the console color used to display sent data. <br></br>
    /// This color is used when for example data is sent via Http or a socket.
    /// </summary>
    public ConsoleColor SentDataColor { get; init; } = ConsoleColor.Cyan;
}
