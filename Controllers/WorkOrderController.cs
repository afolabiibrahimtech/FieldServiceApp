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
        JobTitle = workOrder.JobTitle,
        Description = workOrder.Description,
        Priority = workOrder.Priority,
        Status = workOrder.Status,
        AssignedToId = workOrder.AssignedToId
    };
    return View(model);
}

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
        _context.SaveChanges();
        return RedirectToAction("Index");
    }
    return View(model);
}
    public IActionResult Details(int id)
    {
        var workOrder = _context.WorkOrders.FirstOrDefault(w => w.ID == id);
        if (workOrder == null)
        {
            return NotFound();
        }
        return View(workOrder);
    }

    public IActionResult Delete()
    {
        return View();
    }
}
