using HospitalManagement.Data;
using HospitalManagement.Models;
using HospitalManagement.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace HospitalManagement.Services;

public class DashboardService
{
    private readonly ApplicationDbContext _context;

    public DashboardService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AdminDashboardViewModel> GetAdminDashboardAsync()
    {
        var today = DateTime.Today;
        var bills = await _context.Bills.ToListAsync();
        var appointments = await _context.Appointments.ToListAsync();
        var last7Days = Enumerable.Range(0, 7).Select(i => today.AddDays(-6 + i)).ToList();

        return new AdminDashboardViewModel
        {
            TotalPatients = await _context.Patients.CountAsync(),
            TotalDoctors = await _context.Doctors.CountAsync(),
            TotalAppointments = await _context.Appointments.CountAsync(),
            TodayAppointments = await _context.Appointments.CountAsync(a => a.AppointmentDate == today),
            TotalRevenue = bills.Where(b => b.PaymentStatus == PaymentStatuses.Paid).Sum(b => b.TotalAmount),
            PendingRevenue = bills.Where(b => b.PaymentStatus == PaymentStatuses.Pending).Sum(b => b.TotalAmount),
            RecentPatients = (await _context.Patients
                .OrderByDescending(p => p.RegistrationDate)
                .Take(5)
                .ToListAsync())
                .Select(p => new PatientSummary
                {
                    PatientId = p.PatientId,
                    FullName = p.FullName,
                    Gender = p.Gender,
                    Age = p.Age,
                    RegistrationDate = p.RegistrationDate,
                    BloodGroup = p.BloodGroup
                }).ToList(),
            RecentAppointments = (await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .ToListAsync())
                .OrderByDescending(a => a.AppointmentDate)
                .ThenByDescending(a => a.AppointmentTime)
                .Take(5)
                .Select(a => new AppointmentSummary
                {
                    AppointmentId = a.AppointmentId,
                    PatientName = a.Patient.FullName,
                    DoctorName = a.Doctor.FullName,
                    AppointmentDate = a.AppointmentDate,
                    AppointmentTime = a.AppointmentTime,
                    Status = a.Status
                }).ToList(),
            RevenueLabels = last7Days.Select(d => d.ToString("MMM dd")).ToList(),
            RevenueData = last7Days.Select(d =>
                bills.Where(b => b.PaymentStatus == PaymentStatuses.Paid && b.PaymentDate == d)
                    .Sum(b => b.TotalAmount)).ToList(),
            AppointmentLabels = last7Days.Select(d => d.ToString("MMM dd")).ToList(),
            AppointmentData = last7Days.Select(d =>
                appointments.Count(a => a.AppointmentDate == d)).ToList()
        };
    }

    public async Task<DoctorDashboardViewModel> GetDoctorDashboardAsync(int doctorId)
    {
        var today = DateTime.Today;

        var todaySchedule = (await _context.Appointments
            .Include(a => a.Patient)
            .Include(a => a.Doctor)
            .Where(a => a.DoctorId == doctorId && a.AppointmentDate == today && a.Status != AppointmentStatus.Cancelled)
            .ToListAsync())
            .OrderBy(a => a.AppointmentTime)
            .Select(a => new AppointmentSummary
            {
                AppointmentId = a.AppointmentId,
                PatientName = a.Patient.FullName,
                DoctorName = a.Doctor.FullName,
                AppointmentDate = a.AppointmentDate,
                AppointmentTime = a.AppointmentTime,
                Status = a.Status
            }).ToList();

        var upcoming = (await _context.Appointments
            .Include(a => a.Patient)
            .Include(a => a.Doctor)
            .Where(a => a.DoctorId == doctorId && a.AppointmentDate > today && a.Status == AppointmentStatus.Scheduled)
            .ToListAsync())
            .OrderBy(a => a.AppointmentDate)
            .ThenBy(a => a.AppointmentTime)
            .Take(10)
            .Select(a => new AppointmentSummary
            {
                AppointmentId = a.AppointmentId,
                PatientName = a.Patient.FullName,
                DoctorName = a.Doctor.FullName,
                AppointmentDate = a.AppointmentDate,
                AppointmentTime = a.AppointmentTime,
                Status = a.Status
            }).ToList();

        var patientIds = await _context.Appointments
            .Where(a => a.DoctorId == doctorId)
            .Select(a => a.PatientId)
            .Distinct()
            .Take(10)
            .ToListAsync();

        var patients = await _context.Patients
            .Where(p => patientIds.Contains(p.PatientId))
            .ToListAsync();

        var assignedPatients = patients.Select(p => new PatientSummary
        {
            PatientId = p.PatientId,
            FullName = p.FullName,
            Gender = p.Gender,
            Age = p.Age,
            RegistrationDate = p.RegistrationDate,
            BloodGroup = p.BloodGroup
        }).ToList();

        return new DoctorDashboardViewModel
        {
            TodayAppointments = todaySchedule.Count,
            UpcomingAppointments = upcoming.Count,
            TodaySchedule = todaySchedule,
            UpcomingSchedule = upcoming,
            AssignedPatients = assignedPatients
        };
    }

