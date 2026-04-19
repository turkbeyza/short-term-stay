using System.ComponentModel;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ModelContextProtocol.Server;

// ─────────────────────────────────────────────────────────────────────────────
// Short-Term Stay MCP Server (stdio transport)
//
// Exposes query_listings, book_listing, review_listing as MCP tools.
// Connects to the API Gateway (http://localhost:5006) via HTTP.
//
// Usage with Claude Desktop — add to claude_desktop_config.json:
// {
//   "mcpServers": {
//     "short-term-stay": {
//       "command": "dotnet",
//       "args": ["run", "--project", "PATH\\ShortTermStay.McpServer"]
//     }
//   }
// }
//
// Test with MCP Inspector:
//   npx @modelcontextprotocol/inspector dotnet run --project ShortTermStay.McpServer
// ─────────────────────────────────────────────────────────────────────────────

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSingleton<ApiClient>();

builder.Services.AddMcpServer()
    .WithStdioServerTransport()
    .WithTools<ShortStayTools>();

await builder.Build().RunAsync();


// ── API Client (handles auth + HTTP) ────────────────────────────────────────

public class ApiClient
{
    private readonly HttpClient _http = new() { BaseAddress = new Uri("http://localhost:5006") };
    private static string? _cachedToken;
    private static readonly SemaphoreSlim _lock = new(1, 1);

    private static readonly JsonSerializerOptions _json =
        new() { PropertyNameCaseInsensitive = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public async Task<string> GetAsync(string path)
    {
        await EnsureAuthAsync();
        var resp = await _http.GetAsync(path);
        var body = await resp.Content.ReadAsStringAsync();
        if (string.IsNullOrWhiteSpace(body) || body is "[]" or "null")
            return "No listings found matching the given criteria.";
        return body;
    }

    public async Task<string> PostAsync(string path, object payload)
    {
        await EnsureAuthAsync();
        var json    = JsonSerializer.Serialize(payload, _json);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var resp    = await _http.PostAsync(path, content);
        return await resp.Content.ReadAsStringAsync();
    }

    private async Task EnsureAuthAsync()
    {
        if (!string.IsNullOrEmpty(_cachedToken)) return;
        await _lock.WaitAsync();
        try
        {
            if (!string.IsNullOrEmpty(_cachedToken)) return;

            // Register agent_user (ignore failure if already exists)
            var reg = JsonSerializer.Serialize(new { Username = "agent_user", Password = "AgentPassword123!", Role = "Guest" });
            await _http.PostAsync("/api/v1/Auth/register",
                new StringContent(reg, Encoding.UTF8, "application/json"));

            // Login
            var login = JsonSerializer.Serialize(new { Username = "agent_user", Password = "AgentPassword123!" });
            var resp  = await _http.PostAsync("/api/v1/Auth/login",
                new StringContent(login, Encoding.UTF8, "application/json"));

            if (resp.IsSuccessStatusCode)
            {
                var body   = await resp.Content.ReadAsStringAsync();
                var parsed = JsonSerializer.Deserialize<JsonElement>(body);
                _cachedToken = parsed.GetProperty("token").GetString();
                _http.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", _cachedToken);
            }
        }
        finally { _lock.Release(); }
    }
}


// ── MCP Tools ────────────────────────────────────────────────────────────────

[McpServerToolType]
public class ShortStayTools(ApiClient api)
{
    [McpServerTool(Name = "query_listings")]
    [Description("Search for available short-term stay listings. All parameters are optional.")]
    public async Task<string> QueryListingsAsync(
        [Description("City to search in (optional)")] string? city = null,
        [Description("Country to search in (optional)")] string? country = null,
        [Description("Minimum number of people (optional)")] int? noOfPeople = null,
        [Description("Check-in date yyyy-MM-dd (optional)")] string? dateFrom = null,
        [Description("Check-out date yyyy-MM-dd (optional)")] string? dateTo = null)
    {
        var qs = new List<string>();
        if (!string.IsNullOrWhiteSpace(city))    qs.Add($"City={Uri.EscapeDataString(city.Trim())}");
        if (!string.IsNullOrWhiteSpace(country)) qs.Add($"Country={Uri.EscapeDataString(country.Trim())}");
        if (noOfPeople.HasValue)                 qs.Add($"NoOfPeople={noOfPeople}");
        if (!string.IsNullOrWhiteSpace(dateFrom) && DateTime.TryParse(dateFrom, out var df))
            qs.Add($"DateFrom={df:yyyy-MM-dd}");
        if (!string.IsNullOrWhiteSpace(dateTo) && DateTime.TryParse(dateTo, out var dt))
            qs.Add($"DateTo={dt:yyyy-MM-dd}");

        var url = "/api/v1/Listings" + (qs.Any() ? "?" + string.Join("&", qs) : "");
        return await api.GetAsync(url);
    }

    [McpServerTool(Name = "book_listing")]
    [Description("Book a listing. Requires listing ID, check-in/out dates, and guest names.")]
    public async Task<string> BookListingAsync(
        [Description("Numeric listing ID")] int listingId,
        [Description("Check-in date (yyyy-MM-dd)")] string fromDate,
        [Description("Check-out date (yyyy-MM-dd)")] string toDate,
        [Description("Guest names, comma-separated")] string namesOfPeople)
    {
        if (!DateTime.TryParse(fromDate, out var df)) return "Error: Invalid check-in date.";
        if (!DateTime.TryParse(toDate,   out var dt)) return "Error: Invalid check-out date.";
        return await api.PostAsync("/api/v1/Bookings",
            new { ListingId = listingId, From = df.ToString("yyyy-MM-dd"), To = dt.ToString("yyyy-MM-dd"), NamesOfPeople = namesOfPeople });
    }

    [McpServerTool(Name = "review_listing")]
    [Description("Submit a review for a completed booking. Rating must be 1-5.")]
    public async Task<string> ReviewListingAsync(
        [Description("Numeric booking ID")] int bookingId,
        [Description("Rating 1-5")] int rating,
        [Description("Review comment")] string comment)
    {
        if (rating < 1 || rating > 5) return "Error: Rating must be 1 to 5.";
        return await api.PostAsync("/api/v1/Reviews",
            new { BookingId = bookingId, Rating = rating, Comment = comment });
    }
}
