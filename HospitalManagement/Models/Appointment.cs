using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HospitalManagement.Models;

public class Appointment
{
    public int AppointmentId { get; set; }

    [Required]
    public int PatientId { get; set; }
    public Patient Patient { get; set; } = null!;

    [Required]
    public int DoctorId { get; set; }
    public Doctor Doctor { get; set; } = null!;

    [Required, DataType(DataType.Date)]
    [Display(Name = "Appointment Date")]
    public DateTime AppointmentDate { get; set; }

    [Required]
    [Display(Name = "Appointment Time")]
    public TimeSpan AppointmentTime { get; set; }

    [Required, StringLength(20)]
    public string Status { get; set; } = AppointmentStatus.Scheduled;

    public Bill? Bill { get; set; }
}

public static class AppointmentStatus
{
    public const string Scheduled = "Scheduled";
    public const string Completed = "Completed";
    public const string Cancelled = "Cancelled";

    public static readonly string[] All = { Scheduled, Completed, Cancelled };
}
