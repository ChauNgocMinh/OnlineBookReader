using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineBookReader.Models;

namespace OnlineBookReader.Controllers.AdminSite
{
    [Authorize(Roles = "Admin")]
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<ApplicationRole> _roleManager;

        public AccountController(UserManager<ApplicationUser> userManager,
                                    SignInManager<ApplicationUser> signInManager,
                                    RoleManager<ApplicationRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index()
        {
            var users = _userManager.Users.ToList();
            var roles = await _roleManager.Roles.ToListAsync();
            var userRoles = new Dictionary<Guid, IList<string>>();
            foreach (var user in users)
            {
                userRoles[user.Id] = await _userManager.GetRolesAsync(user);
            }
            ViewBag.Roles = roles;
            ViewBag.UserRoles = userRoles;
            return View(users);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateRole(Guid userId, string roleName)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());

            if (user == null)
            {
                return NotFound("User not found.");
            }
            var userRoles = await _userManager.GetRolesAsync(user);
            foreach (var role in userRoles)
            {
                await _userManager.RemoveFromRoleAsync(user, role);
            }
            await _userManager.AddToRoleAsync(user, roleName);

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> ToggleLock(Guid id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                return NotFound();
            }
            if (user.LockoutEnd.HasValue && user.LockoutEnd > DateTimeOffset.UtcNow)
            {
                user.LockoutEnd = null;
            }
            else
            {
                user.LockoutEnd = DateTimeOffset.UtcNow.AddYears(100);
            }
            await _userManager.UpdateAsync(user);
            return RedirectToAction(nameof(Index));
        }

    }
}
