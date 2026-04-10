namespace MusicApp.DTOs;

/// <summary>
/// Non-generic wrapper for void operations.
/// </summary>
public class ServiceResult
{
    public bool IsSuccess { get; }
    public string? Error { get; }
    public Dictionary<string, string[]>? FieldErrors { get; }

    private ServiceResult(bool isSuccess, string? error, Dictionary<string, string[]>? fieldErrors)
    {
        IsSuccess = isSuccess;
        Error = error;
        FieldErrors = fieldErrors;
    }

    public static ServiceResult Ok() => new(true, null, null);
    public static ServiceResult Fail(string error, Dictionary<string, string[]>? fieldErrors = null)
        => new(false, error, fieldErrors);
}

/// <summary>
/// Generic wrapper replacing ApiResponse (no HTTP needed in Blazor Server).
/// </summary>
public class ServiceResult<T>
{
    public bool IsSuccess { get; }
    public T? Data { get; }
    public string? Error { get; }
    public Dictionary<string, string[]>? FieldErrors { get; }

    private ServiceResult(bool isSuccess, T? data, string? error, Dictionary<string, string[]>? fieldErrors)
    {
        IsSuccess = isSuccess;
        Data = data;
        Error = error;
        FieldErrors = fieldErrors;
    }

    public static ServiceResult<T> Ok(T data)
        => new(true, data, null, null);

    public static ServiceResult<T> Fail(string error, Dictionary<string, string[]>? fieldErrors = null)
        => new(false, default, error, fieldErrors);
}
