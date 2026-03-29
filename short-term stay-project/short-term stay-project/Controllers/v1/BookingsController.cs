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
public class BookingsController : ControllerBase
{
    private readonly IBookingService _bookingService;

    public BookingsController(IBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    [HttpPost]
    [Authorize(Roles = "Guest")]
    public async Task<IActionResult> BookStay([FromBody] BookingCreateRequest request)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var response = await _bookingService.BookStayAsync(userId, request);
        
        if (response == null)
            return BadRequest(new { Status = "Error", Message = "Listing is not available for requested dates." });

        return Ok(new { Status = "Successful", Data = response });
    }
}
