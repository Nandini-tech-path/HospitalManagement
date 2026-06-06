using HospitalManagement.Models;
using HospitalManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagement.Controllers;

[Authorize(Roles = RoleNames.Admin)]
public class ReportsController : Controller
{
    private readonly DashboardService _dashboardService;

    public ReportsController(DashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    public async Task<IActionResult> Index()
    {
        var model = await _dashboardService.GetReportsAsync();
        return View(model);
    }
}
