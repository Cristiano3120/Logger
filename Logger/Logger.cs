using System.Data;

namespace Cristiano3120.Logging;

public partial class Logger
{
    private readonly Lock _lock = new();
    private readonly LoggerSettings _loggerSettings;
    private readonly string _currentFile = "";
    private readonly Dictionary<Type, Action> _filterAtrributes;

    public Logger() : this(new LoggerSettings())
    { }

    public Logger(LoggerSettings loggerSettings)
    {
        _loggerSettings = loggerSettings;
        _currentFile = MaintainLoggingSystem();
        _filterAtrributes = new();
        Write(LoggerParams.NoNewLine, LogLevel.Debug, "Logger initialized");
    }

    /// <summary>
    /// Associates an action with a specified attribute type to be used as a filter.
    /// </summary>
    /// <remarks>If an action is already associated with the specified attribute type, it will be replaced by
    /// the new action.</remarks>
    /// <typeparam name="TAttribute">The type of attribute to associate with the filter action. Must derive from <see cref="Attribute"/>.</typeparam>
    /// <param name="action">The action to execute when the specified attribute type is encountered. Cannot be null.</param>
    public void AddAttributeToFilter<TAttribute>(Action action) where TAttribute : Attribute
    {
        _filterAtrributes[typeof(TAttribute)] = action;
    }

    /// <summary>
    /// Invokes the callback action associated with the current attribute type, if one is registered.
    /// </summary>
    private void ExecuteCallback(Attribute attr)
    {
        if (_filterAtrributes.TryGetValue(attr.GetType(), out Action? action))
        {
            action();
        }
    }

    private string MaintainLoggingSystem()
    {
        string pathToLoggingDic = _loggerSettings.PathToLogDirectory;
    
        if (!Directory.Exists(pathToLoggingDic))
        {
            _ = Directory.CreateDirectory(pathToLoggingDic);
        }
        else
        {
            string[] files = Directory.GetFiles(pathToLoggingDic, "*.md");

            if (files.Length >= _loggerSettings.MaxAmmountOfLoggingFiles)
            {
                files = [.. files.OrderBy(File.GetCreationTime)];
                // +1 to make room for a new File
                int filesToRemove = files.Length - _loggerSettings.MaxAmmountOfLoggingFiles + 1;

                for (int i = 0; i < filesToRemove; i++)
                {
                    File.Delete(files[i]);
                }
            }
        }

        string timestamp = DateTime.Now.ToString("dd-MM-yyyy/HH-mm-ss");
        string pathToNewFile = Path.Combine(pathToLoggingDic, $"{timestamp}.md");

        FileStream stream = File.Create(pathToNewFile);
        stream.Dispose();

        return pathToNewFile;
    }

    public void LogCallerInfos(LoggerParams loggerParams, CallerInfos callerInfos)
    {
        string msg = $"[Caller]: {callerInfos.CallerName}() in {callerInfos.FilePath} at line {callerInfos.LineNum}";
        Write(loggerParams, LogLevel.Debug, msg);
    }

    /// <summary>
    /// Writes an informational log entry using the specified parameters and message.
    /// </summary>
    /// <param name="loggerParams">The logging parameters that define the context and configuration for the log entry. Cannot be null.</param>
    /// <param name="message">The informational message to log. Cannot be null or empty.</param>
    /// <param name="callerInfos">Optional information about the caller, such as file name, line number, or member name. May be null.</param>
    public void LogInformation(LoggerParams loggerParams, string message, CallerInfos callerInfos = null)
    {
        Write(loggerParams, LogLevel.Information, message, callerInfos);
    }

    /// <summary>
    /// Logs a warning message using the specified logger parameters.
    /// </summary>
    /// <param name="loggerParams">The configuration settings and context to use for logging the warning message. Cannot be null.</param>
    /// <param name="message">The warning message to log. Cannot be null or empty.</param>
    /// <param name="callerInfos">Optional information about the caller, such as file name or line number, to include with the log entry. May be
    /// null.</param>
    public void LogWarning(LoggerParams loggerParams, string message, CallerInfos callerInfos = null)
    {
        Write(loggerParams, LogLevel.Warning, message, callerInfos);
    }

