using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using FieldServiceApp.Models;
using FieldServiceApp.ViewModels;

namespace FieldServiceApp.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    [HttpGet]
    public IActionResult Login() => View();

    [HttpGet]
    public IActionResult Register() => View();

    [HttpGet]
    public IActionResult AccessDenied() => View();

    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (ModelState.IsValid)
        {
            // everyone who signs up starts as a technician, admin promotes later
            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                Role = UserRole.Technician
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                // Identity role, needed for [Authorize(Roles = ...)]
                await _userManager.AddToRoleAsync(user, UserRole.Technician.ToString());

                await _signInManager.SignInAsync(user, isPersistent: false);
                TempData["Success"] = "Account created successfully. Welcome, " + model.FirstName + "!";
                return RedirectToAction("Index", "WorkOrder");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
        }
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (ModelState.IsValid)
        {
            var result = await _signInManager.PasswordSignInAsync(
                model.Email, model.Password, isPersistent: false, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                TempData["Success"] = "You are now logged in.";
                return RedirectToAction("Index", "WorkOrder");
            }

            ModelState.AddModelError("", "Invalid login attempt.");
        }
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Login");
    }
}