using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Catalog.Application.Products;

public sealed record CreateProductCommand(
    string Name,
    string Description,
    decimal Price,
    int CategoryId);

