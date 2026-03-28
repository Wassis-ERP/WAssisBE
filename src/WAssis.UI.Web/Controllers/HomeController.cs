using Microsoft.AspNetCore.Mvc;

namespace WAssis.UI.Web.Controllers;

public class HomeController : Controller
{
    public IActionResult Index() => View();
}
