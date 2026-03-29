using System.Globalization;
using CsvHelper;
using short_term_stay_project.Data;
using short_term_stay_project.Models;

namespace short_term_stay_project.Services;

public class FileProcessingService : IFileProcessingService
{
    private readonly ShortTermStayDbContext _context;

    public FileProcessingService(ShortTermStayDbContext context)
    {
        _context = context;
    }

    public async Task<int> ProcessListingsCsvAsync(int hostId, Stream fileStream)
    {
        using var reader = new StreamReader(fileStream);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

        var records = csv.GetRecords<CsvListingRecord>();
        int count = 0;

        foreach (var record in records)
        {
            var listing = new Listing
            {
                HostId = hostId,
                NoOfPeople = record.NoOfPeople,
                Country = record.Country,
                City = record.City,
                Price = record.Price
            };
            _context.Listings.Add(listing);
            count++;
        }

        await _context.SaveChangesAsync();
        return count;
    }

    private class CsvListingRecord
    {
        public int NoOfPeople { get; set; }
        public string Country { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public decimal Price { get; set; }
    }
}
