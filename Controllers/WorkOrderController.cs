using Microsoft.AspNetCore.Mvc;
using FieldServiceApp.Models;
using FieldServiceApp.Data;

namespace FieldServiceApp.Controllers;

public class WorkOrderController : Controller
{
    private readonly ApplicationDbContext _context;

    public WorkOrderController(ApplicationDbContext context)
    {
        _context = context;
    }
    public IActionResult Create()
    {
        return View();
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
