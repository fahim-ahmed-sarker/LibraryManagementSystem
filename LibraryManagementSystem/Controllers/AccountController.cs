using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ApplicationDbContext context,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _configuration = configuration;
        }

        // ==========================================
        // LOGIN
        // ==========================================

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;

            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(
            string email,
            string password,
            bool rememberMe = false,
            string? returnUrl = null)
        {
            if (string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError(
                    "",
                    "Email and password are required.");

                ViewBag.ReturnUrl = returnUrl;

                return View();
            }

            var user = await _userManager
                .FindByEmailAsync(email.Trim());

            if (user == null)
            {
                ModelState.AddModelError(
                    "",
                    "Invalid email or password.");

                ViewBag.ReturnUrl = returnUrl;

                return View();
            }

            var result = await _signInManager
                .PasswordSignInAsync(
                    user.UserName!,
                    password,
                    rememberMe,
                    lockoutOnFailure: true);

            if (result.Succeeded)
            {
                if (!string.IsNullOrWhiteSpace(returnUrl) &&
                    Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }

                if (await _userManager.IsInRoleAsync(
                    user,
                    "Librarian"))
                {
                    return RedirectToAction(
                        "Index",
                        "Reports");
                }

                return RedirectToAction(
                    "Index",
                    "Home");
            }

            if (result.IsLockedOut)
            {
                ModelState.AddModelError(
                    "",
                    "Your account is temporarily locked. Please try again later.");
            }
            else
            {
                ModelState.AddModelError(
                    "",
                    "Invalid email or password.");
            }

            ViewBag.ReturnUrl = returnUrl;

            return View();
        }


        // ==========================================
        // MEMBER REGISTRATION
        // ==========================================

        [HttpGet]
        [AllowAnonymous]
        public IActionResult RegisterMember()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterMember(
            RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var existingUser =
                await _userManager.FindByEmailAsync(
                    model.Email.Trim());

            if (existingUser != null)
            {
                ModelState.AddModelError(
                    "Email",
                    "An account with this email already exists.");

                return View(model);
            }

            var user = new ApplicationUser
            {
                UserName = model.Email.Trim(),
                Email = model.Email.Trim(),
                FullName = model.FullName.Trim(),
                EmailConfirmed = true
            };

            var result = await _userManager
                .CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(
                        "",
                        error.Description);
                }

                return View(model);
            }

            await _userManager.AddToRoleAsync(
                user,
                "Member");

            var member = new Member
            {
                ApplicationUserId = user.Id,
                Name = model.FullName.Trim(),
                Email = model.Email.Trim(),
                RegistrationDate = DateTime.Now
            };

            _context.Members.Add(member);

            await _context.SaveChangesAsync();

            await _signInManager.SignInAsync(
                user,
                isPersistent: false);

            return RedirectToAction(
                "Index",
                "Home");
        }


        // ==========================================
        // LIBRARIAN REGISTRATION
        // ==========================================

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> RegisterLibrarian()
        {
            if (await _context.Librarians.AnyAsync() &&
                !User.IsInRole("Librarian"))
            {
                return RedirectToAction(
                    nameof(Login),
                    new
                    {
                        returnUrl = Url.Action(
                            nameof(RegisterLibrarian),
                            "Account")
                    });
            }

            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterLibrarian(
            RegisterViewModel model,
            string librarianCode)
        {
            var librarianExists = await _context.Librarians.AnyAsync();

            if (librarianExists && !User.IsInRole("Librarian"))
            {
                return Forbid();
            }

            var configuredCode =
                _configuration["LibrarianRegistration:Code"];

            if (string.IsNullOrWhiteSpace(librarianCode) ||
                !string.Equals(
                    librarianCode.Trim(),
                    configuredCode,
                    StringComparison.Ordinal))
            {
                ModelState.AddModelError(
                    "librarianCode",
                    "Invalid librarian registration code.");
            }

            if (!ModelState.IsValid)
                return View(model);

            var existingUser =
                await _userManager.FindByEmailAsync(
                    model.Email.Trim());

            if (existingUser != null)
            {
                ModelState.AddModelError(
                    "Email",
                    "An account with this email already exists.");

                return View(model);
            }

            var user = new ApplicationUser
            {
                UserName = model.Email.Trim(),
                Email = model.Email.Trim(),
                FullName = model.FullName.Trim(),
                EmailConfirmed = true
            };

            var result = await _userManager
                .CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(
                        "",
                        error.Description);
                }

                return View(model);
            }

            await _userManager.AddToRoleAsync(
                user,
                "Librarian");

            // Get first library if one exists.
            var library = await _context.Libraries
                .OrderBy(l => l.LibraryID)
                .FirstOrDefaultAsync();

            // If no library exists, create a default one.
            if (library == null)
            {
                library = new Library
                {
                    Name = "Main Library",
                    Location = "Not Set",
                    OperatingHours = "Not Set",
                    ContactDetails = "Not Set"
                };

                _context.Libraries.Add(library);

                await _context.SaveChangesAsync();
            }

            var librarian = new Librarian
            {
                ApplicationUserId = user.Id,
                Name = model.FullName.Trim(),
                Email = model.Email.Trim(),
                LibraryID = library.LibraryID
            };

            _context.Librarians.Add(librarian);

            await _context.SaveChangesAsync();

            await _signInManager.SignInAsync(
                user,
                isPersistent: false);

            return RedirectToAction(
                "Index",
                "Reports");
        }


        // ==========================================
        // LOGOUT
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();

            return RedirectToAction(
                "Index",
                "Home");
        }
    }
}