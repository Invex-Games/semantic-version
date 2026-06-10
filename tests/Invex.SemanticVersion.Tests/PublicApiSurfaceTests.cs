namespace Invex.SemanticVersion.Tests;

[TestFixture]
public class PublicApiTests
{
    [Test]
    public async Task VerifyPublicApiSurface()
    {
        await VerifyJson(PublicApiSurfaceTestUtil.GetPublicApiSurface(typeof(SemVer).Assembly));
    }
}
