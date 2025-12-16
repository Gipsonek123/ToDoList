using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApp.Models;
using WebApp.Models.EntityFramework;
using WebApp.Services.Interfaces;
using WebApp.ViewModels;
using static WebApp.ViewModels.AdminViewModel;

namespace WebApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly WebAppDbContext _webAppDbContext;
        private readonly RoleManager<IdentityRole<int>> _roleManager;
        private readonly SignInManager<User> _signInManager;

        public AdminController(UserManager<User> userManager, WebAppDbContext webAppDbContext, RoleManager<IdentityRole<int>> roleManager, SignInManager<User> signInManager)
        {
            _userManager = userManager;
            _webAppDbContext = webAppDbContext;
            _roleManager = roleManager;
            _signInManager = signInManager;
        }

        [HttpGet]
        public async Task<IActionResult> AdminPanel()
        {
            var allUsers = await _userManager.Users
                .Where(u => u.UserName.ToLower() != "admin")
                .ToListAsync();
            var userList = new List<UserViewModel>();

            foreach (var user in allUsers)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userList.Add(new UserViewModel
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    Role = roles.FirstOrDefault()
                });
            }

            return View(userList);
        }

        [HttpGet]
        public IActionResult CreateNewUser()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateNewUser(RegisterViewModel registerViewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(registerViewModel);
            }

            var user = new User
            {
                UserName = registerViewModel.Username,
                Email = registerViewModel.Email
            };

            var result = await _userManager.CreateAsync(user, registerViewModel.Password);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                return View(registerViewModel);
            }

            const string defaultRole = "User";
            if (!await _roleManager.RoleExistsAsync(defaultRole))
            {
                await _roleManager.CreateAsync(new IdentityRole<int>(defaultRole));
            }

            await _userManager.AddToRoleAsync(user, defaultRole);

            return RedirectToAction("AdminPanel");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            await _userManager.DeleteAsync(user);
            return RedirectToAction("AdminPanel");
        }

        [HttpGet]
        public async Task<IActionResult> EditUser(int id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());

            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault();

            var model = new AdminViewModel
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
            };

            if (!string.IsNullOrEmpty(role) && Enum.TryParse<RoleType>(role, out var parsed))
            {
                model.Role = parsed;
            }

            ModelState.Clear();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(int id, AdminViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByIdAsync(id.ToString());

            var existingByUserName = await _userManager.FindByNameAsync(model.UserName);
            if (existingByUserName != null && existingByUserName.Id != user.Id)
            {
                ModelState.AddModelError(nameof(model.UserName), "This username already exist.");
                return View(model);
            }

            var existingByEmail = await _userManager.FindByEmailAsync(model.Email);
            if (existingByEmail != null && existingByEmail.Id != user.Id)
            {
                ModelState.AddModelError(nameof(model.Email), "This email is already used.");
                return View(model);
            }

            await _userManager.SetUserNameAsync(user, model.UserName);
            await _userManager.SetEmailAsync(user, model.Email);

            if (!string.IsNullOrEmpty(model.Password))
            {
                foreach (var validator in _userManager.PasswordValidators)
                {
                    var validation = await validator.ValidateAsync(_userManager, user, model.Password);
                    if (!validation.Succeeded)
                    {
                        foreach (var error in validation.Errors)
                        {
                            ModelState.AddModelError(nameof(model.Password), error.Description);
                        }
                        return View(model);
                    }
                }

                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var pwResult = await _userManager.ResetPasswordAsync(user, token, model.Password);
                if (!pwResult.Succeeded)
                {
                    foreach (var e in pwResult.Errors) ModelState.AddModelError(string.Empty, e.Description);
                    return View(model);
                }
            }

            var roles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, roles);

            string newRole = model.Role.ToString();
            await _userManager.AddToRoleAsync(user, newRole);

            await _userManager.UpdateAsync(user);
            await _signInManager.RefreshSignInAsync(user);

            if (user == await _userManager.GetUserAsync(User))
            {
                return RedirectToAction("Welcome", "Account");
            }

            return RedirectToAction("AdminPanel");
        }

    }
}
