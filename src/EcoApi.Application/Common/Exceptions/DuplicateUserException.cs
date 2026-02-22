namespace EcoApi.Application.Common.Exceptions;

public sealed class DuplicateUserException : Exception
{
    public DuplicateUserException(string email)
        : base($"User with email '{email}' already exists.")
    {
    }
}
