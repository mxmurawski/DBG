#region

using DBG.Infrastructure.Interfaces;
using DBG.Infrastructure.Models.Db;
using DBG.Infrastructure.Models.Helper;
using DBG.Infrastructure.Models.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

#endregion

namespace DBG.MSSQLWorker.Controllers;

[ApiController]
[Route("{guid:guid}/[action]")]
public class MainController
    : Controller
{
    private readonly IConnectionManager connectionManager;
    private readonly ILogger<MainController> logger;

    public MainController(ILogger<MainController> logger, IConnectionManager connectionManager)
    {
        this.logger = logger;
        this.connectionManager = connectionManager;
    }

    [HttpGet]
    [Authorize(Policy = "Viewer")]
    public async Task<IActionResult> TestSystemConnectivityAsync(Guid guid)
    {
        bool check;
        try
        {
            check = await connectionManager.TestConnectivityAsync(guid);
        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message);
            return NotFound(ex.Message);
        }

        return Ok(check);
    }

    [HttpGet]
    [Authorize(Policy = "Viewer")]
    public async Task<IActionResult> GetStaticStateAsync(Guid guid)
    {
        DbStaticState? state;
        try
        {
            state = await connectionManager.PerformReadDbStaticStateAsync(guid);
        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message);
            return NotFound(ex.Message);
        }

        return Ok(state);
    }

    [HttpGet]
    [Authorize(Policy = "Viewer")]
    public async Task<IActionResult> GetDynamicStateAsync(Guid guid)
    {
        DbDynamicState? state;
        try
        {
            state = await connectionManager.PerformReadDbDynamicStateAsync(guid);
        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message);
            return NotFound(ex.Message);
        }

        return Ok(state);
    }

    [HttpPost]
    [Route("{dbname}")]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> ExecuteQueryAsync(Guid guid, string dbname, [FromBody] string query)
    {
        DbResult? result;
        try
        {
            result = await connectionManager.ExecuteQueryAsync(guid, query, dbname);
        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message);
            return NotFound(ex.Message);
        }

        return Ok(result);
    }

    [HttpPost]
    [Route("{dbname}")]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> ExecuteNonQueryAsync(Guid guid, string dbname, [FromBody] string query)
    {
        try
        {
            await connectionManager.ExecuteNonQueryAsync(guid, query, dbname);
        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message);
            return NotFound(ex.Message);
        }

        return Ok();
    }

    [HttpDelete]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> DeleteConnectionAsync(Guid guid)
    {
        bool changed;
        try
        {
            changed = await connectionManager.DeleteConnectionAsync(guid);
        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message);
            return NotFound(ex.Message);
        }

        return Ok(changed);
    }

    [HttpGet]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> AddOrUpdateConnectionAsync(Guid guid)
    {
        bool changed;
        try
        {
            changed = await connectionManager.AddOrUpdateConnectionAsync(guid);
        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message);
            return NotFound(ex.Message);
        }

        return Ok(changed);
    }

    [HttpGet]
    [Authorize(Policy = "Viewer")]
    public async Task<IActionResult> GetPermissionAuditResultAsync(Guid guid)
    {
        string? result;
        try
        {
            result = await connectionManager.GetPermissionAuditResultAsync(guid);
        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message);
            return NotFound(ex.Message);
        }

        return Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> CreateUserAsync(Guid guid, [FromBody] CreateRequest request)
    {
        bool check;
        try
        {
            check = await connectionManager.CreateUserAsync(guid, request);
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
        bool check;
        try
        {
            check = await connectionManager.CreateLoginAsync(guid, request);
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
        bool check;
        try
        {
            check = await connectionManager.DropUserAsync(guid, request);
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
        bool check;
        try
        {
            check = await connectionManager.DropLoginAsync(guid, request);
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
        bool ckeck;
        try
        {
            ckeck = await connectionManager.GrantAsync(guid, request);
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
        _ = token.ToString().Replace("Bearer ", "");
        bool check;
        try
        {
            check = await connectionManager.RevokeAsync(guid, request);
        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message);
            return NotFound(ex.Message);
        }

        return Ok(check);
    }
}