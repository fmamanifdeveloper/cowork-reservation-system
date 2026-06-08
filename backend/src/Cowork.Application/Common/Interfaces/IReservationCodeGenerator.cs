namespace Cowork.Application.Common.Interfaces;

public interface IReservationCodeGenerator
{
    string Generate(DateTimeOffset createdAt);
}