using FakeSurveyGenerator.Api.Tests.Integration.Setup;

namespace FakeSurveyGenerator.Api.Tests.Integration.Shared;

public sealed class WebApplicationFactoryConcurrencyTests
{
    [Test]
    public async Task GivenSharedFactory_WhenCreatingChildrenConcurrently_ThenAllChildrenAreTrackedAndDisposeSuccessfully()
    {
        // Exercise the real factory bookkeeping without starting hosts or containers.
        for (var round = 0; round < 20; round++)
        {
            await using var factory = new IntegrationTestWebApplicationFactory(new AspireTestSettings("unused", "unused"));
            Parallel.For(0, 1024, new ParallelOptions { MaxDegreeOfParallelism = 16 }, _ =>
            {
                factory.WithWebHostBuilderLocked(_ => { });
            });

            await Assert.That(factory.Factories.Count).IsEqualTo(1024);
            await Assert.That(factory.Factories.All(child => child is not null)).IsTrue();
        }
    }
}
