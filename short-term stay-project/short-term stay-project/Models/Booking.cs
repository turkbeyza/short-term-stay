namespace short_term_stay_project.Models;

public class Booking
{
    public int Id { get; set; }
    public int ListingId { get; set; }
    public Listing Listing { get; set; } = null!;
    
    public int GuestId { get; set; }
    public User Guest { get; set; } = null!;
    
    public DateTime From { get; set; }
    public DateTime To { get; set; }
    public string NamesOfPeople { get; set; } = string.Empty;
    
    public Review? Review { get; set; }
}
