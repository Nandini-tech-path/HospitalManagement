namespace HospitalManagement.Models.ViewModels;

public class AdminDashboardViewModel
{
    public int TotalPatients { get; set; }
    public int TotalDoctors { get; set; }
    public int TotalAppointments { get; set; }
    public int TodayAppointments { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal PendingRevenue { get; set; }
    public List<PatientSummary> RecentPatients { get; set; } = new();
    public List<AppointmentSummary> RecentAppointments { get; set; } = new();
    public List<string> RevenueLabels { get; set; } = new();
    public List<decimal> RevenueData { get; set; } = new();
    public List<string> AppointmentLabels { get; set; } = new();
    public List<int> AppointmentData { get; set; } = new();
}

public class DoctorDashboardViewModel
{
    public int TodayAppointments { get; set; }
    public int UpcomingAppointments { get; set; }
    public List<AppointmentSummary> TodaySchedule { get; set; } = new();
    public List<AppointmentSummary> UpcomingSchedule { get; set; } = new();
    public List<PatientSummary> AssignedPatients { get; set; } = new();
}

public class ReceptionistDashboardViewModel
{
    public int NewRegistrations { get; set; }
    public int TodayAppointments { get; set; }
    public decimal TotalBilled { get; set; }
    public decimal PendingPayments { get; set; }
    public int PaidBills { get; set; }
    public int PendingBills { get; set; }
    public List<PatientSummary> RecentPatients { get; set; } = new();
    public List<AppointmentSummary> TodaySchedule { get; set; } = new();
}

public class PatientSummary
{
    public int PatientId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Gender { get; set; } = string.Empty;
    public int Age { get; set; }
    public DateTime RegistrationDate { get; set; }
    public string? BloodGroup { get; set; }
}

public class AppointmentSummary
{
    public int AppointmentId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public string DoctorName { get; set; } = string.Empty;
    public DateTime AppointmentDate { get; set; }
    public TimeSpan AppointmentTime { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class ReportsViewModel
{
    public int TotalPatients { get; set; }
    public int RecentRegistrations { get; set; }
    public int TotalDoctors { get; set; }
    public Dictionary<string, int> DoctorDistribution { get; set; } = new();
    public List<string> DailyAppointmentLabels { get; set; } = new();
    public List<int> DailyAppointmentData { get; set; } = new();
    public List<string> MonthlyAppointmentLabels { get; set; } = new();
    public List<int> MonthlyAppointmentData { get; set; } = new();
    public List<string> DailyRevenueLabels { get; set; } = new();
    public List<decimal> DailyRevenueData { get; set; } = new();
    public List<string> MonthlyRevenueLabels { get; set; } = new();
    public List<decimal> MonthlyRevenueData { get; set; } = new();
}

public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public string? Search { get; set; }
    public string? Sort { get; set; }
    public bool SortDesc { get; set; }
}
