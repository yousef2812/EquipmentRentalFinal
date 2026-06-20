using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace SharedEquipment.Models
{
    public class Payment:BaseEntity
    {
        [Required]
        [Column(TypeName = "decimal(10,2)")]
        [Range(0.01,100000)]
        [Display(Name = "Amount")]
        public decimal Amount { get; set; }

        [Required]
        [Display(Name = "Payment Date")]
        public DateTime PaymentDate {  get; set; }

        [Required]
        [Display(Name = "Payment Method")]
        public PaymentMethod PaymentMethod { get; set; }

        [MaxLength(50)]
        [Display(Name = "Reference Number")]
        public string? RefrenceNumber { get; set; }

        [Display(Name = "Receipt URL")]
        public string? ReceiptUrl { get; set; }

        [MaxLength (50)]
        [Display(Name = "Notes")]
        public string? Notes { get; set; }

        [Required]
        [Display(Name = "Payment Status")]
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;

        public int? RentalContractId { get; set; }

        public RentalContract? RentalContract { get; set; }

        [MaxLength(30)]
        [Display(Name = "Contract Number")]
        public string? ContractNumber { get; set; }

        [MaxLength(100)]
        [Display(Name = "Customer Name")]
        public string? CustomerName { get; set; }
    }
}
