using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Catalog.Domain.Entities
{
    public class Category
    {
        public int Id { get; private set; }
        public string Name { get; private set; } = string.Empty;
        public string Description { get; private set; } = string.Empty;
        public bool IsActive { get; private set; } = true;

        // Navigation (read-only)
        public IReadOnlyCollection<Product> Products => _products;
        private readonly List<Product> _products = new();

        private Category() { }

        public Category(string name, string description)
        {
            Name = name;
            Description = description;
        }
    }

}




