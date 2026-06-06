using HospitalManagement.Data;
using HospitalManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace HospitalManagement.Controllers;

[Authorize(Roles = $"{RoleNames.Admin},{RoleNames.Receptionist}")]
public class BillsController : Controller
{
    private readonly ApplicationDbContext _context;

    public BillsController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(string? search, string? sort, bool sortDesc = false, int page = 1)
    {
        const int pageSize = 10;
        var query = _context.Bills
            .Include(b => b.Patient)
            .Include(b => b.Appointment).ThenInclude(a => a.Doctor)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim();
            query = query.Where(b =>
                b.Patient.FullName.Contains(search) ||
                b.PaymentStatus.Contains(search));
        }

        query = (sort?.ToLower()) switch
        {
            "patient" => sortDesc ? query.OrderByDescending(b => b.Patient.FullName) : query.OrderBy(b => b.Patient.FullName),
            "amount" => sortDesc ? query.OrderByDescending(b => b.ConsultationFee + b.MedicineCharges + b.OtherCharges) : query.OrderBy(b => b.ConsultationFee + b.MedicineCharges + b.OtherCharges),
            "status" => sortDesc ? query.OrderByDescending(b => b.PaymentStatus) : query.OrderBy(b => b.PaymentStatus),
            _ => query.OrderByDescending(b => b.BillId)
        };

        var totalCount = await query.CountAsync();
        var bills = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        ViewBag.Search = search;
        ViewBag.Sort = sort;
        ViewBag.SortDesc = sortDesc;
        ViewBag.Page = page;
        ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        ViewBag.TotalCount = totalCount;

        return View(bills);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var bill = await _context.Bills
            .Include(b => b.Patient)
            .Include(b => b.Appointment).ThenInclude(a => a.Doctor)
            .FirstOrDefaultAsync(b => b.BillId == id);

        if (bill == null) return NotFound();
        return View(bill);
    }

    public async Task<IActionResult> Create()
    {
        await PopulateDropdowns();
        return View(new Bill());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Bill bill)
    {
        if (await _context.Bills.AnyAsync(b => b.AppointmentId == bill.AppointmentId))
            ModelState.AddModelError("AppointmentId", "A bill already exists for this appointment.");

        if (ModelState.IsValid)
        {
            bill.PaymentStatus = PaymentStatuses.Pending;
            _context.Add(bill);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Bill generated successfully.";
            return RedirectToAction(nameof(Index));
        }
        await PopulateDropdowns();
        return View(bill);
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var bill = await _context.Bills.FindAsync(id);
        if (bill == null) return NotFound();
        await PopulateDropdowns();
        return View(bill);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Bill bill)
    {
        if (id != bill.BillId) return NotFound();

        if (ModelState.IsValid)
        {
            if (bill.PaymentStatus == PaymentStatuses.Paid && bill.PaymentDate == null)
                bill.PaymentDate = DateTime.Today;

            try
            {
                _context.Update(bill);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Bill updated successfully.";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Bills.AnyAsync(b => b.BillId == id))
                    return NotFound();
                throw;
            }
            return RedirectToAction(nameof(Index));
        }
        await PopulateDropdowns();
        return View(bill);
    }

    public async Task<IActionResult> Print(int? id)
    {
        if (id == null) return NotFound();

        var bill = await _context.Bills
            .Include(b => b.Patient)
            .Include(b => b.Appointment).ThenInclude(a => a.Doctor)
            .FirstOrDefaultAsync(b => b.BillId == id);

        if (bill == null) return NotFound();
        return View(bill);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkPaid(int id)
    {
        var bill = await _context.Bills.FindAsync(id);
        if (bill != null)
        {
            bill.PaymentStatus = PaymentStatuses.Paid;
            bill.PaymentDate = DateTime.Today;
            await _context.SaveChangesAsync();
            TempData["Success"] = "Payment recorded successfully.";
        }
        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateDropdowns()
    {
        var appointmentsWithoutBills = await _context.Appointments
            .Include(a => a.Patient)
            .Include(a => a.Doctor)
            .Where(a => ! _context.Bills.Any(b => b.AppointmentId == a.AppointmentId))
            .OrderByDescending(a => a.AppointmentDate)
            .ToListAsync();

        ViewBag.AppointmentId = new SelectList(
            appointmentsWithoutBills.Select(a => new
            {
                a.AppointmentId,
                Display = $"{a.Patient.FullName} - {a.Doctor.FullName} ({a.AppointmentDate:MMM dd, yyyy})"
            }),
            "AppointmentId", "Display");

        ViewBag.PatientId = new SelectList(await _context.Patients.OrderBy(p => p.FullName).ToListAsync(), "PatientId", "FullName");
        ViewBag.PaymentStatusList = new SelectList(PaymentStatuses.All);
    }
}
