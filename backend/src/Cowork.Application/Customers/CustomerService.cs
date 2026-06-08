using Cowork.Application.Common.Exceptions;
using Cowork.Application.Common.Interfaces;
using Cowork.Domain.Entities;

namespace Cowork.Application.Customers;

public sealed class CustomerService
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IAuditLogger _auditLogger;
    private readonly IUnitOfWork _unitOfWork;

    public CustomerService(
        ICustomerRepository customerRepository,
        IAuditLogger auditLogger,
        IUnitOfWork unitOfWork)
    {
        _customerRepository = customerRepository;
        _auditLogger = auditLogger;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<CustomerDto>> ListAsync(CancellationToken cancellationToken)
    {
        var customers = await _customerRepository.ListAsync(cancellationToken);
        return customers.Select(ToDto).ToList();
    }

    public async Task<CustomerDto> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var customer = await _customerRepository.GetByIdAsync(id, cancellationToken);

        if (customer is null)
            throw new NotFoundException("Customer was not found.");

        return ToDto(customer);
    }

    public async Task<CustomerDto> CreateAsync(
        CreateCustomerRequest request,
        CancellationToken cancellationToken)
    {
        var customer = new Customer(
            Guid.NewGuid(),
            request.FullName,
            request.Email,
            request.Phone,
            request.DocumentNumber);

        _customerRepository.Add(customer);

        await _auditLogger.LogAsync(
            "CustomerCreated",
            "Customer",
            customer.Id,
            null,
            customer.Id,
            "Create",
            "Customer profile was created.",
            null,
            new
            {
                customer.Id,
                customer.FullName,
                customer.Email,
                customer.Phone,
                customer.DocumentNumber
            },
            null,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ToDto(customer);
    }

    private static CustomerDto ToDto(Customer customer)
    {
        return new CustomerDto(
            customer.Id,
            customer.FullName,
            customer.Email,
            customer.Phone,
            customer.DocumentNumber);
    }
}