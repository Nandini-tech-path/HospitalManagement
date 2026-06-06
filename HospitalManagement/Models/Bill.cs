using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HospitalManagement.Models;

public class Bill
{
    public int BillId { get; set; }

    [Required]
    public int PatientId { get; set; }
    public Patient Patient { get; set; } = null!;

    [Required]
    public int AppointmentId { get; set; }
    public Appointment Appointment { get; set; } = null!;

    [Column(TypeName = "decimal(18,2)")]
    [Display(Name = "Consultation Fee")]
    public decimal ConsultationFee { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    [Display(Name = "Medicine Charges")]
    public decimal MedicineCharges { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    [Display(Name = "Other Charges")]
    public decimal OtherCharges { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    [Display(Name = "Total Amount")]
    public decimal TotalAmount => ConsultationFee + MedicineCharges + OtherCharges;

    [Required, StringLength(20)]
    [Display(Name = "Payment Status")]
    public string PaymentStatus { get; set; } = PaymentStatuses.Pending;

    [DataType(DataType.Date)]
    [Display(Name = "Payment Date")]
    public DateTime? PaymentDate { get; set; }
}

public static class PaymentStatuses
{
    public const string Paid = "Paid";
    public const string Pending = "Pending";

    public static readonly string[] All = { Paid, Pending };
}
