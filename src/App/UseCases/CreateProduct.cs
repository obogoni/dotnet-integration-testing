using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks.Sources;
using Ardalis.GuardClauses;
using Ardalis.Specification;
using CSharpFunctionalExtensions;
using IntegrationTesting.App.Domain;

namespace IntegrationTesting.App.UseCases
{
    public interface ICreateProduct
    {
        Task<Result<Product>> Create(CreateRequest request);
    }

    public record CreateRequest()
    {
        public string Name { get; set; } = null!;

        public decimal Price { get; set; }

        public int CategoryId { get; set; } = default!;
    }

    public sealed class CreateProduct(
        IRepositoryBase<Product> productRepo,
        IRepositoryBase<Category> categoryRepo) : ICreateProduct
    {

        public async Task<Result<Product>> Create(CreateRequest request)
        {
            Guard.Against.Null(request);
            
            var category = await categoryRepo.GetByIdAsync(request.CategoryId);
            if (category == null) return Result.Failure<Product>(Errors.CategoryNotFound);

            var createResult = Product.Create(request.Name, request.Price, category.Id);
            if (createResult.IsFailure) return Result.Failure<Product>(createResult.Error);

            var product = createResult.Value;

            await productRepo.AddAsync(product);

            return Result.Success(product);
        }

        public static class Errors
        {
            public const string CategoryNotFound = "Category was not found!";
        }
    }
}