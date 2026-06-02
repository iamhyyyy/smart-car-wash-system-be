
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace SmartCarWash.Domain.Entities
{
    public class User : IdentityUser<Guid>
    {
        [Column(TypeName = "varchar(255)")]
        public string? FirstName { get; set; }
        [Column(TypeName = "varchar(255)")]
        public string? LastName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(7);
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow.AddHours(7);
        public DateTime? LastLoginAt { get; set; }
        public bool IsActive { get; set; } = true;

        public CustomerProfile? CustomerProfile { get; set; }
        public ICollection<Promotion> CreatedPromotions { get; set; } = new List<Promotion>();
    }
}