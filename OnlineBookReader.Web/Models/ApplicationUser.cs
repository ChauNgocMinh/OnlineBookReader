using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace OnlineBookReader.Web.Models
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        public string FullName { get; set; } = string.Empty;
    }

    public class ApplicationRole : IdentityRole<Guid>
    {
    }
}
