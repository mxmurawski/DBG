#region

using DBG.Infrastructure.Interfaces;
using DBG.Infrastructure.Models.Dto;
using DBG.Infrastructure.Models.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

#endregion

namespace DBG.API.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class UserController
    : Controller
{
    private readonly IAuthService authService;
    private readonly ILogger<UserController> logger;
    private readonly IPersistenceService persistenceService;

    public UserController(
        ILogger<UserController> logger,
        IPersistenceService persistenceService,
        IAuthService authService)
    {
        this.logger = logger;
        this.persistenceService = persistenceService;
        this.authService = authService;
    }

    [HttpGet]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> GetAsync()
    {
        IList<UserDto>? users;
        try
        {
            users = await persistenceService.GetUserDtosAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message);
            return NotFound(ex.Message);
        }

        return Ok(users);
    }

    [HttpGet]
    [Route("{guid:guid}")]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> GetAsync(Guid guid)
    {
        UserDto? user;
        try
        {
            user = await persistenceService.GetUserDtoAsync(guid);
        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message);
            return NotFound(ex.Message);
        }

        return Ok(user);
    }

    [HttpPost]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> AddAsync([FromBody] UserAddRequest request)
    {
        Guid g;
        try
        {
            g = await persistenceService.RegisterUserAsync(request);
        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message);
            return NotFound(ex.Message);
        }

        return Ok(g);
    }

    [HttpPut]
    [Route("{guid:guid}")]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> UpdateAsync(Guid guid, [FromBody] UserUpdateRequest request)
    {
        Guid g;
        try
        {
            g = await persistenceService.UpdateUserAsync(guid, request);
        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message);
            return NotFound(ex.Message);
        }

        return Ok(g);
    }

    [HttpDelete]
    [Route("{guid:guid}")]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> DeleteAsync(Guid guid)
    {
        Guid g;
        try
        {
            g = await persistenceService.DeleteUserAsync(guid);
        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message);
            return NotFound(ex.Message);
        }

        return Ok(g);
    }

    [HttpPut]
    [Authorize(Policy = "Viewer")]
    public async Task<IActionResult> UpdateMyPasswordAsync([FromBody] UsersPasswordUpdateRequest request)
    {
        bool changed;
        _ = HttpContext.Request.Headers.TryGetValue("Authorization", out var token);
        token = token.ToString().Replace("Bearer ", "");
        try
        {
            changed = await authService.UpdateMyPasswordAsync(token!, request.Password);
        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message);
            return NotFound(ex.Message);
        }

        return Ok(changed);
    }
}