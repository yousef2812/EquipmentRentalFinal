using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SharedEquipment.Models
{
    public class RentalContract : BaseEntity
    {
        [Required]
        [MaxLength(30)]
        [Display(Name = "Contract Number")]
        public string ContractNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        [Display(Name = "Customer Name")]
        public string Name { get; set; } = string.Empty;

        public int? CustomerId { get; set; }

        public Customer? Customer { get; set; }

        [Required]
        [MaxLength(100)]
        [Display(Name = "Category")]
        public string Category { get; set; } = string.Empty;

        [MaxLength(500)]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        [Display(Name = "Daily Rate")]
        public decimal DailyRate { get; set; }

        [Required]
        [Display(Name = "Rental Start Date")]
        public DateTime StartDate { get; set; }

        [Required]
        [Display(Name = "Rental End Date")]
        public DateTime EndDate { get; set; }

        [Display(Name = "Actual Return Date")]
        public DateTime? ActualReturnDate { get; set; }

        [Column(TypeName = "decimal(12,2)")]
        [Display(Name = "Total Amount")]
        public decimal TotalAmount { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        [Display(Name = "Deposit Amount")]
        public decimal DepositAmount { get; set; }

        [Display(Name = "Contract Status")]
        public RentalStatus Status { get; set; } = RentalStatus.Active;

        [Display(Name = "Is Available")]
        public bool IsAvailable { get; set; } = true;

        [Display(Name = "Image URL")]
        public string? ImageUrl { get; set; }

        public ICollection<RentalDetail> Details { get; set; } = new List<RentalDetail>();

        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}
