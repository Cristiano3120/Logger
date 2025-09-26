using System.Data;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Cristiano3120.Logging;

/// <summary>
/// Provides functionality for logging messages, warnings, errors, and diagnostic information to files and the console,
/// with support for configurable log levels, structured payload logging, and attribute-based filtering.
/// </summary>
/// <remarks>The Logger class supports flexible logging scenarios, including writing to both files and the
/// console, and allows customization through LoggerSettings. It enables advanced features such as filtering or
/// transforming logged data based on custom attributes applied to output types. Thread safety is ensured for file and
/// console writes. Use this class to capture application events, errors, and diagnostic details in a structured and
/// configurable manner.</remarks>
public partial class Logger
{
    private readonly Lock _lock = new();
    private readonly LoggerSettings _loggerSettings;
    private readonly string _currentFile;
    private readonly Dictionary<Type, Action<PropertyInfo, JsonNode>> _filterAttributes;

    /// <summary>
    /// Initializes the logger with default <see cref="LoggerSettings"/>
    /// </summary>
    public Logger() : this(new LoggerSettings())
    { }

    /// <summary>
    /// Initializes a new instance of the Logger class using the specified logger settings.
    /// </summary>
    /// <param name="loggerSettings">The configuration settings to use for the logger. Cannot be null.</param>
    public Logger(LoggerSettings loggerSettings)
    {
        _loggerSettings = loggerSettings;
        _currentFile = MaintainLoggingSystem();
        _filterAttributes = [];
        Write(LoggerParams.NoNewLine, LogLevel.Debug, "Logger initialized");
    }

    /// <summary>
    /// Associates an action with a specified attribute type to be used as a filter.
    /// 
    /// <para>
    /// <c>Example:</c> <br></br>
    /// <![CDATA[
    /// logger.AddAttributeToFilter<FilterAtrribute>(static (prop, jsonNode) =>
    /// {
    ///     FilterAtrribute? filterAtrribute = prop.GetCustomAttribute<FilterAtrribute>();
    ///     _ = (jsonNode?[prop.Name] = filterAtrribute?.FilterSymbol);
    /// });
    /// ]]>
    /// </para>
    /// </summary>
    /// <remarks>If an action is already associated with the specified attribute type, it will be replaced by
    /// the new action.</remarks>
    /// <typeparam name="TAttribute">The type of attribute to associate with the filter action. Must derive from <see cref="Attribute"/>.</typeparam>
    /// <param name="action">The action to execute when the specified attribute type is encountered. Cannot be null.</param>
    public void AddAttributeToFilter<TAttribute>(Action<PropertyInfo, JsonNode> action) where TAttribute : Attribute
    {
        _filterAttributes[typeof(TAttribute)] = action;
    }

    /// <summary>
    /// Logs detailed information about the calling method, including its name, source file, and line number, at the
    /// debug log level.
    /// </summary>
    /// <param name="loggerParams">The logging configuration and context to use when writing the log entry.</param>
    /// <param name="callerInfos">The information about the calling method, including its name, file path, and line number. Cannot be null.</param>
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
    public void LogInformation(LoggerParams loggerParams, string message, CallerInfos? callerInfos = null)
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
    public void LogWarning(LoggerParams loggerParams, string message, CallerInfos? callerInfos = null)
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
    public void LogDebug(LoggerParams loggerParams, string message, CallerInfos? callerInfos = null)
    {
        Write(loggerParams, LogLevel.Warning, message, callerInfos);
    }

    /// <summary>
    /// Logs the HTTP payload content at the debug level, formatting it as indented JSON and applying any
    /// attribute-based filtering defined for the output type.
    /// </summary>
    /// <remarks>If the JSON content cannot be parsed, an error is logged and no payload is recorded. If
    /// attribute-based filtering is enabled, properties of the output type decorated with recognized attributes may be
    /// transformed or omitted in the logged output. The method is intended for diagnostic or debugging purposes and
    /// should not be used to log sensitive information unless appropriate filtering is in place.</remarks>
    /// <typeparam name="TOutput">The type representing the expected output. Used to determine which property attributes, if any, should be
    /// applied for filtering or transformation before logging.</typeparam>
    /// <param name="loggerParams">The parameters that control logging behavior, such as context or configuration for the logger.</param>
    /// <param name="payloadType">The type of payload being logged. Used to categorize the log entry.</param>
    /// <param name="httpRequestType">The HTTP request type (such as GET, POST, etc.) associated with the payload. Included in the log message for
    /// context.</param>
    /// <param name="content">The raw JSON content of the HTTP payload to be logged. Must be a valid JSON string.</param>
    public void LogHttpPayload<TOutput>(
        LoggerParams loggerParams, 
        PayloadType payloadType,
        HttpRequestType httpRequestType, 
        string content)
    {
        LogHttpPayloadLogic(typeof(TOutput), loggerParams, payloadType, httpRequestType, content);
    }

