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

        public static Error UserProfileIncomplete()
        {
            return new Error("user.profile.incomplete",
                "The identity provider did not supply a display name and email address for the current user.");
        }

        public static Error UserNotRegistered()
        {
            return new Error("user.not.registered",
                "The current user is not registered. Register the user before performing this action.");
        }
    }
}