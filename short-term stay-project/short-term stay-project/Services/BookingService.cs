using Microsoft.EntityFrameworkCore;
using short_term_stay_project.Data;
using short_term_stay_project.DTOs;
using short_term_stay_project.Models;

namespace short_term_stay_project.Services;

public class BookingService : IBookingService
{
    private readonly ShortTermStayDbContext _context;

    public BookingService(ShortTermStayDbContext context)
    {
        _context = context;
    }

    public async Task<BookingResponse?> BookStayAsync(int guestId, BookingCreateRequest request)
    {
        var available = await IsListingAvailableAsync(request.ListingId, request.From, request.To);
        if (!available)
            return null;

        var booking = new Booking
        {
            ListingId = request.ListingId,
            GuestId = guestId,
            From = request.From,
            To = request.To,
            NamesOfPeople = request.NamesOfPeople
        };

        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync();

        return new BookingResponse(booking.Id, booking.ListingId, booking.From, booking.To, booking.NamesOfPeople);
    }

    public async Task<bool> IsListingAvailableAsync(int listingId, DateTime from, DateTime to)
    {
        return !await _context.Bookings.AnyAsync(b => 
            b.ListingId == listingId && 
            (from < b.To && to > b.From));
    }
}
