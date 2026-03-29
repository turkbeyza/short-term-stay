namespace short_term_stay_project.Models;

public class Listing
{
    public int Id { get; set; }
    public int HostId { get; set; }
    public User Host { get; set; } = null!;
    
    public int NoOfPeople { get; set; }
    public string Country { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public decimal Price { get; set; }
    
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    
    public double AverageRating => Reviews.Any() ? Reviews.Average(r => r.Rating) : 0;
}
