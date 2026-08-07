using Microsoft.AspNetCore.Mvc;
using Ybt.Core.Entities;
using Ybt.Service.Services;

namespace Ybt.Web.Controllers;

public class ProjectsController : Controller
{
    private readonly IService<Project> _projectService;

    public ProjectsController(IService<Project> projectService)
    {
        _projectService = projectService;
    }

    public async Task<IActionResult> Index()
    {
        var projects = await _projectService.GetAllAsync();
        return View(projects);
    }
}
