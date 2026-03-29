using System.Security.Claims;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using short_term_stay_project.Services;

namespace short_term_stay_project.Controllers.v1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IListingService _listingService;
    private readonly IFileProcessingService _fileProcessingService;

    public AdminController(IListingService listingService, IFileProcessingService fileProcessingService)
    {
        _listingService = listingService;
        _fileProcessingService = fileProcessingService;
    }

    [HttpGet("listings/report")]
    public async Task<IActionResult> GetReport([FromQuery] string? country, [FromQuery] string? city, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var response = await _listingService.GetReportAsync(country, city, pageNumber, pageSize);
        return Ok(response);
    }

    [HttpPost("listings/upload")]
    public async Task<IActionResult> UploadListings(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("CSV file is required.");

        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        using var stream = file.OpenReadStream();
        var count = await _fileProcessingService.ProcessListingsCsvAsync(userId, stream);

        return Ok(new { Status = "Successful", Message = $"{count} listings imported.", FileProcessingStatus = "Successful" });
    }
}
