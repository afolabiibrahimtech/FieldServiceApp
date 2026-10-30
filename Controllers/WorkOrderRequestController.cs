using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FieldServiceApp.Data;
using FieldServiceApp.Models;

namespace FieldServiceApp.Controllers;

[Authorize]
public class WorkOrderRequestController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public WorkOrderRequestController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // open jobs nobody has picked up yet
    [Authorize(Roles = "Technician")]
    public IActionResult Available()
    {
        var userId = _userManager.GetUserId(User);

        var orders = _context.WorkOrders
            .Where(w => w.AssignedToId == null && w.Status == Status.Open)
            .OrderByDescending(w => w.Priority)
            .ToList();

        // so the view can show "Request pending"
        ViewBag.MyPending = _context.WorkOrderRequests
            .Where(r => r.TechnicianId == userId && r.Status == RequestStatus.Pending)
            .Select(r => r.WorkOrderId)
            .ToList();

        return View(orders);
    }

    [Authorize(Roles = "Technician")]
    [HttpPost]
    public IActionResult RequestWork(int id)
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null)
        {
            return Challenge();
        }

        var order = _context.WorkOrders.FirstOrDefault(w => w.ID == id);
        if (order == null)
        {
            return NotFound();
        }

        if (order.AssignedToId != null)
        {
            TempData["Error"] = "That work order has already been assigned.";
            return RedirectToAction("Available");
        }

        // don't let them request the same job twice
        var alreadyRequested = _context.WorkOrderRequests.Any(r =>
            r.WorkOrderId == id &&
            r.TechnicianId == userId &&
            r.Status == RequestStatus.Pending);

        if (!alreadyRequested)
        {
            _context.WorkOrderRequests.Add(new WorkOrderRequest
            {
                WorkOrderId = id,
                TechnicianId = userId,
                Status = RequestStatus.Pending,
                DateRequested = DateTime.Now
            });
            _context.SaveChanges();
        }

        TempData["Success"] = "Request sent. A supervisor will review it.";
        return RedirectToAction("Available");
    }

    [Authorize(Roles = "Admin,Supervisor")]
    public IActionResult Pending()
    {
        var requests = _context.WorkOrderRequests
            .Include(r => r.WorkOrder)
            .Include(r => r.Technician)
            .Where(r => r.Status == RequestStatus.Pending)
            .OrderBy(r => r.DateRequested)
            .ToList();

        return View(requests);
    }

    [Authorize(Roles = "Admin,Supervisor")]
    [HttpPost]
    public IActionResult Approve(int id)
    {
        var request = _context.WorkOrderRequests
            .Include(r => r.WorkOrder)
            .FirstOrDefault(r => r.ID == id);

        if (request == null || request.WorkOrder == null)
        {
            return NotFound();
        }

        // already taken by someone else
        if (request.WorkOrder.AssignedToId != null)
        {
            request.Status = RequestStatus.Rejected;
            _context.SaveChanges();
            TempData["Error"] = "That work order was already assigned, so the request was rejected.";
            return RedirectToAction("Pending");
        }

        request.WorkOrder.AssignedToId = request.TechnicianId;
        request.Status = RequestStatus.Approved;

        // reject everyone else who asked for this job
        var others = _context.WorkOrderRequests
            .Where(r => r.WorkOrderId == request.WorkOrderId
                     && r.ID != request.ID
                     && r.Status == RequestStatus.Pending)
            .ToList();

        foreach (var other in others)
        {
            other.Status = RequestStatus.Rejected;
        }

        _context.SaveChanges();
        TempData["Success"] = "Request approved and the work order has been assigned.";
        return RedirectToAction("Pending");
    }

    [Authorize(Roles = "Admin,Supervisor")]
    [HttpPost]
    public IActionResult Reject(int id)
    {
        var request = _context.WorkOrderRequests.FirstOrDefault(r => r.ID == id);
        if (request == null)
        {
            return NotFound();
        }

        request.Status = RequestStatus.Rejected;
        _context.SaveChanges();
        TempData["Success"] = "Request rejected.";
        return RedirectToAction("Pending");
    }
}