    /// <summary>
    /// Logs the HTTP payload content at the debug level, formatting it as indented JSON and applying any
    /// attribute-based filtering defined for the output type.
    /// </summary>
    /// <remarks>If the JSON content cannot be parsed, an error is logged and no payload is recorded. If
    /// attribute-based filtering is enabled, properties of the output type decorated with recognized attributes may be
    /// transformed or omitted in the logged output. The method is intended for diagnostic or debugging purposes and
    /// should not be used to log sensitive information unless appropriate filtering is in place.</remarks>
    /// <param name="type" >The type representing the expected output. Used to determine which property attributes, if any, should be
    /// applied for filtering or transformation before logging</param>
    /// <param name="loggerParams">The parameters that control logging behavior, such as context or configuration for the logger.</param>
    /// <param name="payloadType">The type of payload being logged. Used to categorize the log entry.</param>
    /// <param name="httpRequestType">The HTTP request type (such as GET, POST, etc.) associated with the payload. Included in the log message for
    /// context.</param>
    /// <param name="content">The raw JSON content of the HTTP payload to be logged. Must be a valid JSON string.</param>
    public void LogHttpPayload(
        Type type,
        LoggerParams loggerParams,
        PayloadType payloadType,
        HttpRequestType httpRequestType,
        string content)
    { 
        LogHttpPayloadLogic(type, loggerParams, payloadType, httpRequestType, content);
    }

    private void LogHttpPayloadLogic(Type dataType, LoggerParams loggerParams, PayloadType payloadType,
        HttpRequestType httpRequestType, string content)
    {
        JsonNode? jsonNode = JsonNode.Parse(content);
        if (jsonNode is null)
        {
            LogError(loggerParams, "Couldn´t parse the json to a JsonNode", CallerInfos.Create());
            return;
        }

        if (!_loggerSettings.DeactivateReflection && dataType.CustomAttributes.Any())
        {
            foreach (PropertyInfo prop in dataType.GetProperties())
            {
                foreach (Attribute attribute in prop.GetCustomAttributes())
                {
                    Type attributeType = attribute.GetType();
                    if (_filterAttributes.TryGetValue(attributeType, out Action<PropertyInfo, JsonNode>? action))
                    {
                        action(prop, jsonNode);
                    }
                }
            }
        }

        string formatedJson = jsonNode.ToJsonString(new JsonSerializerOptions() { WriteIndented = true });
        string msg = $"[{payloadType}]({httpRequestType}): {formatedJson}";

        //If a "{" exists put it into its own line
        int bracketIndex;
        if ((bracketIndex = msg.IndexOf('{')) != -1)
        {
            msg = msg.Insert(bracketIndex, "\n");
        }

        Write(loggerParams, LogLevel.Debug, msg);
    }

    /// <summary>
    /// Logs an error message using the specified logging parameters.
    /// </summary>
    /// <param name="loggerParams">The parameters that configure the logging behavior, such as log destination and formatting options. Cannot be
    /// null.</param>
    /// <param name="exMsg">The error message to log. Cannot be null or empty.</param>
    /// <param name="callerInfos">Optional information about the caller, such as file name, line number, or member name. May be null.</param>
    public void LogError(LoggerParams loggerParams, string exMsg, CallerInfos? callerInfos = null)
    {
        Write(loggerParams, LogLevel.Error, exMsg, callerInfos);
    }

    /// <summary>
    /// Logs detailed information about an exception, including its message and stack trace, at the error level.
    /// </summary>
    /// <remarks>This method writes both the exception message and the full stack trace as separate
    /// error-level log entries. Use this method to capture comprehensive error details for troubleshooting.</remarks>
    /// <param name="loggerParams">The parameters that configure the logging behavior, such as log destination or formatting options.</param>
    /// <param name="ex">The exception to log. Cannot be null.</param>
    /// <param name="callerInfos">Optional information about the caller, such as the member name, file path, or line number. Can be null.</param>
    public void LogError(LoggerParams loggerParams, Exception ex, CallerInfos? callerInfos = null)
    {
        Write(loggerParams, LogLevel.Error, $"EXCEPTION MSG: {ex.Message}", callerInfos);
        Write(loggerParams, LogLevel.Error, $"STACKTRACE: {ex}", callerInfos);
    }

    /// <summary>
    /// Logs detailed information about an exception, including its message and stack trace, at the error level.
    /// </summary>
    /// <param name="loggerParams"></param>
    /// <param name="msg"></param>
    /// <param name="callerInfos">The method that called the calle</param>
    /// <param name="calleInfos">The method that called this method</param>
    public void LogError(LoggerParams loggerParams, string msg, CallerInfos callerInfos, CallerInfos calleInfos)
    {
        LogCallerInfos(LoggerParams.NoNewLine, callerInfos);
        Write(loggerParams, LogLevel.Error, $"[Calle(threw the ex)]: {calleInfos.CallerName}() in {calleInfos.FilePath} at line {calleInfos.LineNum}");
        Write(loggerParams, LogLevel.Error, $"EXCEPTION MSG: {msg}");
    }

    /// <summary>
    /// Logs detailed information about an exception, including its message and stack trace, at the error level.
    /// </summary>
    /// <param name="loggerParams"></param>
    /// <param name="ex">The exception that was thrown</param>
    /// <param name="callerInfos">The method that called the calle</param>
    /// <param name="calleInfos">The method that called this method</param>
    public void LogError(LoggerParams loggerParams, Exception ex, CallerInfos callerInfos, CallerInfos calleInfos)
    {
        LogCallerInfos(LoggerParams.NoNewLine, callerInfos);
        Write(loggerParams, LogLevel.Error, $"[Calle(threw the ex)]: {calleInfos.CallerName}() in {calleInfos.FilePath} at line {calleInfos.LineNum}");
        Write(loggerParams, LogLevel.Error, $"EXCEPTION MSG: {ex.Message}");
        Write(loggerParams, LogLevel.Error, $"{ex}");
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
}
