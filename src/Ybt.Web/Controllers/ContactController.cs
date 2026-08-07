using Microsoft.AspNetCore.Mvc;

namespace Ybt.Web.Controllers;

public class ContactController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    public IActionResult SendMessage()
    {
        // For demonstration, just redirect back with a success message
        TempData["Message"] = "Mesajınız başarıyla gönderildi!";
        return RedirectToAction("Index");
    }
}
