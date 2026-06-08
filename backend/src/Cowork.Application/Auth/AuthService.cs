using Cowork.Application.Common.Exceptions;
using Cowork.Application.Common.Interfaces;

namespace Cowork.Application.Auth;

public sealed class AuthService
{
    private readonly IAppUserRepository _appUserRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IAuditLogger _auditLogger;
    private readonly IUnitOfWork _unitOfWork;

    public AuthService(
        IAppUserRepository appUserRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator,
        IAuditLogger auditLogger,
        IUnitOfWork unitOfWork)
    {
        _appUserRepository = appUserRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
        _auditLogger = auditLogger;
        _unitOfWork = unitOfWork;
    }

    public async Task<LoginResponse> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Username))
            throw new BusinessRuleException("Username is required.");

        if (string.IsNullOrWhiteSpace(request.Password))
            throw new BusinessRuleException("Password is required.");

        var user = await _appUserRepository.GetByUsernameAsync(
            request.Username.Trim(),
            cancellationToken);

        if (user is null || !user.CanLogin())
        {
            await LogFailedLoginAsync(request.Username, cancellationToken);
            throw new BusinessRuleException("Invalid username or password.");
        }

        var isValidPassword = _passwordHasher.Verify(
            request.Password,
            user.PasswordHash);

        if (!isValidPassword)
        {
            await LogFailedLoginAsync(request.Username, cancellationToken);
            throw new BusinessRuleException("Invalid username or password.");
        }

        user.RegisterLogin();

        var accessToken = _jwtTokenGenerator.Generate(user);

        await _auditLogger.LogAsync(
            "UserLoginSucceeded",
            "AppUser",
            user.Id,
            user.Id,
            user.CustomerId,
            "Login",
            "User login succeeded.",
            null,
            new
            {
                user.Id,
                user.Username,
                user.Role,
                user.CustomerId
            },
            null,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new LoginResponse(
            accessToken,
            new AuthenticatedUserDto(
                user.Id,
                user.CustomerId,
                user.Username,
                user.DisplayName,
                user.Role));
    }

    private async Task LogFailedLoginAsync(
        string username,
        CancellationToken cancellationToken)
    {
        await _auditLogger.LogAsync(
            "UserLoginFailed",
            "AppUser",
            null,
            null,
            null,
            "Login",
            "User login failed.",
            null,
            new { Username = username },
            null,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}