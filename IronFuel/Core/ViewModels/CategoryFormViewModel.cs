using System.ComponentModel.DataAnnotations;

namespace IronFuel.Web.Core.ViewModels
{
    public class CategoryFormViewModel
    {
        public int Id { get; set; }

        [MaxLength(200, ErrorMessage = Errors.MaxLength)]
        [Display(Name = "Category")]
        [Remote("AllowedItem", "Categories", AdditionalFields = nameof(Id), ErrorMessage = Errors.Duplicated),
            RegularExpression(RegexPattern.CharactersOnly_Eng, ErrorMessage = Errors.OnlyEnglishLetters)]
        public string Name { get; set; } = null!;
    }
}
