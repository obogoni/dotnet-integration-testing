using System.Runtime.Versioning;
using Ardalis.GuardClauses;
using CSharpFunctionalExtensions;

namespace IntegrationTesting.App.Domain
{
    public class Product : Entity<int>
    {
        public static class Errors
        {
            public const string RequiredName = "Product name is required!";
            public const string RequiredCategory = "Product category is required!";
        }

        private Product()
        {
            Name = null!;
        }

        public string Name { get; set; }

        public decimal Price { get; set; }

        public int CategoryId { get; set; }

        public static Result<Product> Create(string name, decimal price, int category)
        {
            if (string.IsNullOrEmpty(name)) return Result.Failure<Product>(Errors.RequiredName);
            if (category <= 0) return Result.Failure<Product>(Errors.RequiredCategory);

            var product = new Product()
            {
                Name = name,
                Price = price,
                CategoryId = category
            };

            return Result.Success(product);
        }
    }
}
