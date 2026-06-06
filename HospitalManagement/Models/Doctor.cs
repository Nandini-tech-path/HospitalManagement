using System.ComponentModel.DataAnnotations;

namespace HospitalManagement.Models;

public class Doctor
{
    public int DoctorId { get; set; }

    [Required, StringLength(100)]
    [Display(Name = "Full Name")]
    public string FullName { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string Specialization { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(100)]
    public string Email { get; set; } = string.Empty;

    [Required, Phone, StringLength(15)]
    [Display(Name = "Phone Number")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Range(0, 50)]
    [Display(Name = "Experience (Years)")]
    public int ExperienceYears { get; set; }

    [StringLength(100)]
    [Display(Name = "Available Days")]
    public string AvailableDays { get; set; } = "Mon-Fri";

    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
