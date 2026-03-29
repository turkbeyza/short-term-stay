namespace short_term_stay_project.DTOs;

public record BookingCreateRequest(
    int ListingId, 
    DateTime From, 
    DateTime To, 
    string NamesOfPeople
);

public record BookingResponse(
    int Id, 
    int ListingId, 
    DateTime From, 
    DateTime To, 
    string NamesOfPeople
);
