using CSharpFunctionalExtensions;

namespace IntegrationTesting.App.Domain
{
    public class Category : Entity<int>
    {
        public string Name { get; set; }

        public Category()
        {
            Name = null!;
        }

        public static Result<Category> Create(string name)
        {
            if (string.IsNullOrEmpty(name)) return Result.Failure<Category>(Errors.RequiredName);

            var category = new Category()
            {
                Name = name
            };

            return category;
        }

        public static class Errors
        {
            public const string RequiredName = "Category name is required!";
        }

    }

}

