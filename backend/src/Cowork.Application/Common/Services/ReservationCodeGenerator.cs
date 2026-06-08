using Cowork.Application.Common.Interfaces;
using System.Security.Cryptography;

namespace Cowork.Application.Common.Services;

public sealed class ReservationCodeGenerator : IReservationCodeGenerator
{
    public string Generate(DateTimeOffset createdAt)
    {
        var suffix = RandomNumberGenerator.GetInt32(1000, 9999);
        return $"RSV-{createdAt:yyyyMMddHHmmssfff}-{suffix}";
    }
}