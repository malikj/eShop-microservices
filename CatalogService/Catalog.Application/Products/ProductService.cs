using Catalog.Domain.Entities;
using Catalog.Domain.Repositories;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Catalog.Application.Products;

public interface IProductService
{
    Task<Guid> CreateAsync(CreateProductCommand command);
    Task<IEnumerable<ProductDto>> GetAllAsync();
}

public sealed class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;

    private readonly IValidator<CreateProductCommand> _validator;

    public ProductService(
        IProductRepository productRepository,
        ICategoryRepository categoryRepository,
        IValidator<CreateProductCommand> validator)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _validator = validator;
    }

    public async Task<Guid> CreateAsync(CreateProductCommand command)
    {
        // Ensure category exists (important business rule)
        var category = await _categoryRepository.GetByIdAsync(command.CategoryId);
        if (category is null)
            throw new InvalidOperationException("Category does not exist");

        var product = new Product(
            command.Name,
            command.Description,
            command.Price,
            command.CategoryId);

        await _productRepository.AddAsync(product);
        return product.Id;
    }

    public async Task<IEnumerable<ProductDto>> GetAllAsync()
    {
        var products = await _productRepository.GetAllAsync();

        return products.Select(p => new ProductDto
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            Price = p.Price,
            CategoryId = p.CategoryId
        });
    }
}
