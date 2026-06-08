using Cowork.Domain.Enums;

namespace Cowork.Application.Auth;

public sealed record AuthenticatedUserDto(
    Guid Id,
    Guid? CustomerId,
    string Username,
    string DisplayName,
    AppUserRole Role);