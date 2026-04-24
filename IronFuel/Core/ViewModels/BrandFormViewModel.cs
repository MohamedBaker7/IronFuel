using System.ComponentModel.DataAnnotations;

namespace IronFuel.Web.Core.ViewModels
{
    public class BrandFormViewModel
    {
        public int Id { get; set; }

        [MaxLength(200, ErrorMessage = Errors.MaxLength)]
        [Display(Name = "Brand")]
        [Remote("AllowedItem", "Brands", AdditionalFields = "Id", ErrorMessage = Errors.Duplicated),
            RegularExpression(RegexPattern.CharactersOnly_Eng, ErrorMessage = Errors.OnlyEnglishLetters)]
        public string Name { get; set; } = null!;
    }
}
