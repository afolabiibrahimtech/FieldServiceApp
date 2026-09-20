namespace FieldServiceApp.Models;
using Microsoft.AspNetCore.Identity;
public class ApplicationUser : IdentityUser
{
   public UserRole Role {get;set;}
}
