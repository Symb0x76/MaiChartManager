using System.Globalization;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace MaiChartManager.Controllers.App;

[ApiController]
[Route("MaiChartManagerServlet/[action]Api")]
public class LocaleController(StaticSettings settings, ILogger<LocaleController> logger) : ControllerBase
{
    [HttpGet]
    public string GetCurrentLocale()
    {
        return StaticSettings.CurrentLocale;
    }

    [HttpPost]
    public void SetLocale([FromBody] string locale)
    {
        AppMain.SetLocale(locale);
    }
}