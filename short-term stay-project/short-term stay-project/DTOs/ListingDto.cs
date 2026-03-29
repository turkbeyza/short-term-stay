namespace short_term_stay_project.DTOs;

public record ListingCreateRequest(int NoOfPeople, string Country, string City, decimal Price);

public record ListingQueryRequest(
    DateTime? DateFrom, 
    DateTime? DateTo, 
    int? NoOfPeople, 
    string? Country, 
    string? City,
    int PageNumber = 1,
    int PageSize = 10
);

public record ListingResponse(
    int Id, 
    int HostId, 
    int NoOfPeople, 
    string Country, 
    string City, 
    decimal Price, 
    double AverageRating
);

public record ListingReportResponse(
    int Id,
    string Country,
    string City,
    decimal Price,
    double AverageRating
);
