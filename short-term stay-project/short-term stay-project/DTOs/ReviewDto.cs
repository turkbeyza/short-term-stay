namespace short_term_stay_project.DTOs;

public record ReviewCreateRequest(
    int BookingId, 
    int Rating, 
    string Comment
);

public record ReviewResponse(
    int Id, 
    int BookingId, 
    int Rating, 
    string Comment
);
