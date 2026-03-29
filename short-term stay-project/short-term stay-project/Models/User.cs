namespace short_term_stay_project.Models;

public enum UserRole
{
    Host,
    Guest,
    Admin
}

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    
    public ICollection<Listing> Listings { get; set; } = new List<Listing>();
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
