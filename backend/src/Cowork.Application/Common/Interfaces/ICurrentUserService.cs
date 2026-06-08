using Cowork.Domain.Enums;

namespace Cowork.Application.Common.Interfaces;

public interface ICurrentUserService
{
    Guid? UserId { get; }
    Guid? CustomerId { get; }
    AppUserRole? Role { get; }
    string? Username { get; }
    bool IsAuthenticated { get; }
}