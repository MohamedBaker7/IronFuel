using System;
using System.Collections.Generic;
using System.Text;

namespace IronFuel.Domain.Consts
{
    public static class Errors
    {

        public const string RequiredField = "RequiredField";
        public const string MaxLength = "Length cannot be more than {1} characters";
        public const string MaxMinLength = "The {0} must be at least {2} and at max {1} characters long.";
        public const string Duplicated = "Another record with the same {0} is already exists!";
        public const string AllowedExtension = "Supported image formats: .jpg, .jpeg, .png.";
        public const string MaximumSize = "Each image must be 5 MB or smaller.";
        public const string ValidateDateInFuture = "Date cannot be in the future";
        public const string ValidateRange = "{0} should be between {1} to {2}";
        public const string PasswordNotMatch = "The password and confirmation password do not match.";
        public const string WeekPassword = "Password must be at least 8 characters and contain uppercase, lowercase, number, and special character.";
        public const string InvalidUsername = "Username can only contain letters or digits.";
        public const string InvalidPhoneNumber = "Invalid phone number.";
        public const string OnlyEnglishLetters = "Only English letters are allowed.";
        public const string OnlyArabicLetters = "Only Arabic letters are allowed.";
        public const string OnlyNumbersAndLetters = "Only Arabic/English letters or digits are allowed.";
        public const string DenySpecialCharacters = "Special characters are not allowed.";
        public const string InvalidNationalId = "Invalid National ID.";
        public const string EmptyGalleryImages = "At least one gallery image is required.";
        public const string EmptyImage = "Image file cannot be empty.";
        public const string InvalidStartDate = "Invalid Start Date.";
        public const string InvalidEndDate = "Invalid End Date.";
        public const string DuplicatedProducts = "This product already exists for this brand. Please choose a different name.";
        public const string DuplicatedFlavourAndSize = "Each flavour/size combination must be unique.";
        public const string InvalidFlavour = "One or more variants use an invalid flavour.";
        public const string InvalidBrand = "Select a valid brand.";
        public const string InvalidCategory = "Select a valid category.";


    }
}
