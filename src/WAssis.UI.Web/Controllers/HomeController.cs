using Microsoft.AspNetCore.Mvc;

namespace WAssis.UI.Web.Controllers;

public class HomeController : Controller
{
    public IActionResult Index() => Content("W.Assis UI Web placeholder (Equinox-style)");
}