    public async Task<ReceptionistDashboardViewModel> GetReceptionistDashboardAsync()
    {
        var today = DateTime.Today;
        var weekAgo = today.AddDays(-7);
        var bills = await _context.Bills.ToListAsync();

        return new ReceptionistDashboardViewModel
        {
            NewRegistrations = await _context.Patients.CountAsync(p => p.RegistrationDate >= weekAgo),
            TodayAppointments = await _context.Appointments.CountAsync(a => a.AppointmentDate == today),
            TotalBilled = bills.Sum(b => b.TotalAmount),
            PendingPayments = bills.Where(b => b.PaymentStatus == PaymentStatuses.Pending).Sum(b => b.TotalAmount),
            PaidBills = bills.Count(b => b.PaymentStatus == PaymentStatuses.Paid),
            PendingBills = bills.Count(b => b.PaymentStatus == PaymentStatuses.Pending),
            RecentPatients = (await _context.Patients
                .OrderByDescending(p => p.RegistrationDate)
                .Take(5)
                .ToListAsync())
                .Select(p => new PatientSummary
                {
                    PatientId = p.PatientId,
                    FullName = p.FullName,
                    Gender = p.Gender,
                    Age = p.Age,
                    RegistrationDate = p.RegistrationDate,
                    BloodGroup = p.BloodGroup
                }).ToList(),
            TodaySchedule = (await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .Where(a => a.AppointmentDate == today)
                .ToListAsync())
                .OrderBy(a => a.AppointmentTime)
                .Select(a => new AppointmentSummary
                {
                    AppointmentId = a.AppointmentId,
                    PatientName = a.Patient.FullName,
                    DoctorName = a.Doctor.FullName,
                    AppointmentDate = a.AppointmentDate,
                    AppointmentTime = a.AppointmentTime,
                    Status = a.Status
                }).ToList()
        };
    }

    public async Task<ReportsViewModel> GetReportsAsync()
    {
        var today = DateTime.Today;
        var weekAgo = today.AddDays(-7);
        var bills = await _context.Bills.Include(b => b.Appointment).ToListAsync();
        var appointments = await _context.Appointments.ToListAsync();

        var last7Days = Enumerable.Range(0, 7).Select(i => today.AddDays(-6 + i)).ToList();
        var last6Months = Enumerable.Range(0, 6).Select(i => today.AddMonths(-5 + i)).ToList();

        var doctorDist = await _context.Doctors
            .GroupBy(d => d.Specialization)
            .Select(g => new { Specialization = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Specialization, x => x.Count);

        return new ReportsViewModel
        {
            TotalPatients = await _context.Patients.CountAsync(),
            RecentRegistrations = await _context.Patients.CountAsync(p => p.RegistrationDate >= weekAgo),
            TotalDoctors = await _context.Doctors.CountAsync(),
            DoctorDistribution = doctorDist,
            DailyAppointmentLabels = last7Days.Select(d => d.ToString("MMM dd")).ToList(),
            DailyAppointmentData = last7Days.Select(d => appointments.Count(a => a.AppointmentDate == d)).ToList(),
            MonthlyAppointmentLabels = last6Months.Select(d => d.ToString("MMM yyyy")).ToList(),
            MonthlyAppointmentData = last6Months.Select(d =>
                appointments.Count(a => a.AppointmentDate.Month == d.Month && a.AppointmentDate.Year == d.Year)).ToList(),
            DailyRevenueLabels = last7Days.Select(d => d.ToString("MMM dd")).ToList(),
            DailyRevenueData = last7Days.Select(d =>
                bills.Where(b => b.PaymentStatus == PaymentStatuses.Paid && b.PaymentDate == d)
                    .Sum(b => b.TotalAmount)).ToList(),
            MonthlyRevenueLabels = last6Months.Select(d => d.ToString("MMM yyyy")).ToList(),
            MonthlyRevenueData = last6Months.Select(d =>
                bills.Where(b => b.PaymentStatus == PaymentStatuses.Paid &&
                    b.PaymentDate.HasValue && b.PaymentDate.Value.Month == d.Month && b.PaymentDate.Value.Year == d.Year)
                    .Sum(b => b.TotalAmount)).ToList()
        };
    }
}
