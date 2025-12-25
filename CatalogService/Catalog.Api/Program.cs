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
using MassTransit;
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
builder.Services.AddDbContext<CatalogDbContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("CatalogDb"),
        sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(10),
                errorNumbersToAdd: null);
        });
});

// --------------------
// MassTransit (RabbitMQ)
// --------------------
builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(
            builder.Configuration["RabbitMQ:Host"],
            "/",
            h =>
            {
                h.Username(builder.Configuration["RabbitMQ:Username"]);
                h.Password(builder.Configuration["RabbitMQ:Password"]);
            });

        // IMPORTANT: do NOT block startup
        cfg.ConfigureEndpoints(context);
    });
});

// --------------------
// Application services
// --------------------
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICheckoutService, CheckoutService>();

builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();

builder.Services.AddValidatorsFromAssemblyContaining<CreateCategoryCommandValidator>();

// --------------------
// Messaging abstraction
// --------------------
builder.Services.AddScoped<IEventPublisher, MassTransitEventPublisher>();

// ====================
// ====================
var app = builder.Build();

// --------------------
// Apply EF Core migrations (NO SEEDING)
// --------------------
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
    dbContext.Database.Migrate();
}

// --------------------
// Middleware pipeline
// --------------------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.MapControllers();

app.Run();
