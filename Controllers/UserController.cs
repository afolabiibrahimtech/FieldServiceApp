using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FieldServiceApp.Data;
using FieldServiceApp.Models;

namespace FieldServiceApp.Controllers;

[Authorize(Roles = "Admin")]
public class UserController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;

    public UserController(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
    {
        _userManager = userManager;
        _context = context;
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

    [HttpPost]
    public async Task<IActionResult> ToggleLogin(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        if (user.Id == _userManager.GetUserId(User))
        {
            TempData["Error"] = "You can't disable your own account.";
            return RedirectToAction("Index");
        }

        var isDisabled = user.LockoutEnd != null && user.LockoutEnd > DateTimeOffset.UtcNow;

        if (isDisabled)
        {
            await _userManager.SetLockoutEndDateAsync(user, null);
            TempData["Success"] = user.Email + " can log in again.";
        }
        else
        {
            await _userManager.SetLockoutEnabledAsync(user, true);
            await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
            await _userManager.UpdateSecurityStampAsync(user);
            TempData["Success"] = user.Email + " has been disabled.";
        }

        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> Delete(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        if (user.Id == _userManager.GetUserId(User))
        {
            TempData["Error"] = "You can't delete your own account.";
            return RedirectToAction("Index");
        }

        // free up their jobs so nothing is left pointing at a deleted user
        var jobs = _context.WorkOrders.Where(w => w.AssignedToId == user.Id).ToList();
        foreach (var job in jobs)
        {
            job.AssignedToId = null;
            if (job.Status == Status.InProgress)
            {
                job.Status = Status.Open;
            }
        }
        await _context.SaveChangesAsync();

        var result = await _userManager.DeleteAsync(user);
        if (result.Succeeded)
        {
            TempData["Success"] = user.Email + " was deleted.";
        }
        else
        {
            TempData["Error"] = "Couldn't delete that user.";
        }

        return RedirectToAction("Index");
    }
}