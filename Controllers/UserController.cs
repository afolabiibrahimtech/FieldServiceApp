using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FieldServiceApp.Models;

namespace FieldServiceApp.Controllers;

[Authorize(Roles = "Admin")]
public class UserController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;

    public UserController(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var users = await _userManager.Users.OrderBy(u => u.Email).ToListAsync();
        return View(users);
    }

    [HttpGet]
    public async Task<IActionResult> EditName(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }
        return View(user);
    }

    [HttpPost]
    public async Task<IActionResult> EditName(string id, string firstName, string lastName)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
        {
            TempData["Error"] = "First and last name can't be empty.";
            return RedirectToAction("EditName", new { id });
        }

        user.FirstName = firstName.Trim();
        user.LastName = lastName.Trim();
        await _userManager.UpdateAsync(user);

        TempData["Success"] = "Name updated for " + user.Email + ".";
        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> ChangeRole(string id, UserRole role)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        // stops an admin from locking themselves out
        if (user.Id == _userManager.GetUserId(User))
        {
            TempData["Error"] = "You can't change your own role.";
            return RedirectToAction("Index");
        }

        var oldRoles = await _userManager.GetRolesAsync(user);
        await _userManager.RemoveFromRolesAsync(user, oldRoles);
        await _userManager.AddToRoleAsync(user, role.ToString());

        user.Role = role;
        await _userManager.UpdateAsync(user);

        TempData["Success"] = user.Email + " is now " + role + ".";
        return RedirectToAction("Index");
    }
}