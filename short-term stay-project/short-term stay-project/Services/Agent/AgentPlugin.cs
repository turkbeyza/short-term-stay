using System.ComponentModel;
using Microsoft.SemanticKernel;

namespace short_term_stay_project.Services.Agent;

/// <summary>
/// Semantic Kernel plugin wrapper around ShortStayMcpTools.
/// The chat UI uses this via SK's function calling.
/// The same tool logic lives in ShortStayMcpTools (single source of truth).
/// </summary>
public class AgentPlugin
{
    private readonly ShortStayMcpTools _tools;

    public AgentPlugin(ShortStayMcpTools tools)
    {
        _tools = tools;
    }

    [KernelFunction("query_listings")]
    [Description("Search for available short-term stay listings. All parameters are optional. Returns a JSON array of matching listings.")]
    public Task<string> QueryListingsAsync(
        [Description("City to search in (optional)")] string? city = null,
        [Description("Country to search in (optional)")] string? country = null,
        [Description("Number of people (optional, digits only)")] string? noOfPeople = null,
        [Description("Check-in date (optional, yyyy-MM-dd)")] string? dateFrom = null,
        [Description("Check-out date (optional, yyyy-MM-dd)")] string? dateTo = null)
    {
        int? people = null;
        if (!string.IsNullOrWhiteSpace(noOfPeople) && int.TryParse(noOfPeople.Trim(), out var p))
            people = p;

        return _tools.QueryListingsAsync(city, country, people, dateFrom, dateTo);
    }

    [KernelFunction("book_listing")]
    [Description("Book a listing. Requires the listing ID, check-in date, check-out date, and names of guests.")]
    public Task<string> BookListingAsync(
        [Description("The numeric listing ID")] string listingId,
        [Description("Check-in date (yyyy-MM-dd)")] string fromDate,
        [Description("Check-out date (yyyy-MM-dd)")] string toDate,
        [Description("Full names of all guests, comma-separated")] string namesOfPeople)
    {
        if (!int.TryParse(listingId?.Trim(), out var id))
            return Task.FromResult("Error: Invalid listing ID. Please provide a numeric ID.");

        return _tools.BookListingAsync(id, fromDate, toDate, namesOfPeople);
    }

    [KernelFunction("review_listing")]
    [Description("Submit a review for a completed booking. Rating must be 1 to 5.")]
    public Task<string> ReviewListingAsync(
        [Description("The numeric booking ID")] string bookingId,
        [Description("Rating from 1 (worst) to 5 (best)")] string rating,
        [Description("Written review comment")] string comment)
    {
        if (!int.TryParse(bookingId?.Trim(), out var bid))
            return Task.FromResult("Error: Invalid booking ID.");
        if (!int.TryParse(rating?.Trim(), out var r))
            return Task.FromResult("Error: Invalid rating. Must be a number 1-5.");

        return _tools.ReviewListingAsync(bid, r, comment);
    }
}
