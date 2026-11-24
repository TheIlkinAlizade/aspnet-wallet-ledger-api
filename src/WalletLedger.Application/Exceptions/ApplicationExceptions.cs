namespace WalletLedger.Application.Exceptions;

public sealed class AuthenticationFailedException() : Exception("Authentication failed.");

public sealed class ConflictException(string message) : Exception(message);

public sealed class RequestValidationException(IReadOnlyList<string> errors) : Exception("Validation failed.")
{
    public IReadOnlyList<string> Errors { get; } = errors;
}