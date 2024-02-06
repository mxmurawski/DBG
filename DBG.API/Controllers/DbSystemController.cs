#region

using DBG.Infrastructure.Interfaces;
using DBG.Infrastructure.Models.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

#endregion

namespace DBG.API.Controllers;

[ApiController]
[Route("[controller]/[action]/{guid:guid}")]
public class DbSystemController
    : Controller
{
    private readonly ILogger<DbSystemController> logger;
    private readonly IDbSystemService systemService;

    public DbSystemController(ILogger<DbSystemController> logger, IDbSystemService systemService)
    {
        this.logger = logger;
        this.systemService = systemService;
    }

    [HttpPost]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> CreateUserAsync(Guid guid, [FromBody] CreateRequest request)
    {
        _ = HttpContext.Request.Headers.TryGetValue("Authorization", out var token);
        token = token.ToString().Replace("Bearer ", "");
        bool check;
        try
        {
            check = await systemService.CreateUserAsync(guid, request, token);
        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message);
            return NotFound(ex.Message);
        }

        return Ok(check);
    }

    [HttpPost]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> CreateLoginAsync(Guid guid, [FromBody] CreateRequest request)
    {
        _ = HttpContext.Request.Headers.TryGetValue("Authorization", out var token);
        token = token.ToString().Replace("Bearer ", "");
        bool check;
        try
        {
            check = await systemService.CreateLoginAsync(guid, request, token);
        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message);
            return NotFound(ex.Message);
        }

        return Ok(check);
    }

    [HttpPost]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> DropUserAsync(Guid guid, [FromBody] DropRequest request)
    {
        _ = HttpContext.Request.Headers.TryGetValue("Authorization", out var token);
        token = token.ToString().Replace("Bearer ", "");
        bool check;
        try
        {
            check = await systemService.DropUserAsync(guid, request, token);
        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message);
            return NotFound(ex.Message);
        }

        return Ok(check);
    }

    [HttpPost]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> DropLoginAsync(Guid guid, [FromBody] DropRequest request)
    {
        _ = HttpContext.Request.Headers.TryGetValue("Authorization", out var token);
        token = token.ToString().Replace("Bearer ", "");
        bool check;
        try
        {
            check = await systemService.DropLoginAsync(guid, request, token);
        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message);
            return NotFound(ex.Message);
        }

        return Ok(check);
    }

    [HttpPost]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> GrantAsync(Guid guid, [FromBody] GrantOrRevokeRequest request)
    {
        _ = HttpContext.Request.Headers.TryGetValue("Authorization", out var token);
        token = token.ToString().Replace("Bearer ", "");
        bool ckeck;
        try
        {
            ckeck = await systemService.GrantAsync(guid, request, token);
        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message);
            return NotFound(ex.Message);
        }

        return Ok(ckeck);
    }

    [HttpPost]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> RevokeAsync(Guid guid, [FromBody] GrantOrRevokeRequest request)
    {
        _ = HttpContext.Request.Headers.TryGetValue("Authorization", out var token);
        token = token.ToString().Replace("Bearer ", "");
        bool check;
        try
        {
            check = await systemService.RevokeAsync(guid, request, token);
        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message);
            return NotFound(ex.Message);
        }

        return Ok(check);
    }
}