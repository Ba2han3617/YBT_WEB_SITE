using Microsoft.AspNetCore.Mvc;
using Ybt.Core.Entities;
using Ybt.Service.Services;

namespace Ybt.Web.Controllers;

public class EventsController : Controller
{
    private readonly IService<Event> _eventService;

    public EventsController(IService<Event> eventService)
    {
        _eventService = eventService;
    }

    public async Task<IActionResult> Index()
    {
        var events = await _eventService.GetAllAsync();
        return View(events);
    }

    public async Task<IActionResult> Details(string slug)
    {
        // For simplicity, finding by slug manually in this basic service
        var events = await _eventService.GetAllAsync();
        var @event = events.FirstOrDefault(e => e.Slug == slug);
        if (@event == null) return NotFound();
        return View(@event);
    }
}
