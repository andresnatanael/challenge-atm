using System.ComponentModel.DataAnnotations;

namespace AtmChallenge.Domain.Entities
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string card { get; set; }
        public string pin { get; set; }
        public string Role { get; set; }
        public int FailedLoginAttempts { get; set; } = 0;
        public DateTime? LockoutEnd { get; set; }
    }
}