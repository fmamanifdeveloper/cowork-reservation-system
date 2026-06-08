using Cowork.Application.Common.Exceptions;
using Cowork.Application.Common.Interfaces;
using Cowork.Domain.Entities;

namespace Cowork.Application.Customers;

public sealed class CustomerService
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IAuditLogger _auditLogger;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public CustomerService(
        ICustomerRepository customerRepository,
        IAuditLogger auditLogger,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _customerRepository = customerRepository;
        _auditLogger = auditLogger;
        _currentUserService = currentUserService;
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
        var currentUserId = _currentUserService.UserId;

        var customer = new Customer(
            Guid.NewGuid(),
            request.FullName,
            request.Email,
            request.Phone,
            request.DocumentNumber,
            currentUserId);

        _customerRepository.Add(customer);

        await _auditLogger.LogAsync(
            "CustomerCreated",
            "Customer",
            customer.Id,
            currentUserId,
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
                customer.DocumentNumber,
                customer.CreatedByUserId
            },
            null,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ToDto(customer);
    }

    public async Task<CustomerDto> UpdateAsync(
        Guid id,
        UpdateCustomerRequest request,
        CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId;

        var customer = await _customerRepository.GetByIdAsync(id, cancellationToken);

        if (customer is null)
            throw new NotFoundException("Customer was not found.");

        var oldValues = new
        {
            customer.FullName,
            customer.Email,
            customer.Phone,
            customer.DocumentNumber,
            customer.UpdatedByUserId
        };

        customer.Update(
            request.FullName,
            request.Email,
            request.Phone,
            request.DocumentNumber,
            currentUserId);

        await _auditLogger.LogAsync(
            "CustomerUpdated",
            "Customer",
            customer.Id,
            currentUserId,
            customer.Id,
            "Update",
            "Customer profile was updated.",
            oldValues,
            new
            {
                customer.FullName,
                customer.Email,
                customer.Phone,
                customer.DocumentNumber,
                customer.UpdatedByUserId
            },
            null,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ToDto(customer);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId;

        var customer = await _customerRepository.GetByIdAsync(id, cancellationToken);

        if (customer is null)
            throw new NotFoundException("Customer was not found.");

        var oldValues = new
        {
            customer.FullName,
            customer.IsDeleted,
            customer.DeletedAt,
            customer.DeletedByUserId
        };

        customer.Delete(currentUserId);

        await _auditLogger.LogAsync(
            "CustomerDeleted",
            "Customer",
            customer.Id,
            currentUserId,
            customer.Id,
            "Delete",
            "Customer profile was logically deleted.",
            oldValues,
            new
            {
                customer.FullName,
                customer.IsDeleted,
                customer.DeletedAt,
                customer.DeletedByUserId
            },
            null,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
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