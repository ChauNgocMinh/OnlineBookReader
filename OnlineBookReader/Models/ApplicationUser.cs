using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace OnlineBookReader.Models
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        public string FullName { get; set; } = string.Empty;
        public Guid? LastReadId { get; set; }
        public virtual Book? LastRead { get; set; }
    }

    public class ApplicationRole : IdentityRole<Guid>
    {
    }
}
