namespace Cowork.Application.Auth;

public sealed record LoginResponse(
    string AccessToken,
    AuthenticatedUserDto User);