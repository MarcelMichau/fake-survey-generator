namespace FakeSurveyGenerator.Application.Common.Errors
{
    public static class Errors
    {
        public static class General
        {
            public static Error NotFound(string entityName = "Record", long? id = null)
            {
                var forId = id == null ? "" : $" for Id '{id}'";
                return new Error("record.not.found", $"{entityName} not found{forId}");
            }
        }
    }
}
