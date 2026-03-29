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
public class ReviewsController : ControllerBase
{
    private readonly IReviewService _reviewService;

    public ReviewsController(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    [HttpPost]
    [Authorize(Roles = "Guest")]
    public async Task<IActionResult> AddReview([FromBody] ReviewCreateRequest request)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var response = await _reviewService.AddReviewAsync(userId, request);
        
        if (response == null)
            return BadRequest(new { Status = "Error", Message = "Review validation failed (must be the booker and no previous review)." });

        return Ok(new { Status = "Successful", Data = response });
    }
}
