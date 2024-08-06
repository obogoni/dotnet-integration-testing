using DotNet.Testcontainers.Containers;
using IntegrationTesting.App.Infrastructure;
using IntegrationTesting.App.Infrastructure.Persistence;
using Testcontainers.MsSql;
using Xunit.Sdk;
using CSharpFunctionalExtensions;
using FluentAssertions;
using System.Diagnostics;
using DbUp;
using System.Reflection;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using IntegrationTesting.App.Domain;
using IntegrationTesting.App.UseCases;
using IntegrationTesting.Setup;

namespace App.IntegrationTests;

public class CreateProductTests : IntegrationTestsBase
{
    public CreateProductTests() 
        : base(new DatabaseConfiguration()
        {
            DbName = "IntegrationTesting",
            UserPassword = "myStr0ngPassword!",
            MigrationsAssembly = Assembly.GetAssembly(typeof(IntegrationTesting.Database.DbConfiguration))
        })
    { 

    }

    [Fact]
    public async Task Product_is_created_successfully()
    {

        //Arrange 

        var dbContextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                                            .UseSqlServer(ConnectionString)
                                            .Options;

        using var dbContext = new ApplicationDbContext(dbContextOptions);

        var createCategoryResult = Category.Create("Drinks");

        var categoryRepo = new CategoryRepository(dbContext);
        var productRepo = new ProductRepository(dbContext);

        var category = createCategoryResult.Value;

        await categoryRepo.AddAsync(category);

        var useCase = new CreateProduct(productRepo, categoryRepo);

        //Act
        
        var createResult = await useCase.Create(new CreateRequest { Name = "Coca-Cola", Price = 10.0M, CategoryId = category.Id });

        //Assert

        createResult.IsSuccess.Should().BeTrue();

        var product = createResult.Value;
        var sameProduct = await productRepo.GetByIdAsync(product.Id);

        sameProduct.Should().NotBeNull();
        sameProduct.Name.Should().Be("Coca-Cola");
        sameProduct.Price.Should().Be(10.0M);
        sameProduct.CategoryId.Should().Be(category.Id);
    }

}