using Cowork.Domain.Enums;

namespace Cowork.Domain.Entities;

public sealed class AppUser
{
    private AppUser()
    {
        Username = string.Empty;
        PasswordHash = string.Empty;
        DisplayName = string.Empty;
    }

    public AppUser(
        Guid id,
        Guid? customerId,
        string username,
        string passwordHash,
        string displayName,
        AppUserRole role,
        AppUserStatus status,
        Guid? createdByUserId = null)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("Username is required.", nameof(username));

        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Password hash is required.", nameof(passwordHash));

        if (string.IsNullOrWhiteSpace(displayName))
            throw new ArgumentException("Display name is required.", nameof(displayName));

        if (role == AppUserRole.Customer && customerId is null)
            throw new InvalidOperationException("Customer users must be linked to a customer profile.");

        if (role != AppUserRole.Customer && customerId is not null)
            throw new InvalidOperationException("Only customer users can be linked to a customer profile.");

        Id = id;
        CustomerId = customerId;
        Username = username.Trim();
        PasswordHash = passwordHash;
        DisplayName = displayName.Trim();
        Role = role;
        Status = status;
        IsActive = status == AppUserStatus.Active;
        IsDeleted = false;
        CreatedAt = DateTimeOffset.UtcNow;
        CreatedByUserId = createdByUserId;
        Version = 1;
    }

    public Guid Id { get; private set; }
    public Guid? CustomerId { get; private set; }

    public string Username { get; private set; }
    public string PasswordHash { get; private set; }
    public string DisplayName { get; private set; }

    public AppUserRole Role { get; private set; }
    public AppUserStatus Status { get; private set; }

    public bool IsActive { get; private set; }
    public bool IsDeleted { get; private set; }

    public DateTimeOffset? LastLoginAt { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }
    public DateTimeOffset? DeletedAt { get; private set; }

    public Guid? CreatedByUserId { get; private set; }
    public Guid? UpdatedByUserId { get; private set; }
    public Guid? DeletedByUserId { get; private set; }

    public int Version { get; private set; }

    public void RegisterLogin()
    {
        LastLoginAt = DateTimeOffset.UtcNow;
    }

    public bool CanLogin()
    {
        return IsActive && !IsDeleted && Status == AppUserStatus.Active;
    }
}