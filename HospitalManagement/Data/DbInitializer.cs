using HospitalManagement.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace HospitalManagement.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        var dbProvider = scope.ServiceProvider.GetRequiredService<IConfiguration>()["DatabaseProvider"] ?? "SqlServer";
        if (dbProvider.Equals("Sqlite", StringComparison.OrdinalIgnoreCase))
            await context.Database.EnsureCreatedAsync();
        else
            await context.Database.MigrateAsync();

        foreach (var role in RoleNames.All)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        if (await context.Patients.AnyAsync())
            return;

        await SeedDoctorsAsync(context);
        await SeedUsersAsync(userManager);
        await SeedPatientsAsync(context);
        await SeedAppointmentsAsync(context);
        await SeedBillsAsync(context);
    }

    private static async Task SeedUsersAsync(UserManager<ApplicationUser> userManager)
    {
        var users = new[]
        {
            new { Email = "admin@hospital.com", Password = "Admin@123", FullName = "System Administrator", Role = RoleNames.Admin, DoctorId = (int?)null },
            new { Email = "reception@hospital.com", Password = "Reception@123", FullName = "Sarah Johnson", Role = RoleNames.Receptionist, DoctorId = (int?)null },
            new { Email = "doctor@hospital.com", Password = "Doctor@123", FullName = "Dr. Michael Chen", Role = RoleNames.Doctor, DoctorId = (int?)1 }
        };

        foreach (var u in users)
        {
            if (await userManager.FindByEmailAsync(u.Email) != null)
                continue;

            var user = new ApplicationUser
            {
                UserName = u.Email,
                Email = u.Email,
                EmailConfirmed = true,
                FullName = u.FullName,
                DoctorId = u.DoctorId
            };

            var result = await userManager.CreateAsync(user, u.Password);
            if (result.Succeeded)
                await userManager.AddToRoleAsync(user, u.Role);
        }
    }

    private static async Task SeedDoctorsAsync(ApplicationDbContext context)
    {
        var doctors = new List<Doctor>
        {
            new() { FullName = "Dr. Michael Chen", Specialization = "Cardiology", Email = "doctor@hospital.com", PhoneNumber = "555-0101", ExperienceYears = 15, AvailableDays = "Mon,Wed,Fri" },
            new() { FullName = "Dr. Emily Rodriguez", Specialization = "Pediatrics", Email = "emily.rodriguez@hospital.com", PhoneNumber = "555-0102", ExperienceYears = 10, AvailableDays = "Mon-Fri" },
            new() { FullName = "Dr. James Wilson", Specialization = "Orthopedics", Email = "james.wilson@hospital.com", PhoneNumber = "555-0103", ExperienceYears = 12, AvailableDays = "Tue,Thu,Sat" },
            new() { FullName = "Dr. Lisa Park", Specialization = "Neurology", Email = "lisa.park@hospital.com", PhoneNumber = "555-0104", ExperienceYears = 18, AvailableDays = "Mon,Wed,Fri" },
            new() { FullName = "Dr. Robert Taylor", Specialization = "General Medicine", Email = "robert.taylor@hospital.com", PhoneNumber = "555-0105", ExperienceYears = 8, AvailableDays = "Mon-Fri" }
        };

        context.Doctors.AddRange(doctors);
        await context.SaveChangesAsync();
    }

    private static async Task SeedPatientsAsync(ApplicationDbContext context)
    {
        var patients = new List<Patient>
        {
            new() { FullName = "John Anderson", DateOfBirth = new DateTime(1985, 3, 15), Gender = "Male", PhoneNumber = "555-1001", Email = "john.anderson@email.com", Address = "123 Oak Street, Springfield", BloodGroup = "O+", RegistrationDate = DateTime.Today.AddDays(-30) },
            new() { FullName = "Maria Garcia", DateOfBirth = new DateTime(1990, 7, 22), Gender = "Female", PhoneNumber = "555-1002", Email = "maria.garcia@email.com", Address = "456 Maple Ave, Springfield", BloodGroup = "A+", RegistrationDate = DateTime.Today.AddDays(-25) },
            new() { FullName = "David Kim", DateOfBirth = new DateTime(1978, 11, 8), Gender = "Male", PhoneNumber = "555-1003", Email = "david.kim@email.com", Address = "789 Pine Road, Springfield", BloodGroup = "B+", RegistrationDate = DateTime.Today.AddDays(-20) },
            new() { FullName = "Sarah Thompson", DateOfBirth = new DateTime(1995, 1, 30), Gender = "Female", PhoneNumber = "555-1004", Email = "sarah.thompson@email.com", Address = "321 Elm Street, Springfield", BloodGroup = "AB+", RegistrationDate = DateTime.Today.AddDays(-15) },
            new() { FullName = "Robert Lee", DateOfBirth = new DateTime(1982, 9, 12), Gender = "Male", PhoneNumber = "555-1005", Email = "robert.lee@email.com", Address = "654 Cedar Lane, Springfield", BloodGroup = "O-", RegistrationDate = DateTime.Today.AddDays(-12) },
            new() { FullName = "Jennifer White", DateOfBirth = new DateTime(1988, 4, 5), Gender = "Female", PhoneNumber = "555-1006", Email = "jennifer.white@email.com", Address = "987 Birch Blvd, Springfield", BloodGroup = "A-", RegistrationDate = DateTime.Today.AddDays(-10) },
            new() { FullName = "Michael Brown", DateOfBirth = new DateTime(1975, 12, 20), Gender = "Male", PhoneNumber = "555-1007", Email = "michael.brown@email.com", Address = "147 Willow Way, Springfield", BloodGroup = "B-", RegistrationDate = DateTime.Today.AddDays(-8) },
            new() { FullName = "Emily Davis", DateOfBirth = new DateTime(1992, 6, 18), Gender = "Female", PhoneNumber = "555-1008", Email = "emily.davis@email.com", Address = "258 Spruce St, Springfield", BloodGroup = "O+", RegistrationDate = DateTime.Today.AddDays(-5) },
            new() { FullName = "Christopher Martinez", DateOfBirth = new DateTime(1980, 8, 25), Gender = "Male", PhoneNumber = "555-1009", Email = "chris.martinez@email.com", Address = "369 Ash Court, Springfield", BloodGroup = "A+", RegistrationDate = DateTime.Today.AddDays(-3) },
            new() { FullName = "Amanda Johnson", DateOfBirth = new DateTime(1998, 2, 14), Gender = "Female", PhoneNumber = "555-1010", Email = "amanda.johnson@email.com", Address = "741 Poplar Place, Springfield", BloodGroup = "AB-", RegistrationDate = DateTime.Today.AddDays(-1) }
        };

        context.Patients.AddRange(patients);
        await context.SaveChangesAsync();
    }

    private static async Task SeedAppointmentsAsync(ApplicationDbContext context)
    {
        var random = new Random(42);
        var appointments = new List<Appointment>();
        var statuses = new[] { AppointmentStatus.Scheduled, AppointmentStatus.Completed, AppointmentStatus.Cancelled };

        for (int i = 0; i < 20; i++)
        {
            var daysOffset = random.Next(-10, 15);
            appointments.Add(new Appointment
            {
                PatientId = random.Next(1, 11),
                DoctorId = random.Next(1, 6),
                AppointmentDate = DateTime.Today.AddDays(daysOffset),
                AppointmentTime = new TimeSpan(random.Next(8, 17), random.Next(0, 2) * 30, 0),
                Status = daysOffset < 0 ? (random.Next(2) == 0 ? AppointmentStatus.Completed : AppointmentStatus.Cancelled) : AppointmentStatus.Scheduled
            });
        }

        context.Appointments.AddRange(appointments);
        await context.SaveChangesAsync();
    }

    private static async Task SeedBillsAsync(ApplicationDbContext context)
    {
        var appointmentData = await context.Appointments
            .Where(a => a.Status == AppointmentStatus.Completed)
            .Select(a => new { a.AppointmentId, a.PatientId, a.AppointmentDate })
            .Take(10)
            .ToListAsync();

        var random = new Random(42);
        var bills = new List<Bill>();

        foreach (var appt in appointmentData)
        {
            var isPaid = random.Next(2) == 0;
            bills.Add(new Bill
            {
                PatientId = appt.PatientId,
                AppointmentId = appt.AppointmentId,
                ConsultationFee = random.Next(50, 200),
                MedicineCharges = random.Next(10, 150),
                OtherCharges = random.Next(0, 50),
                PaymentStatus = isPaid ? PaymentStatuses.Paid : PaymentStatuses.Pending,
                PaymentDate = isPaid ? appt.AppointmentDate.AddDays(1) : null
            });
        }

        if (bills.Count < 10)
        {
            var existingBillApptIds = await context.Bills.Select(b => b.AppointmentId).ToListAsync();
            var moreAppointments = await context.Appointments
                .Where(a => !existingBillApptIds.Contains(a.AppointmentId))
                .Select(a => new { a.AppointmentId, a.PatientId })
                .Take(10 - bills.Count)
                .ToListAsync();

            foreach (var appt in moreAppointments)
            {
                bills.Add(new Bill
                {
                    PatientId = appt.PatientId,
                    AppointmentId = appt.AppointmentId,
                    ConsultationFee = random.Next(50, 200),
                    MedicineCharges = random.Next(10, 150),
                    OtherCharges = random.Next(0, 50),
                    PaymentStatus = PaymentStatuses.Pending
                });
            }
        }

        context.Bills.AddRange(bills);
        await context.SaveChangesAsync();
    }
}
