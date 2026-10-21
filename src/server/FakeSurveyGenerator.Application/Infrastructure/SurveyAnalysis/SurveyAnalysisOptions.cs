namespace FakeSurveyGenerator.Application.Infrastructure.SurveyAnalysis;

internal sealed class SurveyAnalysisOptions
{
    public const string SectionName = "SurveyAnalysis";

    /// <summary>
    /// The Microsoft Foundry resource endpoint, for example <c>https://&lt;resource&gt;.services.ai.azure.com/</c>.
    /// Survey analysis is unavailable when this is not configured.
    /// </summary>
    public string? Endpoint { get; set; }

    /// <summary>
    /// The name of the Microsoft-Decision-1 model deployment.
    /// </summary>
    public string Deployment { get; set; } = "decision-1";

    /// <summary>
    /// Pins the Microsoft Entra tenant used to acquire tokens. Developer tools can be signed in to several tenants.
    /// </summary>
    public string? TenantId { get; set; }
}
