using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using UoN.ExpressiveAnnotations.NetCore.Attributes;

namespace IronFuel.Web.Core.ViewModels
{
    public class UserFormViewModel
    {
        public string? Id { get; set; }

        [MaxLength(100, ErrorMessage = Errors.MaxLength)]
        [Display(Name = "Full Name"),
            RegularExpression(RegexPattern.CharactersOnly_Eng, ErrorMessage = Errors.OnlyEnglishLetters)]
        public string FullName { get; set; } = null!;

        [MaxLength(20, ErrorMessage = Errors.MaxLength),
            Display(Name = "Username"),
            Remote("AllowedUsername", "Users", AdditionalFields = "Id", ErrorMessage = Errors.Duplicated),
            RegularExpression(RegexPattern.Username, ErrorMessage = Errors.InvalidUsername)]
        public string UserName { get; set; } = null!;

        [MaxLength(200, ErrorMessage = Errors.MaxLength),
            EmailAddress,
            Remote("AllowedEmail", "Users", AdditionalFields = "Id", ErrorMessage = Errors.Duplicated)]
        public string Email { get; set; } = null!;

        [DataType(DataType.Password),
            StringLength(100, ErrorMessage = Errors.MaxMinLength, MinimumLength = 8),
            RegularExpression(RegexPattern.Password, ErrorMessage = Errors.WeekPassword),
            RequiredIf("Id == null", ErrorMessage = Errors.RequiredField)]
        public string? Password { get; set; }

        [DataType(DataType.Password),
            Display(Name = "Confirm password"),
            Compare("Password", ErrorMessage = Errors.PasswordNotMatch),
            RequiredIf("Id == null", ErrorMessage = Errors.RequiredField)]
        public string? ConfirmPassword { get; set; }

        [Display(Name = "Roles")]
        public IList<string> SelectedRoles { get; set; } = new List<string>();

        public IEnumerable<SelectListItem>? Roles { get; set; }



    }
}
