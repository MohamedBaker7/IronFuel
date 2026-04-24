using System.ComponentModel.DataAnnotations;

namespace IronFuel.Web.Core.ViewModels
{
    public class FlavorFormViewModel
    {
        public int Id { get; set; }

        [MaxLength(200, ErrorMessage = Errors.MaxLength)]
        [Display(Name = "Flavor")]
        [Remote("AllowedItem", "Flavors", AdditionalFields = "Id", ErrorMessage = Errors.Duplicated),
            RegularExpression(RegexPattern.CharactersOnly_Eng, ErrorMessage = Errors.OnlyEnglishLetters)]
        public string Name { get; set; } = null!;
    }
}
