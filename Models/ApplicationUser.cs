namespace FieldServiceApp.Models;
using Microsoft.AspNetCore.Identity;
public class ApplicationUser : IdentityUser
{
   public UserRole Role {get;set;}
   public string FirstName {get;set;} = string.Empty;
   public string LastName {get;set;} = string.Empty;

   // first and last name together, used for showing a person on the pages
   public string FullName => (FirstName + " " + LastName).Trim();
}