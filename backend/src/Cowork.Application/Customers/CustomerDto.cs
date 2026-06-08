namespace Cowork.Application.Customers;

public sealed record CustomerDto(
    Guid Id,
    string FullName,
    string? Email,
    string? Phone,
    string? DocumentNumber);