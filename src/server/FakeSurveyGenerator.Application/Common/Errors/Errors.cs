namespace FakeSurveyGenerator.Application.Common.Errors
{
    public static class Errors
    {
        public static class General
        {
            public static Error NotFound(string entityName, int id) =>
                new Error("record.not.found", $"'{entityName}' not found for Id '{id}'");
        }
    }
}
