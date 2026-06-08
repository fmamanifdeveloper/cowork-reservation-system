namespace Cowork.Application.Customers;

public sealed record CreateCustomerRequest(
    string FullName,
    string? Email,
    string? Phone,
    string? DocumentNumber);