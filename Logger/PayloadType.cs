namespace Cristiano3120.Logging;

/// <summary>
/// Specifies the direction of a payload in a communication process.
/// </summary>
/// <remarks>Use this enumeration to indicate whether a payload was received or sent. This can be useful for
/// logging, auditing, or processing messages based on their direction.</remarks>
public enum PayloadType : byte
{
    Received = 0,
    Sent = 1,
}