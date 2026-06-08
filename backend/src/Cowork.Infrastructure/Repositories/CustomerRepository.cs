using Cowork.Application.Common.Interfaces;
using Cowork.Domain.Entities;
using Cowork.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Cowork.Infrastructure.Repositories;

public sealed class CustomerRepository : ICustomerRepository
{
    private readonly CoworkDbContext _dbContext;

    public CustomerRepository(CoworkDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<Customer>> ListAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.Customers
            .AsNoTracking()
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.FullName)
            .ToListAsync(cancellationToken);
    }

    public async Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _dbContext.Customers
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);
    }

    public void Add(Customer customer)
    {
        _dbContext.Customers.Add(customer);
    }
}