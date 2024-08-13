using FluentAssertions;
using IntegrationTesting.App.Domain;
using IntegrationTesting.App.Infrastructure;
using IntegrationTesting.App.Infrastructure.Persistence;
using IntegrationTesting.App.IntegrationTests;
using IntegrationTesting.App.UseCases;
using IntegrationTesting.Setup;
using Microsoft.EntityFrameworkCore;

namespace App.IntegrationTests;

public class CreateProductTests : IClassFixture<DatabaseFixture>
{
	public DatabaseFixtureBase Fixture { get; private set; }

	public CreateProductTests(DatabaseFixture fixture)
	{
		Fixture = fixture;
	}

	[Fact]
	public async Task Product_is_created_successfully()
	{
		//Arrange

		var dbContextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
											.UseSqlServer(Fixture.ConnectionString)
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

	[Fact]
	public async Task Product_cant_be_created_with_invalid_category()
	{
		//Arrange

		var dbContextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
											.UseSqlServer(Fixture.ConnectionString)
											.Options;

		using var dbContext = new ApplicationDbContext(dbContextOptions);

		var productRepo = new ProductRepository(dbContext);
		var categoryRepo = new CategoryRepository(dbContext);

		var useCase = new CreateProduct(productRepo, categoryRepo);

		var invalidCategoryId = -1;

		//Act

		var result = await useCase.Create(new CreateRequest { Name = "Coca-Cola", Price = 10.0M, CategoryId = invalidCategoryId });

		//Assert

		result.IsFailure.Should().BeTrue();
		result.Error.Should().Be(CreateProduct.Errors.CategoryNotFound);
	}
}