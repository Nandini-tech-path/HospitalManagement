using HospitalManagement.Data;
using HospitalManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HospitalManagement.Controllers;

[Authorize(Roles = RoleNames.Admin)]
public class DoctorsController : Controller
{
    private readonly ApplicationDbContext _context;

    public DoctorsController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(string? search, string? sort, bool sortDesc = false, int page = 1)
    {
        const int pageSize = 10;
        var query = _context.Doctors.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim();
            query = query.Where(d =>
                d.FullName.Contains(search) ||
                d.Specialization.Contains(search) ||
                d.Email.Contains(search));
        }

        query = (sort?.ToLower()) switch
        {
            "name" => sortDesc ? query.OrderByDescending(d => d.FullName) : query.OrderBy(d => d.FullName),
            "specialization" => sortDesc ? query.OrderByDescending(d => d.Specialization) : query.OrderBy(d => d.Specialization),
            "experience" => sortDesc ? query.OrderByDescending(d => d.ExperienceYears) : query.OrderBy(d => d.ExperienceYears),
            _ => query.OrderBy(d => d.FullName)
        };

        var totalCount = await query.CountAsync();
        var doctors = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        ViewBag.Search = search;
        ViewBag.Sort = sort;
        ViewBag.SortDesc = sortDesc;
        ViewBag.Page = page;
        ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        ViewBag.TotalCount = totalCount;

        return View(doctors);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var doctor = await _context.Doctors
            .Include(d => d.Appointments).ThenInclude(a => a.Patient)
            .FirstOrDefaultAsync(d => d.DoctorId == id);

        if (doctor == null) return NotFound();
        return View(doctor);
    }

    public IActionResult Create() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Doctor doctor)
    {
        if (ModelState.IsValid)
        {
            _context.Add(doctor);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Doctor added successfully.";
            return RedirectToAction(nameof(Index));
        }
        return View(doctor);
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var doctor = await _context.Doctors.FindAsync(id);
        if (doctor == null) return NotFound();
        return View(doctor);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Doctor doctor)
    {
        if (id != doctor.DoctorId) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(doctor);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Doctor updated successfully.";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Doctors.AnyAsync(d => d.DoctorId == id))
                    return NotFound();
                throw;
            }
            return RedirectToAction(nameof(Index));
        }
        return View(doctor);
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();
        var doctor = await _context.Doctors.FindAsync(id);
        if (doctor == null) return NotFound();
        return View(doctor);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var doctor = await _context.Doctors.FindAsync(id);
        if (doctor != null)
        {
            _context.Doctors.Remove(doctor);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Doctor deleted successfully.";
        }
        return RedirectToAction(nameof(Index));
    }
}
