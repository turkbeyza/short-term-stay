using short_term_stay_project.DTOs;
using short_term_stay_project.Models;

namespace short_term_stay_project.Services;

public interface IAuthService
{
    Task<LoginResponse?> LoginAsync(LoginRequest request);
    Task<bool> RegisterAsync(RegisterRequest request);
}

public interface IListingService
{
    Task<ListingResponse> CreateListingAsync(int hostId, ListingCreateRequest request);
    Task<IEnumerable<ListingResponse>> QueryListingsAsync(ListingQueryRequest request);
    Task<IEnumerable<ListingReportResponse>> GetReportAsync(string? country, string? city, double? minRating, int pageNumber, int pageSize);
}

public interface IBookingService
{
    Task<BookingResponse?> BookStayAsync(int guestId, BookingCreateRequest request);
    Task<bool> IsListingAvailableAsync(int listingId, DateTime from, DateTime to);
}

public interface IReviewService
{
    Task<ReviewResponse?> AddReviewAsync(int guestId, ReviewCreateRequest request);
}

public interface IFileProcessingService
{
    Task<int> ProcessListingsCsvAsync(int hostId, Stream fileStream);
}
