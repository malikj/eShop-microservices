using Catalog.Api.Middleware;
using Catalog.Application.Categories;
using Catalog.Application.Products;
using Catalog.Application.Checkout;
using Catalog.Application.Abstractions.Messaging;
using Catalog.Domain.Repositories;
using Catalog.Infrastructure.Persistence;
using Catalog.Infrastructure.Repositories;
using Catalog.Infrastructure.Messaging;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// --------------------
// Framework services
// --------------------
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// --------------------
// Database
// --------------------
//builder.Services.AddDbContext<CatalogDbContext>(options =>
//{
//    options.UseSqlServer(
//        builder.Configuration.GetConnectionString("CatalogDb"),
//        sqlOptions =>
//        {
//            sqlOptions.EnableRetryOnFailure(
//                maxRetryCount: 5,
//                maxRetryDelay: TimeSpan.FromSeconds(10),
//                errorNumbersToAdd: null);
//        });
//});

builder.Services.AddDbContext<CatalogDbContext>(options =>
{
    options.UseInMemoryDatabase("CatalogDb");
});

// --------------------
// Application services
// --------------------
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICheckoutService, CheckoutService>();

builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();

// --------------------
// Messaging abstraction
// --------------------
builder.Services.Configure<ServiceBusSettings>(
    builder.Configuration.GetSection(ServiceBusSettings.SectionName));

builder.Services.AddSingleton<IEventPublisher, AzureServiceBusPublisher>();

// --------------------
// Validation
// --------------------
builder.Services.AddValidatorsFromAssemblyContaining<CreateProductCommandValidator>();

// ====================
// Build app
// ====================
var app = builder.Build();

// --------------------
// Middleware pipeline - IMPORTANT ORDER!
// --------------------
// ✅ Swagger MUST be first (before exception handling)
app.UseSwagger();
app.UseSwaggerUI();

//app.UseHttpsRedirection();
app.UseAuthorization();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.MapControllers();

app.Run();
