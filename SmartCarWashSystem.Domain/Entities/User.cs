
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace SmartCarWash.Domain.Entities
{
    // Kế thừa IdentityUser giúp có sẵn các trường: Id, Email, PasswordHash, PhoneNumber...
    public class User : IdentityUser<Guid>
    {
        [Column(TypeName = "varchar(255)")]
        public string? FirstName { get; set; }

        [Column(TypeName = "varchar(255)")]
        public string? LastName { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(7);

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow.AddHours(7);
        public DateTime DateOfBirth { get; set; }
    }
}