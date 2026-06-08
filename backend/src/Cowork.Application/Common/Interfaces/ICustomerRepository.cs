using Cowork.Domain.Entities;

namespace Cowork.Application.Common.Interfaces;

public interface ICustomerRepository
{
    Task<IReadOnlyList<Customer>> ListAsync(CancellationToken cancellationToken);
    Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    void Add(Customer customer);
}