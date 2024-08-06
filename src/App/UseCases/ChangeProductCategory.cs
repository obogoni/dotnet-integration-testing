using Ardalis.GuardClauses;
using Ardalis.Specification;
using CSharpFunctionalExtensions;
using IntegrationTesting.App.Domain;

namespace IntegrationTesting.App.UseCases
{
    public interface IChangeProductCategory
    {
        Task<Result> ChangeCategory(ChangeCategoryRequest request);
    }

    public class ChangeCategoryRequest
    {
        public int ProductId { get; set; }

        public int NewCategoryId { get; set; }
    }

    public sealed class ChangeProductCategory(
        IRepositoryBase<Product> productRepo,
        IRepositoryBase<Category> categoryRepo
    ) : IChangeProductCategory
    {
        public async Task<Result> ChangeCategory(ChangeCategoryRequest request)
        {
            Guard.Against.Null(request);

             var product = await productRepo.GetByIdAsync(request.ProductId);
             if (product == null) return Result.Failure(Errors.ProductNotFound);

             var category = await categoryRepo.GetByIdAsync(request.NewCategoryId);
             if (category == null) return Result.Failure(Errors.CategoryNotFound);

             product.CategoryId = category.Id;

             await productRepo.UpdateAsync(product);

             return Result.Success();
        }

        public static class Errors
        {
            public const string CategoryNotFound = "Category not found!";
            public const string ProductNotFound = "Product not found!";
        }
    }
}