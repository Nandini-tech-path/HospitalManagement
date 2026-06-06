using HospitalManagement.Data;
using HospitalManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace HospitalManagement.Controllers;

[Authorize]
public class AppointmentsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public AppointmentsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index(string? search, string? sort, bool sortDesc = false, int page = 1)
    {
        const int pageSize = 10;
        var query = _context.Appointments
            .Include(a => a.Patient)
            .Include(a => a.Doctor)
            .AsQueryable();

        if (User.IsInRole(RoleNames.Doctor))
        {
            var user = await _userManager.GetUserAsync(User);
            if (user?.DoctorId != null)
                query = query.Where(a => a.DoctorId == user.DoctorId);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim();
            query = query.Where(a =>
                a.Patient.FullName.Contains(search) ||
                a.Doctor.FullName.Contains(search) ||
                a.Status.Contains(search));
        }

        query = (sort?.ToLower()) switch
        {
            "date" => sortDesc ? query.OrderByDescending(a => a.AppointmentDate) : query.OrderBy(a => a.AppointmentDate),
            "patient" => sortDesc ? query.OrderByDescending(a => a.Patient.FullName) : query.OrderBy(a => a.Patient.FullName),
            "doctor" => sortDesc ? query.OrderByDescending(a => a.Doctor.FullName) : query.OrderBy(a => a.Doctor.FullName),
            "status" => sortDesc ? query.OrderByDescending(a => a.Status) : query.OrderBy(a => a.Status),
            _ => query.OrderByDescending(a => a.AppointmentDate)
        };

        var totalCount = await query.CountAsync();
        var appointments = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        ViewBag.Search = search;
        ViewBag.Sort = sort;
        ViewBag.SortDesc = sortDesc;
        ViewBag.Page = page;
        ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        ViewBag.TotalCount = totalCount;

        return View(appointments);
    }

    [Authorize(Roles = $"{RoleNames.Admin},{RoleNames.Receptionist}")]
    public async Task<IActionResult> Create()
    {
        await PopulateDropdowns();
        return View(new Appointment { AppointmentDate = DateTime.Today, AppointmentTime = new TimeSpan(9, 0, 0) });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = $"{RoleNames.Admin},{RoleNames.Receptionist}")]
    public async Task<IActionResult> Create(Appointment appointment)
    {
        if (ModelState.IsValid)
        {
            appointment.Status = AppointmentStatus.Scheduled;
            _context.Add(appointment);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Appointment scheduled successfully.";
            return RedirectToAction(nameof(Index));
        }
        await PopulateDropdowns();
        return View(appointment);
    }

    [Authorize(Roles = $"{RoleNames.Admin},{RoleNames.Receptionist}")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var appointment = await _context.Appointments.FindAsync(id);
        if (appointment == null) return NotFound();
        await PopulateDropdowns();
        return View(appointment);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = $"{RoleNames.Admin},{RoleNames.Receptionist}")]
    public async Task<IActionResult> Edit(int id, Appointment appointment)
    {
        if (id != appointment.AppointmentId) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(appointment);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Appointment updated successfully.";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Appointments.AnyAsync(a => a.AppointmentId == id))
                    return NotFound();
                throw;
            }
            return RedirectToAction(nameof(Index));
        }
        await PopulateDropdowns();
        return View(appointment);
    }

    [Authorize(Roles = $"{RoleNames.Admin},{RoleNames.Receptionist},{RoleNames.Doctor}")]
    public async Task<IActionResult> Cancel(int? id)
    {
        if (id == null) return NotFound();
        var appointment = await _context.Appointments
            .Include(a => a.Patient)
            .Include(a => a.Doctor)
            .FirstOrDefaultAsync(a => a.AppointmentId == id);
        if (appointment == null) return NotFound();
        return View(appointment);
    }

    [HttpPost, ActionName("Cancel")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = $"{RoleNames.Admin},{RoleNames.Receptionist},{RoleNames.Doctor}")]
    public async Task<IActionResult> CancelConfirmed(int id)
    {
        var appointment = await _context.Appointments.FindAsync(id);
        if (appointment != null)
        {
            appointment.Status = AppointmentStatus.Cancelled;
            await _context.SaveChangesAsync();
            TempData["Success"] = "Appointment cancelled.";
        }
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = $"{RoleNames.Admin},{RoleNames.Receptionist},{RoleNames.Doctor}")]
    public async Task<IActionResult> Complete(int? id)
    {
        if (id == null) return NotFound();
        var appointment = await _context.Appointments
            .Include(a => a.Patient)
            .Include(a => a.Doctor)
            .FirstOrDefaultAsync(a => a.AppointmentId == id);
        if (appointment == null) return NotFound();
        return View(appointment);
    }

    [HttpPost, ActionName("Complete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = $"{RoleNames.Admin},{RoleNames.Receptionist},{RoleNames.Doctor}")]
    public async Task<IActionResult> CompleteConfirmed(int id)
    {
        var appointment = await _context.Appointments.FindAsync(id);
        if (appointment != null)
        {
            appointment.Status = AppointmentStatus.Completed;
            await _context.SaveChangesAsync();
            TempData["Success"] = "Appointment marked as completed.";
        }
        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateDropdowns()
    {
        ViewBag.PatientId = new SelectList(await _context.Patients.OrderBy(p => p.FullName).ToListAsync(), "PatientId", "FullName");
        ViewBag.DoctorId = new SelectList(await _context.Doctors.OrderBy(d => d.FullName).ToListAsync(), "DoctorId", "FullName");
        ViewBag.StatusList = new SelectList(AppointmentStatus.All);
    }
}
