using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SharedEquipment.Models
{
    public class RentalDetail : BaseEntity
    {
        [Required]
        [Range(1, 100)]
        [Display(Name = "Quantity")]
        public int Quantity { get; set; } = 1;

        public int? EquipmentId { get; set; }

        public Equipment? Equipment { get; set; }

        [Required]
        [MaxLength(100)]
        [Display(Name = "Equipment Name")]
        public string EquipmentName { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        [Display(Name = "Rate at Time of Rental")]
        public decimal RateAtTime { get; set; }

        [Required]
        [Column(TypeName = "decimal(12,2)")]
        [Display(Name = "Subtotal")]
        public decimal SubTotal { get; set; }

        [Display(Name = "Is Returned")]
        public bool IsReturned { get; set; }

        [Display(Name = "Return Date")]
        public DateTime? ReturnDate { get; set; }

        [Display(Name = "Return Condition")]
        public EquipmentCondition? ReturnCondition { get; set; }

        [MaxLength(500)]
        [Display(Name = "Damage Notes")]
        public string? DamageNotes { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        [Display(Name = "Damage Fee")]
        public decimal DamageFee { get; set; }

        public int RentalContractId { get; set; }

        public RentalContract RentalContract { get; set; } = null!;
    }
}
