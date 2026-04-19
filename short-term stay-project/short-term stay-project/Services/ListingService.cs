using Microsoft.EntityFrameworkCore;
using short_term_stay_project.Data;
using short_term_stay_project.DTOs;
using short_term_stay_project.Models;

namespace short_term_stay_project.Services;

public class ListingService : IListingService
{
    private readonly ShortTermStayDbContext _context;

    public ListingService(ShortTermStayDbContext context)
    {
        _context = context;
    }

    public async Task<ListingResponse> CreateListingAsync(int hostId, ListingCreateRequest request)
    {
        var listing = new Listing
        {
            HostId = hostId,
            NoOfPeople = request.NoOfPeople,
            Country = request.Country,
            City = request.City,
            Price = request.Price
        };

        _context.Listings.Add(listing);
        await _context.SaveChangesAsync();

        return new ListingResponse(listing.Id, listing.HostId, listing.NoOfPeople, listing.Country, listing.City, listing.Price, 0);
    }

    public async Task<IEnumerable<ListingResponse>> QueryListingsAsync(ListingQueryRequest request)
    {
        var query = _context.Listings.AsQueryable();

        if (request.NoOfPeople.HasValue)
            query = query.Where(l => l.NoOfPeople >= request.NoOfPeople.Value);

        if (!string.IsNullOrEmpty(request.Country))
            query = query.Where(l => EF.Functions.Like(l.Country, request.Country));

        if (!string.IsNullOrEmpty(request.City))
            query = query.Where(l => EF.Functions.Like(l.City, request.City));

        // Date overlap filtering — works if either or both dates provided
        if (request.DateFrom.HasValue || request.DateTo.HasValue)
        {
            var from = request.DateFrom ?? DateTime.MinValue;
            var to = request.DateTo ?? DateTime.MaxValue;
            query = query.Where(l => !l.Bookings.Any(b =>
                (from < b.To && to > b.From)));
        }

        var projectedQuery = query.Select(l => new ListingResponse(
            l.Id, 
            l.HostId, 
            l.NoOfPeople, 
            l.Country, 
            l.City, 
            l.Price, 
            l.Reviews.Any() ? l.Reviews.Average(r => (double)r.Rating) : 0
        ));

        if (request.MinRating.HasValue)
            projectedQuery = projectedQuery.Where(l => l.AverageRating >= request.MinRating.Value);

        return await projectedQuery
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();
    }

    public async Task<IEnumerable<ListingReportResponse>> GetReportAsync(string? country, string? city, double? minRating, int pageNumber, int pageSize)
    {
        var query = _context.Listings.AsQueryable();

        if (!string.IsNullOrEmpty(country))
            query = query.Where(l => l.Country == country);

        if (!string.IsNullOrEmpty(city))
            query = query.Where(l => l.City == city);

        var projectedQuery = query.Select(l => new ListingReportResponse(
            l.Id, 
            l.Country, 
            l.City, 
            l.Price, 
            l.Reviews.Any() ? l.Reviews.Average(r => (double)r.Rating) : 0
        ));

        if (minRating.HasValue)
            projectedQuery = projectedQuery.Where(l => l.AverageRating >= minRating.Value);

        return await projectedQuery
            .OrderByDescending(l => l.AverageRating)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }
}
