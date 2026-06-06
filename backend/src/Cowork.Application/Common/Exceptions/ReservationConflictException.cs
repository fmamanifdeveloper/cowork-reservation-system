namespace Cowork.Application.Common.Exceptions;

public class ReservationConflictException : Exception
{
    public ReservationConflictException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }
}