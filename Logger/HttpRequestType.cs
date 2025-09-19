namespace Cristiano3120.Logging;

/// <summary>
/// Specifies the HTTP request method to be used when making a web request.
/// </summary>
/// <remarks>Use this enumeration to indicate the desired HTTP method, such as GET, POST, PUT, PATCH, or DELETE,
/// when constructing or processing HTTP requests. The values correspond to standard HTTP methods as defined by the HTTP
/// protocol.</remarks>
public enum HttpRequestType : byte
{
    Get,
    Delete,
    Post,
    Patch,
    Put
}