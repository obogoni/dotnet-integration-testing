using Ardalis.Specification.EntityFrameworkCore;
using IntegrationTesting.App.Domain;
using IntegrationTesting.App.Infrastructure.Persistence;

namespace IntegrationTesting.App.Infrastructure;

public class CategoryRepository : RepositoryBase<Category>
{
    public CategoryRepository(ApplicationDbContext context) : base(context)
    {

    }
}
