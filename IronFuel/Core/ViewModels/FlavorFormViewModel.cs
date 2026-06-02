using System.ComponentModel.DataAnnotations;

namespace IronFuel.Web.Core.ViewModels
{
    public class FlavorFormViewModel
    {
        public int Id { get; set; }

        [MaxLength(200, ErrorMessage = Errors.MaxLength)]
        [Display(Name = "Flavor")]
        [Remote("AllowedName", "Flavors", AdditionalFields = nameof(Id), ErrorMessage = Errors.Duplicated),
            RegularExpression(RegexPattern.CharactersOnly_Eng, ErrorMessage = Errors.OnlyEnglishLetters)]
        public string Name { get; set; } = null!;

        [Required, Display(Name = "Code"), MaxLength(6, ErrorMessage = Errors.MaxLength)]
        [RegularExpression(RegexPattern.CharactersOnly_Eng, ErrorMessage = Errors.OnlyEnglishLetters)]
        [Remote("AllowedCode", "Flavors", AdditionalFields = nameof(Id))]
        public string Code { get; set; } = null!;

        public bool IsCodeLocked { get; set; }
    }
}
