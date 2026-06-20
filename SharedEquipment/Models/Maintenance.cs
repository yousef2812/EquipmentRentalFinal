using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SharedEquipment.Models
{
    public class Maintenance : BaseEntity
    {
        public int? EquipmentId { get; set; }

        public Equipment? Equipment { get; set; }

        [Required]
        [MaxLength(100)]
        [Display(Name = "Equipment Name")]
        public string EquipmentName { get; set; } = string.Empty;

        public DateTime MaintenanceDate { get; set; }

        [Required]
        [MaxLength(30)]
        [Display(Name = "Maintenance Type")]
        public string MaintenanceType { get; set; } = string.Empty;

        [Column(TypeName = "decimal(10,2)")]
        [Display(Name = "Cost")]
        public decimal Cost { get; set; }

        [MaxLength(500)]
        [Display(Name = "Description")]
        public string Description { get; set; } = string.Empty;

        [Display(Name = "Next Maintenance Date")]
        public DateTime NextMaintenanceDate { get; set; }

        [MaxLength(100)]
        [Display(Name = "Performed By")]
        public string? PerformedBy { get; set; }
    }
}
