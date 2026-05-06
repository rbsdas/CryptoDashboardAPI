namespace CryptoDashboardAPI.Exceptions;

public class CooldownException : Exception
{
    public int RetryAfterSeconds { get; }

    public CooldownException(string message, int retryAfterSeconds) : base(message)
    {
        RetryAfterSeconds = retryAfterSeconds;
    }
}