    /// <summary>
    /// Writes a debug-level log entry with the specified message and logging parameters.
    /// </summary>
    /// <param name="loggerParams">The logging parameters that define the context and configuration for the log entry. Cannot be null.</param>
    /// <param name="message">The message to include in the debug log entry. Cannot be null.</param>
    /// <param name="callerInfos">Optional information about the caller, such as file name or line number, to include in the log entry. May be
    /// null.</param>
    public void LogDebug(LoggerParams loggerParams, string message, CallerInfos callerInfos = null)
    {
        Write(loggerParams, LogLevel.Warning, message, callerInfos);
    }

    public void LogHttpPayload(LoggerParams loggerParams, Type dataType, PayloadType payloadType, HttpRequestType httpRequestType)
    {
        //Check mithilfe des datatypes ob die klasse n attribute hat das in der _filter... ist
        //Falls ja remove das ausm json
    }

    /// <summary>
    /// Writes the log to the current file and the standard output.
    /// </summary>
    private void Write(LoggerParams loggerParams, LogLevel logLevel, string msg, CallerInfos? callerInfos = null)
    {
        if (!IsLogLevelEnabled(logLevel)
            || _loggerSettings.DeactivateFileLogging && _loggerSettings.DeactivateConsoleLogging)
        {
            return;
        }

        //Lock to prevent multiple threads from writing to the file at the same time
        lock (_lock)
        {
            StreamWriter? fileWriter = null;
            TextWriter? consoleWriter = null;

            // Only init the StreamWriter if we are going to write to a file
            if (!loggerParams.HasFlag(LoggerParams.NoFile) && !_loggerSettings.DeactivateFileLogging)
            {
                fileWriter = new StreamWriter(_currentFile, append: true)
                {
                    AutoFlush = true
                };
            }

            // Only init the console writer if we are going to write to the console
            if (!loggerParams.HasFlag(LoggerParams.NoConsole) && !_loggerSettings.DeactivateConsoleLogging)
            {
                consoleWriter = Console.Out;
                if (Console.Out == TextWriter.Null)
                {
                    string warning = $"[{DateTime.Now:HH:mm:ss}] [{logLevel}]: " +
                        $"You activated console logging but there is no console to log to!\n " +
                        $"If this wasn´t on purpose try calling AllocConsole() from the kernel32 dll!";

                    fileWriter?.WriteLine($"{warning}\n");
                }
            }

            // Determine the ending character based on the NoNewLine flag
            char startingChar = loggerParams.HasFlag(LoggerParams.NoNewLine)
                ? ' '
                : '\n';

            Console.ForegroundColor = GetDesiredConsoleColor(logLevel);
            string timestamp = $"[{DateTime.Now:HH:mm:ss}]";

            if (callerInfos is not null)
            {
                string callerInfosMsg = $"[Caller]: {callerInfos.CallerName}() in {callerInfos.FilePath} at line {callerInfos.LineNum}";
                callerInfosMsg = $"{startingChar}{timestamp} {callerInfosMsg}";

                fileWriter?.WriteLine(callerInfosMsg);
                consoleWriter?.WriteLine(callerInfosMsg);
            }

            msg = $"{startingChar}{timestamp} [{logLevel}]: {msg}";

            msg = msg.TrimStart(' ');

            fileWriter?.WriteLine(msg);
            consoleWriter?.WriteLine(msg);

            fileWriter?.Dispose();
            Console.ResetColor();
        }
    }

    private bool IsLogLevelEnabled(LogLevel logLevel)
        => logLevel <= _loggerSettings.LogLevel;

    private ConsoleColor GetDesiredConsoleColor(LogLevel logLevel)
    {
        return logLevel switch
        {
            LogLevel.None => ConsoleColor.White,
            LogLevel.Error => _loggerSettings.ErrorColor,
            LogLevel.Warning => _loggerSettings.WarningColor,
            LogLevel.Information => _loggerSettings.InformationColor,
            LogLevel.Debug => _loggerSettings.DebugColor,
            _ => throw new NotImplementedException(),
        };
    }
}
