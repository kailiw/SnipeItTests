using System.Threading.Tasks;
using NUnit.Framework;

[TestFixture]
public class SnipeTests
{
    private PlaywrightFixture _fixture = null!;
    private SnipeItHelper _helper = null!;

    [OneTimeSetUp]
    public async Task Setup()
    {
        _fixture = new PlaywrightFixture();
        await _fixture.InitializeAsync();
        _helper = new SnipeItHelper(_fixture.Page);
    }

    [OneTimeTearDown]
    public async Task Teardown()
    {
        await _fixture.DisposeAsync();
    }

    [Test]
    public async Task Test_CreateAndVerifyAsset()
    {
        await _helper.LoginAsync("admin", "password");
        var assetTag = await _helper.CreateAssetAsync();
        await _helper.VerifyAssetAsync(assetTag);
    }
}
