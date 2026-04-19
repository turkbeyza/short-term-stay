using System.ComponentModel;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol.Server;
using short_term_stay_project.Data;
using short_term_stay_project.DTOs;
using short_term_stay_project.Models;
using short_term_stay_project.Services;

namespace short_term_stay_project.Services.Agent;

/// <summary>
/// Single source of truth for all AI agent tools.
/// Modified to be "Root-Safe" by resolving scoped dependencies from an IServiceScopeFactory.
/// </summary>
[McpServerToolType]
public class ShortStayMcpTools
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ShortStayMcpTools> _logger;
    private static readonly JsonSerializerOptions _json = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public ShortStayMcpTools(IServiceScopeFactory scopeFactory, ILogger<ShortStayMcpTools> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    [McpServerTool(Name = "query_listings")]
    [Description("Search for available short-term stay listings. Returns a JSON array of matching listings.")]
    public async Task<string> QueryListingsAsync(
        [Description("City to search in (optional)")] string? city = null,
        [Description("Country to search in (optional)")] string? country = null,
        [Description("Minimum number of people (optional)")] int? noOfPeople = null,
        [Description("Check-in date (yyyy-MM-dd)")] string? dateFrom = null,
        [Description("Check-out date (yyyy-MM-dd)")] string? dateTo = null)
    {
        using var scope = _scopeFactory.CreateScope();
        var listingService = scope.ServiceProvider.GetRequiredService<IListingService>();

        DateTime? from = dateFrom != null && DateTime.TryParse(dateFrom, out var df) ? df : null;
        DateTime? to   = dateTo   != null && DateTime.TryParse(dateTo,   out var dt) ? dt : null;

        var request = new ListingQueryRequest(
            DateFrom:   from,
            DateTo:     to,
            NoOfPeople: noOfPeople,
            Country:    country?.Trim(),
            City:       city?.Trim(),
            MinRating:  null,
            PageNumber: 1,
            PageSize:   20
        );

        var results = (await listingService.QueryListingsAsync(request)).ToList();
        return results.Any() ? JsonSerializer.Serialize(results, _json) : "No listings found matching the given criteria.";
    }

    [McpServerTool(Name = "book_listing")]
    [Description("Book a listing. Requires ID, dates, and guest names.")]
    public async Task<string> BookListingAsync(
        [Description("Numeric listing ID")] int listingId,
        [Description("Check-in date (yyyy-MM-dd)")] string fromDate,
        [Description("Check-out date (yyyy-MM-dd)")] string toDate,
        [Description("Guest names, comma-separated")] string namesOfPeople)
    {
        using var scope = _scopeFactory.CreateScope();
        var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
        var context = scope.ServiceProvider.GetRequiredService<ShortTermStayDbContext>();

        if (!DateTime.TryParse(fromDate, out var df)) return "Error: Invalid check-in date.";
        if (!DateTime.TryParse(toDate,   out var dt)) return "Error: Invalid check-out date.";

        var agentUserId = await GetOrCreateAgentUserIdAsync(context);
        var result = await bookingService.BookStayAsync(agentUserId, new BookingCreateRequest(listingId, df, dt, namesOfPeople));

        return result == null 
            ? "Booking failed: listing not available." 
            : $"Booking confirmed! ID: {result.Id}, from {result.From:yyyy-MM-dd} to {result.To:yyyy-MM-dd}.";
    }

    [McpServerTool(Name = "review_listing")]
    [Description("Submit a review for a completed booking.")]
    public async Task<string> ReviewListingAsync(
        [Description("Numeric booking ID")] int bookingId,
        [Description("Rating 1-5")] int rating,
        [Description("Review comment")] string comment)
    {
        using var scope = _scopeFactory.CreateScope();
        var reviewService = scope.ServiceProvider.GetRequiredService<IReviewService>();
        var context = scope.ServiceProvider.GetRequiredService<ShortTermStayDbContext>();

        if (rating < 1 || rating > 5) return "Error: Rating must be 1-5.";

        var agentUserId = await GetOrCreateAgentUserIdAsync(context);
        var result = await reviewService.AddReviewAsync(agentUserId, new ReviewCreateRequest(bookingId, rating, comment));

        return result == null 
            ? "Review failed." 
            : $"Review submitted! Rating: {result.Rating}/5.";
    }

    private async Task<int> GetOrCreateAgentUserIdAsync(ShortTermStayDbContext context)
    {
        var user = await context.Users.FirstOrDefaultAsync(u => u.Username == "agent_user");
        if (user != null) return user.Id;

        user = new User { Username = "agent_user", PasswordHash = "AgentPassword123!", Role = UserRole.Guest };
        context.Users.Add(user);
        await context.SaveChangesAsync();
        return user.Id;
    }
}
