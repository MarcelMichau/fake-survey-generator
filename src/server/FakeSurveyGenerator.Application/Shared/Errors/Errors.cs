using FluentValidation.Results;

namespace FakeSurveyGenerator.Application.Shared.Errors;

public static class Errors
{
    public static class General
    {
        public static Error ValidationError(ValidationResult validationResult)
        {
            var errors = validationResult.Errors
                .GroupBy(e => e.PropertyName, e => e.ErrorMessage)
                .ToDictionary(failureGroup => failureGroup.Key, failureGroup => failureGroup.ToArray());

            return new ValidationError(errors);
        }

        public static Error NotFound(string entityName = "Record", long? id = null)
        {
            var forId = id is null ? "" : $"for Id: '{id}'";
            return new Error("record.not.found", $"{entityName} not found {forId}");
        }

        public static Error Forbidden(string message = "You are not authorized to perform this action")
        {
            return new Error("forbidden", message);
        }
    }
}