namespace FakeSurveyGenerator.Application.Common.Errors
{
    public sealed record Error
    {
        public string Code { get; }
        public string Message { get; }

        internal Error(string code, string message)
        {
            Code = code;
            Message = message;
        }
    }
}
