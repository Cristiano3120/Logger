namespace Cristiano3120.Logging;

public sealed record LoggerSettings
{
    /// <summary>
    /// Gets the path to the directory where log files are stored. <br></br>
    /// <c>Default:</c> .../Logs
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
