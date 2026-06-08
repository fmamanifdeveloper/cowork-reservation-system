namespace Cowork.Application.Auth;

public sealed record LoginRequest(
    string Username,
    string Password);