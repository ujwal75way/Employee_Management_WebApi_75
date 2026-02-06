using System.ComponentModel.DataAnnotations;

namespace EmployeeManagement.Api.Domain.Entities
{
    public class Employee : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Department { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        public bool IsDeleted { get; set; } = false;
        
        public bool IsActive { get; set; }
    }
}