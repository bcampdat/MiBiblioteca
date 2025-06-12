using System.ComponentModel.DataAnnotations;

namespace MiBiblioteca.Validators
{
    public class PaginasValidacion : ValidationAttribute
    { 
        // value representa las páginas
        protected override ValidationResult IsValid(Object value, ValidationContext validationContext)
        {
            if ((int)value < 0)
            {
                return new ValidationResult("Las páginas no pueden ser negativas");
            }

            return ValidationResult.Success;
        }
    }

}
