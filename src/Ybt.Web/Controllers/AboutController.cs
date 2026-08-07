using Microsoft.AspNetCore.Mvc;
using Ybt.Core.Entities;
using Ybt.Service.Services;

namespace Ybt.Web.Controllers;

public class AboutController : Controller
{
    private readonly IService<TeamMember> _teamMemberService;

    public AboutController(IService<TeamMember> teamMemberService)
    {
        _teamMemberService = teamMemberService;
    }

    public async Task<IActionResult> Index()
    {
        var teamMembers = await _teamMemberService.GetAllAsync();
        var activeMembers = teamMembers.Where(m => m.IsActive).OrderBy(m => m.Order).ToList();
        return View(activeMembers);
    }
}
