using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FieldServiceApp.Models;
using FieldServiceApp.Data;
using FieldServiceApp.ViewModels;

namespace FieldServiceApp.Controllers;

[Authorize]
public class WorkOrderController : Controller
{
    private readonly ApplicationDbContext _context;

    public WorkOrderController(ApplicationDbContext context)
    {
        _context = context;
    }

    // dropdown for Assigned To
    private void LoadUsers(string? selectedId = null)
    {
        var users = _context.Users.OrderBy(u => u.Email).ToList();
        ViewBag.Users = new SelectList(users, "Id", "Email", selectedId);
    }

    // only returns the order if it is assigned to the logged in user
    private WorkOrder? GetMyJob(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return _context.WorkOrders.FirstOrDefault(w => w.ID == id && w.AssignedToId == userId);
    }

    public IActionResult Index()
    {
        var workOrders = _context.WorkOrders
            .Include(w => w.AssignedTo)
            .ToList();
        return View(workOrders);
    }

    [Authorize(Roles = "Admin,Supervisor")]
    [HttpGet]
    public IActionResult Create()
    {
        LoadUsers();
        return View();
    }

    [Authorize(Roles = "Admin,Supervisor")]
    [HttpPost]
    public IActionResult Create(WorkOrderViewModel model)
    {
        if (ModelState.IsValid)
        {
            var workOrder = new WorkOrder
            {
                JobTitle = model.JobTitle,
                Description = model.Description,
                Priority = model.Priority,
                Status = model.Status,
                AssignedToId = model.AssignedToId,
                DateCreated = DateTime.Now
            };

            _context.WorkOrders.Add(workOrder);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
        LoadUsers(model.AssignedToId);
        return View(model);
    }

    [Authorize(Roles = "Admin,Supervisor")]
    [HttpGet]
    public IActionResult Edit(int id)
    {
        var workOrder = _context.WorkOrders.FirstOrDefault(w => w.ID == id);
        if (workOrder == null)
        {
            return NotFound();
        }
        var model = new WorkOrderViewModel
        {
            JobTitle = workOrder.JobTitle ?? string.Empty,
            Description = workOrder.Description ?? string.Empty,
            Priority = workOrder.Priority,
            Status = workOrder.Status,
            AssignedToId = workOrder.AssignedToId
        };
        LoadUsers(workOrder.AssignedToId);
        return View(model);
    }

    [Authorize(Roles = "Admin,Supervisor")]
    [HttpPost]
    public IActionResult Edit(int id, WorkOrderViewModel model)
    {
        if (ModelState.IsValid)
        {
            var workOrder = _context.WorkOrders.FirstOrDefault(w => w.ID == id);
            if (workOrder == null)
            {
                return NotFound();
            }
            workOrder.JobTitle = model.JobTitle;
            workOrder.Description = model.Description;
            workOrder.Priority = model.Priority;
            workOrder.Status = model.Status;
            workOrder.AssignedToId = model.AssignedToId;

            if (model.Status == Status.Completed && workOrder.DateCompleted == null)
            {
                workOrder.DateCompleted = DateTime.Now;
            }

            _context.SaveChanges();
            return RedirectToAction("Index");
        }
        LoadUsers(model.AssignedToId);
        return View(model);
    }

    public IActionResult Details(int id)
    {
        var workOrder = _context.WorkOrders
            .Include(w => w.AssignedTo)
            .FirstOrDefault(w => w.ID == id);
        if (workOrder == null)
        {
            return NotFound();
        }
        return View(workOrder);
    }

    [Authorize(Roles = "Admin,Supervisor")]
    [HttpGet]
    public IActionResult Delete(int id)
    {
        var workOrder = _context.WorkOrders.FirstOrDefault(w => w.ID == id);
        if (workOrder == null)
        {
            return NotFound();
        }
        return View(workOrder);
    }

    [Authorize(Roles = "Admin,Supervisor")]
    [HttpPost, ActionName("Delete")]
    public IActionResult DeleteConfirmed(int id)
    {
        var workOrder = _context.WorkOrders.FirstOrDefault(w => w.ID == id);
        if (workOrder != null)
        {
            _context.WorkOrders.Remove(workOrder);
            _context.SaveChanges();
        }
        return RedirectToAction("Index");
    }

    // technician pages

    [Authorize(Roles = "Technician")]
    public IActionResult MyJobs()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var jobs = _context.WorkOrders
            .Where(w => w.AssignedToId == userId)
            .OrderBy(w => w.Status)
            .ThenByDescending(w => w.Priority)
            .ToList();
        return View(jobs);
    }

    [Authorize(Roles = "Technician")]
    [HttpGet]
    public IActionResult Update(int id)
    {
        var workOrder = GetMyJob(id);
        if (workOrder == null)
        {
            return NotFound();
        }
        return View(workOrder);
    }

    [Authorize(Roles = "Technician")]
    [HttpPost]
    public IActionResult SaveNote(int id, string? notes)
    {
        var workOrder = GetMyJob(id);
        if (workOrder == null)
        {
            return NotFound();
        }

        workOrder.TechnicianNotes = notes;
        _context.SaveChanges();
        TempData["Success"] = "Note saved. Your supervisor can see it.";
        return RedirectToAction("MyJobs");
    }

    [Authorize(Roles = "Technician")]
    [HttpPost]
    public IActionResult Complete(int id, string? notes)
    {
        var workOrder = GetMyJob(id);
        if (workOrder == null)
        {
            return NotFound();
        }

        if (workOrder.Status == Status.Cancelled)
        {
            TempData["Error"] = "This work order was cancelled.";
            return RedirectToAction("MyJobs");
        }

        workOrder.TechnicianNotes = notes;
        workOrder.Status = Status.Completed;
        workOrder.DateCompleted = DateTime.Now;
        _context.SaveChanges();
        TempData["Success"] = "Job marked as completed.";
        return RedirectToAction("MyJobs");
    }
}