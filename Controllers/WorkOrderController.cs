using Microsoft.AspNetCore.Mvc;
using FieldServiceApp.Models;
using FieldServiceApp.Data;
using FieldServiceApp.ViewModels;

namespace FieldServiceApp.Controllers;

public class WorkOrderController : Controller
{
    private readonly ApplicationDbContext _context;

    public WorkOrderController(ApplicationDbContext context)
    {
        _context = context;
    }
    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }
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
        return View(model);
    }
    public IActionResult Index()
    {
        var workOrders = _context.WorkOrders.ToList();
        return View(workOrders);
    }
    public IActionResult Edit()
    {
        return View();
    }
    public IActionResult Details()
    {
        return View();
    }

    public IActionResult Delete()
    {
        return View();
    }
}
