using IronFuel.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace IronFuel.Web.Core.DTO.Products
{
    public record FilterResultDto
    {
        public IEnumerable<ProductViewModel> Products { get; set; } = null!;
        public Dictionary<string, int>? AvailableFlavors { get; set; }
        public Dictionary<string, int>? AvailableSizes { get; set; }
        public int TotalCount { get; set; }
    }
}
