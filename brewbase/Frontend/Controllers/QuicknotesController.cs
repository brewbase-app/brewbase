using Microsoft.AspNetCore.Mvc;

public class QuickNotesController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}