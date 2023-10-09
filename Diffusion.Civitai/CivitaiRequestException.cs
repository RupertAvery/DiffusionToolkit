using System.Net;

namespace Diffusion.Civitai;

public class CivitaiRequestException : Exception
{
    public HttpStatusCode StatusCode { get; }
    public string Path { get; }
    public string Body { get; }

    public CivitaiRequestException(string message, HttpStatusCode statusCode) : base(message)
    {
        StatusCode = statusCode;
    }

    public CivitaiRequestException(string message, string body, HttpStatusCode statusCode) : base($"{message}")
    {
        Body = body;
        StatusCode = statusCode;
    }

    public CivitaiRequestException(string message, string path, string body, HttpStatusCode statusCode) : base($"{path}: {message}")
    {
        Path = path;
        Body = body;
        StatusCode = statusCode;
    }

    public CivitaiRequestException(string message, string path, string body, HttpStatusCode statusCode, Exception innerException) : base($"{path}: {message}", innerException)
    {
        Path = path;
        Body = body;
        StatusCode = statusCode;
    }
}