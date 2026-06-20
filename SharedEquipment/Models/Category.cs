using System.ComponentModel.DataAnnotations;

namespace SharedEquipment.Models
{
    public class Category : BaseEntity
    {
        [Required(ErrorMessage = "Name is required")]
        [MaxLength(50)]
        [Display(Name = "Category Name")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Description { get; set; }

        public string? Icon { get; set; }
    }
}
