using System.Security.Claims;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using short_term_stay_project.DTOs;
using short_term_stay_project.Services;

namespace short_term_stay_project.Controllers.v1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
public class ListingsController : ControllerBase
{
    private readonly IListingService _listingService;

    public ListingsController(IListingService listingService)
    {
        _listingService = listingService;
    }

    [HttpPost]
    [Authorize(Roles = "Host")]
    public async Task<IActionResult> CreateListing([FromBody] ListingCreateRequest request)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var response = await _listingService.CreateListingAsync(userId, request);
        return Ok(new { Status = "Successful", Data = response });
    }

    [HttpGet]
    public async Task<IActionResult> QueryListings([FromQuery] ListingQueryRequest request)
    {
        var response = await _listingService.QueryListingsAsync(request);
        return Ok(response);
    }
}
