using System.ComponentModel.DataAnnotations;

namespace LibraryAPI.Validation
{
    public class FirstLetterCapitalAttribute: ValidationAttribute
    {
        public const string DefaultErrorMessage = "The first letter must be capitalized.";

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is string strValue && !string.IsNullOrEmpty(strValue))
            {
                if (char.IsUpper(strValue[0]))
                    return ValidationResult.Success;
                else
                    return new ValidationResult(DefaultErrorMessage);
            }
            return ValidationResult.Success;
        }
    }
}
