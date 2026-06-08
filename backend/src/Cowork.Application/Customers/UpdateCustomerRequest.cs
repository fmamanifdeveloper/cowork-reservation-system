namespace Cowork.Application.Customers;

public sealed record UpdateCustomerRequest(
    string FullName,
    string? Email,
    string? Phone,
    string? DocumentNumber);