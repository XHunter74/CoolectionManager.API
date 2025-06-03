using System.ComponentModel.DataAnnotations;

namespace xhunter74.CollectionManager.Shared.Validation;

public static class ModelValidator
{
    public static (bool isValid, IEnumerable<ValidationResult> validationResults) ValidateModel(object model)
    {
        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(model, serviceProvider: null, items: null);

        bool isValid = Validator.TryValidateObject(
            model,
            validationContext,
            validationResults,
            validateAllProperties: true
        );

        return (isValid, validationResults);
    }
}
