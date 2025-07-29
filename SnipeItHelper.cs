using System;
using System.Threading.Tasks;
using Microsoft.Playwright;
using NUnit.Framework;
using NUnit.Framework.Legacy;

public class SnipeItHelper
{
    private readonly IPage _page;

    public SnipeItHelper(IPage page)
    {
        _page = page;
    }

    public async Task LoginAsync(string username, string password)
    {
        await _page.GotoAsync("https://demo.snipeitapp.com/login");
        await _page.FillAsync("#username", username);
        await _page.FillAsync("#password", password);
        await _page.ClickAsync("button[type='submit']");
        await _page.WaitForURLAsync(url => !url.Contains("/login"));
    }

    public async Task<string> CreateAssetAsync()
    {
        // Click 'Create New' dropdown
        await _page.ClickAsync("a.dropdown-toggle:has-text('Create New')");
        // Select 'Asset' from dropdown
        await _page.ClickAsync("ul.dropdown-menu >> text=Asset");
        // Wait for create form to load
        await _page.WaitForSelectorAsync("#create-form");

        // Generate unique asset name and tag
        var assetName = $"Macbook Pro 13 - {Guid.NewGuid():N}".Substring(0, 20);
        var assetTag = $"AT-{Guid.NewGuid():N}".Substring(0, 8);

        // Select model from dropdown
        await _page.ClickAsync("#select2-model_select_id-container");
        await _page.FillAsync("input.select2-search__field", "Macbook Pro 13");
        await _page.ClickAsync("li.select2-results__option:has-text('Macbook Pro 13')");

        // Set status and asset tag
        await _page.SelectOptionAsync("#status_select_id", "1");
        await _page.FillAsync("#asset_tag", assetTag);

        // Show assignment controls
        await _page.EvaluateAsync("document.getElementById('assignto_selector').style.display = 'block'");
        await _page.EvaluateAsync("document.getElementById('assigned_user').style.display = 'block'");
        
        // Select user assignment
        await _page.CheckAsync("input[name='checkout_to_type'][value='user']");
        await _page.ClickAsync("#select2-assigned_user_select-container");
        await _page.FillAsync("input.select2-search__field", "");
        await _page.ClickAsync(".select2-results__option");

        // Select location
        await _page.ClickAsync("#select2-rtd_location_id_location_select-container");
        await _page.FillAsync("input.select2-search__field", "");
        await _page.ClickAsync(".select2-results__option");

        // Submit the form
        await _page.ClickAsync("#submit_button");
        await _page.WaitForURLAsync(url => !url.Contains("hardware"));

        return assetTag;
    }

    public async Task VerifyAssetAsync(string assetTag)
    {
        // Navigate to hardware page and search for asset
        await _page.GotoAsync("https://demo.snipeitapp.com/hardware");
        await _page.FillAsync("input.search-input", assetTag);
        await _page.PressAsync("input.search-input", "Enter");
        await _page.WaitForTimeoutAsync(1000);
        await _page.ClickAsync($"a:has-text('{assetTag}')");

        // Verify basic asset information
        var detailTag = await _page.InnerTextAsync("span.js-copy-assettag");
        var modelName = await _page.InnerTextAsync("div.col-md-9 a[href*='/models/']");

        // Extract status text through JavaScript evaluation
        var statusText = await _page.EvaluateAsync<string>(@"() => {
            const icon = document.querySelector('i.fa-solid.fa-circle.text-blue');
            if (!icon) return '';
            let node = icon.nextSibling;
            while (node && node.nodeType !== Node.TEXT_NODE) {
                node = node.nextSibling;
            }
            return node ? node.textContent.trim() : '';
        }");
        Console.WriteLine($"Found asset tag: {detailTag}, Model Name: {modelName}, Status: {statusText}");

        // Assertions for basic asset info
        Assert.That(detailTag, Is.EqualTo(assetTag),
            $"Asset tag '{detailTag}' does not match expected '{assetTag}'");

        Assert.That(modelName, Is.EqualTo("Macbook Pro 13\"").IgnoreCase,
            $"Model name '{modelName}' does not equal 'Macbook Pro 13' (case-insensitive)");

        Assert.That(statusText, Is.EqualTo("Ready to Deploy"),
            $"Status is '{statusText}', expected 'Ready to Deploy'");

        // Switch to History tab
        await _page.ClickAsync("a[data-toggle='tab'][href='#history']");
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle); // Wait for all network requests

        await _page.WaitForSelectorAsync("div#history tbody");

        // Get all history rows
        var rows = await _page.QuerySelectorAllAsync("div#history tbody tr");
        Assert.That(rows.Count, Is.EqualTo(2), "Should have exactly 2 history entries");

        // Validate first row (creation record)
        var firstRowCells = await rows[0].QuerySelectorAllAsync("td");
        await ValidateHistoryRow(
            cells: firstRowCells,
            expectedIconClass: "fa-plus",
            expectedUser: "Admin User",
            expectedAction: "create new",
            expectedAssetText: $"({assetTag}) - Macbook Pro 13\"",
            expectedNote: null
        );

        // Validate second row (checkout record)
        var secondRowCells = await rows[1].QuerySelectorAllAsync("td");
        await ValidateHistoryRow(
            cells: secondRowCells,
            expectedIconClass: "fa-rotate-left",
            expectedUser: "Admin User",
            expectedAction: "checkout",
            expectedAssetText: $"({assetTag}) - Macbook Pro 13\"",
            expectedNote: "Checked out on asset creation"
        );
    }

    private async Task ValidateHistoryRow(
        IReadOnlyList<IElementHandle> cells,
        string expectedIconClass,
        string expectedUser,
        string expectedAction,
        string expectedAssetText,
        string expectedNote)
    {
        // 1. Validate icon
        var icon = await cells[0].QuerySelectorAsync("i");
        var iconClass = await icon.GetAttributeAsync("class");
        Assert.That(iconClass, Does.Contain(expectedIconClass),
            $"Icon class '{iconClass}' should contain '{expectedIconClass}'");

        // 2. Validate user
        var userLink = await cells[2].QuerySelectorAsync("a");
        var userName = await userLink.InnerTextAsync();
        Assert.That(userName, Is.EqualTo(expectedUser),
            $"User name '{userName}' should be '{expectedUser}'");

        // 3. Validate action type
        var actionText = (await cells[3].InnerTextAsync()).Trim();
        Assert.That(actionText, Is.EqualTo(expectedAction),
            $"Action '{actionText}' should be '{expectedAction}'");

        // 4. Validate asset information
        var assetLink = await cells[4].QuerySelectorAsync("a");
        var assetText = await assetLink.InnerTextAsync();
        Assert.That(assetText, Does.Contain(expectedAssetText),
            $"Asset text '{assetText}' should contain '{expectedAssetText}'");

        // 5. Validate note (if expected)
        if (expectedNote != null)
        {
            var noteText = (await cells[8].InnerTextAsync()).Trim();
            Assert.That(noteText, Is.EqualTo(expectedNote),
                $"Note '{noteText}' should be '{expectedNote}'");
        }
    }
}