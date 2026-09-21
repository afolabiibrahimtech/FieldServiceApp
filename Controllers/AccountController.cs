using Microsoft.AspNetCore.Mvc;
using FieldServiceApp.Models;

namespace FieldServiceApp.Controllers;

public class AccountController : Controller
{
    public IActionResult Login()
    {
        return View();
    }
    public IActionResult Register()
    {
        return View();
    }

}
