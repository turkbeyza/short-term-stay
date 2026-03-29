using Microsoft.EntityFrameworkCore;
using short_term_stay_project.Data;
using short_term_stay_project.DTOs;
using short_term_stay_project.Models;

namespace short_term_stay_project.Services;

public class ReviewService : IReviewService
{
    private readonly ShortTermStayDbContext _context;

    public ReviewService(ShortTermStayDbContext context)
    {
        _context = context;
    }

    public async Task<ReviewResponse?> AddReviewAsync(int guestId, ReviewCreateRequest request)
    {
        var booking = await _context.Bookings
            .Include(b => b.Review)
            .FirstOrDefaultAsync(b => b.Id == request.BookingId && b.GuestId == guestId);

        // Validation for review: Must be the booker and no previous review
        if (booking == null || booking.Review != null)
            return null;

        var review = new Review
        {
            BookingId = request.BookingId,
            GuestId = guestId,
            ListingId = booking.ListingId,
            Rating = request.Rating,
            Comment = request.Comment
        };

        _context.Reviews.Add(review);
        await _context.SaveChangesAsync();

        return new ReviewResponse(review.Id, review.BookingId, review.Rating, review.Comment);
    }
}
