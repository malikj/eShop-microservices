using Catalog.Domain.Entities;
using Catalog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Infrastructure.Persistence.Seed;

//public static class CatalogDbSeeder
//{
//    public static async Task SeedAsync(CatalogDbContext context)
//    {
//        // Categories
//        if (!context.Categories.Any())
//        {
//            var electronics = new Category("Electronics", "Electronic items");
//            var books = new Category("Books", "Books & magazines");

//            context.Categories.AddRange(electronics, books);
//            await context.SaveChangesAsync();
//        }

//        // Products
//        if (!context.Products.Any())
//        {
//            var electronics = context.Categories.First(c => c.Name == "Electronics");
//            var books = context.Categories.First(c => c.Name == "Books");

//            var products = new List<Product>
//            {
//                new Product("Laptop", "Gaming Laptop", 1200m, electronics.Id),
//                new Product("Headphones", "Noise Cancelling", 250m, electronics.Id),
//                new Product("Clean Code", "Programming Book", 45m, books.Id)
//            };

//            context.Products.AddRange(products);
//            await context.SaveChangesAsync();
//        }
//    }
//}


public static class CatalogDbSeeder
{
    public static async Task SeedAsync(CatalogDbContext context)
    {
        // Categories
        if (!context.Categories.Any())
        {
            var electronics = new Category("Electronics", "Electronic items");
            var books = new Category("Books", "Books & magazines");

            context.Categories.AddRange(electronics, books);
            await context.SaveChangesAsync();
        }

        // Products
        if (!context.Products.Any())
        {
            var electronics = context.Categories.First(c => c.Name == "Electronics");
            var books = context.Categories.First(c => c.Name == "Books");

            var products = new List<Product>
            {
                new Product("Laptop", "Gaming Laptop", 1200m, electronics.Id),
                new Product("Headphones", "Noise Cancelling", 250m, electronics.Id),
                new Product("Clean Code", "Programming Book", 45m, books.Id)
            };

            context.Products.AddRange(products);
            await context.SaveChangesAsync();
        }
    }
}
