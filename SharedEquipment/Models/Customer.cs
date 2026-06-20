using System.ComponentModel.DataAnnotations;

namespace SharedEquipment.Models
{
    public class Customer : BaseEntity
    {
        [Required(ErrorMessage = "Name is required")]
        [Display(Name = "Customer Name")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required")]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Address is required")]
        [MaxLength(200)]
        [Display(Name = "Address")]
        public string Address { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Identity Number is required")]
        [Display(Name = "Identity Number")]
        public string IdentityNumber { get; set; } = string.Empty;

        public DateTime RegistraionDate { get; set; } = DateTime.UtcNow;
    }
}
