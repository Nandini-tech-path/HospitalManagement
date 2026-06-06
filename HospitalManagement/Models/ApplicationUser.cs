using Microsoft.AspNetCore.Identity;

namespace HospitalManagement.Models;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    public int? DoctorId { get; set; }
    public Doctor? Doctor { get; set; }
}
