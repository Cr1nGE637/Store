namespace Store.Identity.Application.Interfaces;

public sealed class IdentityUniqueConstraintViolationException(Exception innerException)
    : Exception("Identity unique constraint violation", innerException);
