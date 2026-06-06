using HospitalManagement.Models;
using HospitalManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagement.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly DashboardService _dashboardService;
    private readonly UserManager<ApplicationUser> _userManager;

    public DashboardController(DashboardService dashboardService, UserManager<ApplicationUser> userManager)
    {
        _dashboardService = dashboardService;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        if (User.IsInRole(RoleNames.Admin))
        {
            var model = await _dashboardService.GetAdminDashboardAsync();
            return View("Admin", model);
        }

        if (User.IsInRole(RoleNames.Doctor))
        {
            var user = await _userManager.GetUserAsync(User);
            if (user?.DoctorId == null)
                return View("Doctor", new Models.ViewModels.DoctorDashboardViewModel());

            var model = await _dashboardService.GetDoctorDashboardAsync(user.DoctorId.Value);
            return View("Doctor", model);
        }

        if (User.IsInRole(RoleNames.Receptionist))
        {
            var model = await _dashboardService.GetReceptionistDashboardAsync();
            return View("Receptionist", model);
        }

        return RedirectToAction("AccessDenied", "Account");
    }
}
