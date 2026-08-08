namespace SlateDesk.Application.Common.Exceptions;

public class AuthenticationFailedException : Exception
{
    public AuthenticationFailedException(
        string message = "The supplied authentication information is invalid.")
        : base(message)
    {
    }
}

public sealed class TokenReplayDetectedException
    : AuthenticationFailedException
{
    public TokenReplayDetectedException()
        : base(
            "This session is no longer valid. Please sign in again.")
    {
    }
}