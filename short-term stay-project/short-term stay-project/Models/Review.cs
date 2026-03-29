namespace short_term_stay_project.Models;

public class Review
{
    public int Id { get; set; }
    
    public int BookingId { get; set; }
    public Booking Booking { get; set; } = null!;
    
    public int GuestId { get; set; }
    public User Guest { get; set; } = null!;
    
    public int ListingId { get; set; }
    public Listing Listing { get; set; } = null!;
    
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
}
