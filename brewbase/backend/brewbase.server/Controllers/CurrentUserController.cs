using brewbase.server.Dtos;
using brewbase.server.Services;
using Microsoft.AspNetCore.Mvc;

namespace brewbase.server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CurrentUserController : ControllerBase
{
    private readonly ICurrentUserProvider _currentUserProvider;

    public CurrentUserController(ICurrentUserProvider currentUserProvider)
    {
        _currentUserProvider = currentUserProvider;
    }

    /// <summary>
    /// Returns the resolved application user id for this request (for integration checks and demos).
    /// </summary>
    [HttpGet]
    public ActionResult<CurrentUserResponseDto> Get()
    {
        return Ok(new CurrentUserResponseDto { UserId = _currentUserProvider.GetUserId() });
    }
}
