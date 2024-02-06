#region

using DBG.Infrastructure.Interfaces;
using DBG.Infrastructure.Models.Helper;
using Microsoft.AspNetCore.Mvc;

#endregion

namespace DBG.API.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class AuthController : Controller
{
    private readonly IAuthService authService;
    private readonly ILogger<AuthController> logger;

    public AuthController(ILogger<AuthController> logger, IAuthService authService)
    {
        this.logger = logger;
        this.authService = authService;
    }

    [HttpPost]
    public async Task<IActionResult> LoginAsync([FromBody] Login login)
    {
        AuthenticatedResponse? authenticatedResponse;
        try
        {
            authenticatedResponse = await authService.AuthenticateAsync(login);
        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message);
            return Unauthorized(ex.Message);
        }

        return Ok(authenticatedResponse);
    }

    [HttpPost]
    public async Task<IActionResult> RefreshAsync([FromBody] AuthenticatedResponse authenticatedResponse)
    {
        AuthenticatedResponse? response;
        try
        {
            response = await authService.RefreshAsync(authenticatedResponse);
        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message);
            return Unauthorized(ex.Message);
        }

        return Ok(response);
    }
}