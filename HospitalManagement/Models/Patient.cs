using System.ComponentModel.DataAnnotations;

namespace HospitalManagement.Models;

public class Patient
{
    public int PatientId { get; set; }

    [Required, StringLength(100)]
    [Display(Name = "Full Name")]
    public string FullName { get; set; } = string.Empty;

    [Required, DataType(DataType.Date)]
    [Display(Name = "Date of Birth")]
    public DateTime DateOfBirth { get; set; }

    [Display(Name = "Age")]
    public int Age => DateTime.Today.Year - DateOfBirth.Year -
        (DateTime.Today.DayOfYear < DateOfBirth.DayOfYear ? 1 : 0);

    [Required, StringLength(10)]
    public string Gender { get; set; } = string.Empty;

    [Required, Phone, StringLength(15)]
    [Display(Name = "Phone Number")]
    public string PhoneNumber { get; set; } = string.Empty;

    [EmailAddress, StringLength(100)]
    public string? Email { get; set; }

    [StringLength(250)]
    public string? Address { get; set; }

    [StringLength(5)]
    [Display(Name = "Blood Group")]
    public string? BloodGroup { get; set; }

    [Display(Name = "Registration Date")]
    public DateTime RegistrationDate { get; set; } = DateTime.Today;

    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public ICollection<Bill> Bills { get; set; } = new List<Bill>();
}
