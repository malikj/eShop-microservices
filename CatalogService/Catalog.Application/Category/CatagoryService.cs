using Catalog.Domain.Entities;
using Catalog.Domain.Repositories;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Catalog.Application.Categories;

public interface ICategoryService
{
    Task<int> CreateAsync(CreateCategoryCommand command);
    Task<IEnumerable<CategoryDto>> GetAllAsync();
}

public sealed class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _repository;
    private readonly IValidator<CreateCategoryCommand> _validator;

    public CategoryService(
        ICategoryRepository repository,
        IValidator<CreateCategoryCommand> validator)
    {
        _repository = repository;
        _validator = validator;
    }

    public async Task<int> CreateAsync(CreateCategoryCommand command)
    {
        var result = await _validator.ValidateAsync(command);
        if (!result.IsValid)
            throw new ValidationException(result.Errors);

        var category = new Category(
            command.Name,
            command.Description);

        await _repository.AddAsync(category);
        return category.Id;
    }

    public async Task<IEnumerable<CategoryDto>> GetAllAsync()
    {
        var categories = await _repository.GetAllAsync();

        return categories.Select(c => new CategoryDto
        {
            Id = c.Id,
            Name = c.Name,
            Description = c.Description
        });
    }
}

