#region

using Microsoft.AspNetCore.Mvc;

#endregion

namespace DBG.PostgresWorker.Controllers;

[ApiController]
public class HomeController : Controller
{
    [HttpGet]
    [Route("/")]
    public string Index()
    {
        return "It works!";
    }
}