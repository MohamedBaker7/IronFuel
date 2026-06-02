using System.ComponentModel.DataAnnotations;

namespace IronFuel.Web.Core.ViewModels
{
    public class FlavorViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Code { get; set; } = null!;
        public bool IsDeleted { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? LastUpdatedOn { get; set; }
    }
}
