#region

using DBG.Infrastructure.Interfaces;
using DBG.Infrastructure.Models.Db;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

#endregion

namespace DBG.SSHWorker.Controllers;

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
        OsStaticState? state;
        try
        {
            state = await connectionManager.PerformReadOsStaticStateAsync(guid);
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
        OsDynamicState? state;
        try
        {
            state = await connectionManager.PerformReadOsDynamicStateAsync(guid);
        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message);
            return NotFound(ex.Message);
        }

        return Ok(state);
    }

    [HttpPost]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> ExecuteCommandAsync(Guid guid, [FromBody] string command)
    {
        string? result;
        try
        {
            result = await connectionManager.ExecuteCommandAsync(guid, command);
        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message);
            return NotFound(ex.Message);
        }

        return Ok(result);
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
}