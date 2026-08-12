namespace SlateDesk.Application.Common.Exceptions;

public sealed class ResourceNotFoundException
    : Exception
{
    public ResourceNotFoundException(string message)
        : base(message)
    {
    }
}

public sealed class ConflictException

: Exception

{
    public ConflictException(string message)
        : base(message)
    {
    }
}

public sealed class BusinessRuleException

: Exception

{
    public BusinessRuleException(string message)
        : base(message)
    {
    }
}