using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SharedEquipment.Models
{
    public class Equipment : BaseEntity
    {
        [Required(ErrorMessage = "Name is required")]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? SerialNumber { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public DateTime ActualReturnDate { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal DailyRentalRate { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalCost { get; set; }

        public EquipmentCondition Status { get; set; }

        public string Notes { get; set; } = string.Empty;

        public bool IsRented { get; set; }

        public int? CurrentRentalContractId { get; set; }

        [ForeignKey(nameof(Customer))]
        public int? CustomerId { get; set; }

        public Customer? Customer { get; set; }

        [ForeignKey(nameof(EquipmentCategory))]
        public int? EquipmentCategoryId { get; set; }

        public Category? EquipmentCategory { get; set; }
    }
}
