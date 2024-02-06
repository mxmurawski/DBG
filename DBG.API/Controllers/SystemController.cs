#region

using DBG.Infrastructure.Interfaces;
using DBG.Infrastructure.Models.Db;
using DBG.Infrastructure.Models.Dto;
using DBG.Infrastructure.Models.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

#endregion

namespace DBG.API.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class SystemController
    : Controller
{
    private readonly IDbSystemService dbSystemService;
    private readonly ILogger<SystemController> logger;
    private readonly IOsSystemService osSystemService;
    private readonly IPersistenceService persistenceService;

    public SystemController(
        ILogger<SystemController> logger,
        IPersistenceService persistenceService,
        IDbSystemService dbSystemService,
        IOsSystemService osSystemService)
    {
        this.logger = logger;
        this.persistenceService = persistenceService;
        this.dbSystemService = dbSystemService;
        this.osSystemService = osSystemService;
    }

    [HttpPost]
    [Route("")]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> AddAsync([FromBody] SystemEntryAddRequest systemEntry)
    {
        Guid g;
        _ = HttpContext.Request.Headers.TryGetValue("Authorization", out var token);
        token = token.ToString().Replace("Bearer ", "");
        try
        {
            g = await persistenceService.AddSystemEntryAsync(systemEntry);
        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message);
            return NotFound(ex.Message);
        }

        try
        {
            await dbSystemService.AddOrUpdateWorkerConnectionAsync(g, token);
        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message);
        }

        try
        {
            await osSystemService.AddOrUpdateWorkerConnectionAsync(g, token);
        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message);
        }

        return Ok(g);
    }

    [HttpGet]
    [Authorize(Policy = "Viewer")]
    public async Task<IActionResult> GetAsync()
    {
        IList<SystemEntryDto>? systems;
        try
        {
            systems = await persistenceService.GetSystemEntryDtosFromDbAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message);
            return NotFound(ex.Message);
        }

        return Ok(systems);
    }

    [HttpGet]
    [Route("{guid:guid}")]
    [Authorize(Policy = "Viewer")]
    public async Task<IActionResult> GetAsync(Guid guid)
    {
        SystemEntryDto? system;
        try
        {
            system = await persistenceService.GetSystemEntryDtoFromDbAsync(guid);
        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message);
            return NotFound(ex.Message);
        }

        return Ok(system);
    }

    [HttpDelete]
    [Route("{guid:guid}")]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> DeleteAsync(Guid guid)
    {
        _ = HttpContext.Request.Headers.TryGetValue("Authorization", out var token);
        token = token.ToString().Replace("Bearer ", "");
        try
        {
            await dbSystemService.DeleteWorkerConnectionAsync(guid, token);
        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message);
        }

        try
        {
            await osSystemService.DeleteWorkerConnectionAsync(guid, token);
        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message);
        }

        Guid g;
        try
        {
            g = await persistenceService.DeleteSystemEntryAsync(guid);
        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message);
            return NotFound(ex.Message);
        }

        return Ok(g);
    }

    [HttpPatch]
    [Route("{guid:guid}")]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> UpdateEntrysNameAsync(Guid guid, [FromBody] SystemsNameUpdateRequest request)
    {
        Guid g;
        try
        {
            g = await persistenceService.UpdateEntrysNameAsync(guid, request);
        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message);
            return NotFound(ex.Message);
        }

        return Ok(g);
    }

    [HttpPatch]
    [Route("{guid:guid}")]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> UpdateOsEntryAsync(Guid guid, [FromBody] OsSystemEntryUpdateRequest osSystemEntry)
    {
        Guid g;
        _ = HttpContext.Request.Headers.TryGetValue("Authorization", out var token);
        token = token.ToString().Replace("Bearer ", "");
        try
        {
            g = await persistenceService.UpdateSystemEntrysOsEntryAsync(guid, osSystemEntry);
        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message);
            return NotFound(ex.Message);
        }

        try
        {
            await osSystemService.AddOrUpdateWorkerConnectionAsync(g, token);
        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message);
        }

        return Ok(g);
    }

    [HttpPatch]
    [Route("{guid:guid}")]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> UpdateDbEntryAsync(Guid guid, [FromBody] DbSystemEntryUpdateRequest dbSystemEntry)
    {
        Guid g;
        _ = HttpContext.Request.Headers.TryGetValue("Authorization", out var token);
        token = token.ToString().Replace("Bearer ", "");
        try
        {
            g = await persistenceService.UpdateSystemEntrysDbEntryAsync(guid, dbSystemEntry);
        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message);
            return NotFound(ex.Message);
        }
        try
        {
            await dbSystemService.AddOrUpdateWorkerConnectionAsync(g, token);
        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message);
        }
        return Ok(g);
    }

    [HttpGet]
    [Route("{guid:guid}")]
    [Authorize(Policy = "Viewer")]
    public async Task<IActionResult> TestSystemsOsSystemConnectionAsync(Guid guid)
    {
        bool result;
        _ = HttpContext.Request.Headers.TryGetValue("Authorization", out var token);
        token = token.ToString().Replace("Bearer ", "");
        try
        {
            result = await osSystemService.TestOsSystemAsync(guid, token);
        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message);
            return NotFound(ex.Message);
        }

        return Ok(result);
    }

    [HttpGet]
    [Route("{from:datetime}/{to:datetime}")]
    [Authorize(Policy = "Viewer")]
    public async Task<IActionResult> GetSystemsOsSystemsStaticStatesAsync(Guid guid, DateTime from, DateTime to)
    {
        IList<OsStaticState>? states;

        try
        {
            states = await osSystemService.GetOsStaticStatesFromDbAsync(guid, from, to);
        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message);
            return NotFound(ex.Message);
        }

        return Ok(states);
    }

    [HttpGet]
    [Route("{from:datetime}/{to:datetime}")]
    [Authorize(Policy = "Viewer")]
    public async Task<IActionResult> GetSystemsOsSystemsDynamicStatesAsync(Guid guid, DateTime from, DateTime to)
    {
        IList<OsDynamicState>? states;

        try
        {
            states = await osSystemService.GetOsDynamicStatesFromDbAsync(guid, from, to);
        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message);
            return NotFound(ex.Message);
        }

        return Ok(states);
    }

    [HttpGet]
    [Route("{guid:guid}")]
    [Authorize(Policy = "Viewer")]
    public async Task<IActionResult> GetSystemsOsSystemsLastStaticStateAsync(Guid guid)
    {
        OsStaticState? state;
        _ = HttpContext.Request.Headers.TryGetValue("Authorization", out var token);
        token = token.ToString().Replace("Bearer ", "");
        try
        {
            state = await osSystemService.GetLastOsStaticStateAsync(guid, token);
        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message);
            return NotFound(ex.Message);
        }

        return Ok(state);
    }

    [HttpGet]
    [Route("{guid:guid}")]
    [Authorize(Policy = "Viewer")]
    public async Task<IActionResult> GetSystemsOsSystemsLastDynamicStateAsync(Guid guid)
    {
        OsDynamicState? state;
        _ = HttpContext.Request.Headers.TryGetValue("Authorization", out var token);
        token = token.ToString().Replace("Bearer ", "");
        try
        {
            state = await osSystemService.GetLastOsDynamicStateAsync(guid, token);
        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message);
            return NotFound(ex.Message);
        }

        return Ok(state);
    }

    [HttpGet]
    [Route("{guid:guid}")]
    [Authorize(Policy = "Viewer")]
    public async Task<IActionResult> TestSystemsDbSystemConnectionAsync(Guid guid)
    {
        bool result;
        _ = HttpContext.Request.Headers.TryGetValue("Authorization", out var token);
        token = token.ToString().Replace("Bearer ", "");
        try
        {
            result = await dbSystemService.TestDbSystemAsync(guid, token);
        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message);
            return NotFound(ex.Message);
        }

        return Ok(result);
    }

    [HttpGet]
    [Route("{from:datetime}/{to:datetime}")]
    [Authorize(Policy = "Viewer")]
    public async Task<IActionResult> GetSystemsDbSystemsStaticStatesAsync(Guid guid, DateTime from, DateTime to)
    {
        IList<DbStaticState>? states;

        try
        {
            states = await dbSystemService.GetDbStaticStatesFromDbAsync(guid, from, to);
        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message);
            return NotFound(ex.Message);
        }

        return Ok(states);
    }

    [HttpGet]
    [Route("{from:datetime}/{to:datetime}")]
    [Authorize(Policy = "Viewer")]
    public async Task<IActionResult> GetSystemsDbSystemsDynamicStatesAsync(Guid guid, DateTime from, DateTime to)
    {
        IList<DbDynamicState>? states;

        try
        {
            states = await dbSystemService.GetDbDynamicStatesFromDbAsync(guid, from, to);
        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message);
            return NotFound(ex.Message);
        }

        return Ok(states);
    }

    [HttpGet]
    [Route("{guid:guid}")]
    [Authorize(Policy = "Viewer")]
    public async Task<IActionResult> GetSystemsDbSystemsLastStaticStateAsync(Guid guid)
    {
        DbStaticState? state;
        _ = HttpContext.Request.Headers.TryGetValue("Authorization", out var token);
        token = token.ToString().Replace("Bearer ", "");
        try
        {
            state = await dbSystemService.GetLastDbStaticStateAsync(guid, token);
        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message);
            return NotFound(ex.Message);
        }

        return Ok(state);
    }

    [HttpGet]
    [Route("{guid:guid}")]
    [Authorize(Policy = "Viewer")]
    public async Task<IActionResult> GetSystemsDbSystemsLastDynamicStateAsync(Guid guid)
    {
        DbDynamicState? state;
        _ = HttpContext.Request.Headers.TryGetValue("Authorization", out var token);
        token = token.ToString().Replace("Bearer ", "");
        try
        {
            state = await dbSystemService.GetLastDbDynamicStateAsync(guid, token);
        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message);
            return NotFound(ex.Message);
        }

        return Ok(state);
    }


    [HttpGet]
    [Route("{guid:guid}")]
    [Authorize(Policy = "Viewer")]
    public async Task<IActionResult> GetSystemsDbSystemsUsersPermissionAuditResultAsync(Guid guid)
    {
        string? result;
        _ = HttpContext.Request.Headers.TryGetValue("Authorization", out var token);
        token = token.ToString().Replace("Bearer ", "");
        try
        {
            result = await dbSystemService.GetPermissionAuditResultAsync(guid, token);
        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message);
            return NotFound(ex.Message);
        }

        return Ok(result);
    }
}