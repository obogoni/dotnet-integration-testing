using Ardalis.Specification.EntityFrameworkCore;
using IntegrationTesting.App.Domain;
using IntegrationTesting.App.Infrastructure.Persistence;

namespace IntegrationTesting.App.Infrastructure;

public class ProductRepository : RepositoryBase<Product>
{
    public ProductRepository(ApplicationDbContext context) : base(context)
    {

    }

}
