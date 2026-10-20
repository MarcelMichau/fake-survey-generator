namespace FakeSurveyGenerator.Acceptance.Tests;

public sealed class UiDiagnosticCaptureTests
{
    [Test]
    public async Task GivenTimedOutCapture_WhenOperationCompletesLater_ThenArtifactIsNotAttached()
    {
        var pendingCapture = new TaskCompletionSource();
        var attachments = 0;

        var captured = await UiTestDiagnostics.TryCaptureAsync(
            "pending capture", () => pendingCapture.Task, () => attachments++, TimeSpan.Zero);

        await Assert.That(captured).IsFalse();
        pendingCapture.SetResult();
        await Assert.That(attachments).IsEqualTo(0);
    }

    [Test]
    public async Task GivenCompletedCapture_WhenOperationFinishes_ThenArtifactIsAttachedOnce()
    {
        var attachments = 0;

        var captured = await UiTestDiagnostics.TryCaptureAsync(
            "completed capture", () => Task.CompletedTask, () => attachments++);

        await Assert.That(captured).IsTrue();
        await Assert.That(attachments).IsEqualTo(1);
    }
}
