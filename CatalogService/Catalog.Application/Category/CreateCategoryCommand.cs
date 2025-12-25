using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Catalog.Application.Categories;

public sealed record CreateCategoryCommand(
    string Name,
    string Description);
