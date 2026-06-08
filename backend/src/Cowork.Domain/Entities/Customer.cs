namespace Cowork.Domain.Entities;

public sealed class Customer
{
    private Customer()
    {
        FullName = string.Empty;
    }

    public Customer(
        Guid id,
        string fullName,
        string? email,
        string? phone,
        string? documentNumber,
        Guid? createdByUserId = null)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            throw new ArgumentException("Customer full name is required.", nameof(fullName));

        Id = id;
        FullName = fullName.Trim();
        Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim();
        Phone = string.IsNullOrWhiteSpace(phone) ? null : phone.Trim();
        DocumentNumber = string.IsNullOrWhiteSpace(documentNumber) ? null : documentNumber.Trim();

        IsDeleted = false;
        CreatedAt = DateTimeOffset.UtcNow;
        CreatedByUserId = createdByUserId;
        Version = 1;
    }

    public Guid Id { get; private set; }

    public string FullName { get; private set; }
    public string? Email { get; private set; }
    public string? Phone { get; private set; }
    public string? DocumentNumber { get; private set; }

    public bool IsDeleted { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }
    public DateTimeOffset? DeletedAt { get; private set; }

    public Guid? CreatedByUserId { get; private set; }
    public Guid? UpdatedByUserId { get; private set; }
    public Guid? DeletedByUserId { get; private set; }

    public int Version { get; private set; }
}