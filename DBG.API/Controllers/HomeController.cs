#region

using DBG.Infrastructure.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

#endregion

namespace DBG.API.Controllers;

[ApiController]
public class HomeController
    : Controller
{
    private readonly ILinuxClient linuxClient;
    private readonly IMssqlClient mssqlClient;
    private readonly IPostgresClient postgresClient;

    public HomeController(ILinuxClient linuxClient, IMssqlClient mssqlClient, IPostgresClient postgresClient)
    {
        this.linuxClient = linuxClient;
        this.mssqlClient = mssqlClient;
        this.postgresClient = postgresClient;
    }

    [HttpGet]
    [Route("[action]")]
    [Authorize(Policy = "Viewer")]
    public async Task<IActionResult> CheckWorkersConnectivityAsync()
    {
        return Ok(new
        {
            SSHWorker = await linuxClient.PingAsync(),
            MSSQLWorker = await mssqlClient.PingAsync(),
            PostgresWorker = await postgresClient.PingAsync()
        });
    }

    [HttpGet]
    [Route("/")]
    public string Test()
    {
        return "It works!";
    }
}