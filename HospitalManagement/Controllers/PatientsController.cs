using HospitalManagement.Data;
using HospitalManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HospitalManagement.Controllers;

[Authorize(Roles = $"{RoleNames.Admin},{RoleNames.Receptionist}")]
public class PatientsController : Controller
{
    private readonly ApplicationDbContext _context;

    public PatientsController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(string? search, string? sort, bool sortDesc = false, int page = 1)
    {
        const int pageSize = 10;
        var query = _context.Patients.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim();
            query = query.Where(p =>
                p.FullName.Contains(search) ||
                (p.Email != null && p.Email.Contains(search)) ||
                p.PhoneNumber.Contains(search));
        }

        query = (sort?.ToLower()) switch
        {
            "name" => sortDesc ? query.OrderByDescending(p => p.FullName) : query.OrderBy(p => p.FullName),
            "date" => sortDesc ? query.OrderByDescending(p => p.RegistrationDate) : query.OrderBy(p => p.RegistrationDate),
            "gender" => sortDesc ? query.OrderByDescending(p => p.Gender) : query.OrderBy(p => p.Gender),
            _ => query.OrderByDescending(p => p.RegistrationDate)
        };

        var totalCount = await query.CountAsync();
        var patients = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        ViewBag.Search = search;
        ViewBag.Sort = sort;
        ViewBag.SortDesc = sortDesc;
        ViewBag.Page = page;
        ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        ViewBag.TotalCount = totalCount;

        return View(patients);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var patient = await _context.Patients
            .Include(p => p.Appointments).ThenInclude(a => a.Doctor)
            .Include(p => p.Bills)
            .FirstOrDefaultAsync(p => p.PatientId == id);

        if (patient == null) return NotFound();
        return View(patient);
    }

    public IActionResult Create() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Patient patient)
    {
        if (ModelState.IsValid)
        {
            patient.RegistrationDate = DateTime.Today;
            _context.Add(patient);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Patient registered successfully.";
            return RedirectToAction(nameof(Index));
        }
        return View(patient);
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var patient = await _context.Patients.FindAsync(id);
        if (patient == null) return NotFound();
        return View(patient);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Patient patient)
    {
        if (id != patient.PatientId) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(patient);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Patient updated successfully.";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Patients.AnyAsync(p => p.PatientId == id))
                    return NotFound();
                throw;
            }
            return RedirectToAction(nameof(Index));
        }
        return View(patient);
    }

    [Authorize(Roles = RoleNames.Admin)]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();
        var patient = await _context.Patients.FindAsync(id);
        if (patient == null) return NotFound();
        return View(patient);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var patient = await _context.Patients.FindAsync(id);
        if (patient != null)
        {
            _context.Patients.Remove(patient);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Patient deleted successfully.";
        }
        return RedirectToAction(nameof(Index));
    }
}
