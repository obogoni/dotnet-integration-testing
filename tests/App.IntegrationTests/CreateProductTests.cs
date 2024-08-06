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

namespace App.IntegrationTests;

public class CreateProductTests : IAsyncLifetime
{
    private string connectionString;
    private readonly MsSqlContainer container = new MsSqlBuilder().Build();

    public async Task InitializeAsync()
    {
        await container.StartAsync();

        connectionString = await SetUpDatabase(container);
    }

    public async Task DisposeAsync()
    {
        await container.DisposeAsync();
    }

    [Fact]
    public async Task Product_is_created_successfully()
    {

        //Arrange 

        var dbContextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                                            .UseSqlServer(connectionString)
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

    private async Task<string> SetUpDatabase(MsSqlContainer container)
    {
        #region SQL

        const string CREATE_DB_SQL = @"

            IF NOT EXISTS (SELECT name FROM master.dbo.sysdatabases WHERE name = N'{0}')
            BEGIN
                CREATE DATABASE {0};
            END
        ";

        #endregion

        var connString = container.GetConnectionString();

        using var connection = new SqlConnection(connString);

        await connection.OpenAsync();

        var statement = string.Format(CREATE_DB_SQL, IntegrationTesting.Database.DbConfiguration.DB_NAME);
        var command = new SqlCommand(statement, connection);

        try
        {
            await command.ExecuteNonQueryAsync();
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);

            //TODO: treat exception

            throw;
        }

        await connection.CloseAsync();

        var targetConnectionString = GetTargetConnectionString(container, IntegrationTesting.Database.DbConfiguration.DB_NAME);

        var upgrader = DeployChanges.To
                        .SqlDatabase(targetConnectionString)
                        .WithScriptsEmbeddedInAssembly(Assembly.GetAssembly(typeof(IntegrationTesting.Database.DbConfiguration)))
                        .LogToTrace()
                        .Build();

        upgrader.PerformUpgrade();

        return targetConnectionString;
    }

    private string GetTargetConnectionString(MsSqlContainer container, string dbName)
    {
        var server = $"{container.Hostname},{container.GetMappedPublicPort(MsSqlBuilder.MsSqlPort)}";

        var dictionary = new Dictionary<string, string>
        {
            { "Server", server },
            { "Database", dbName },
            { "User Id", MsSqlBuilder.DefaultUsername },
            { "Password", MsSqlBuilder.DefaultPassword },
            { "TrustServerCertificate", bool.TrueString }
        };

        return string.Join(";", dictionary.Select((KeyValuePair<string, string> property) => string.Join("=", property.Key, property.Value)));
    }

}