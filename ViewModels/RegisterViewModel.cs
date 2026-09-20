namespace FieldServiceApp.ViewModels;
using System.ComponentModel.DataAnnotations;
using FieldServiceApp.Models;

public class RegisterViewModel
{
    [Required]
    public string FirstName {get;set;}

    [Required]
    public string LastName {get;set;}
    
    [Required]
    [EmailAddress]
    public string Email {get;set;}
    [Required]
    [DataType(DataType.Password)]
    public string Password {get;set;} 

    [Required]
    [DataType(DataType.Password)]
    [Compare("Password")]
    public string ConfirmPassword {get;set;} 

    [Required]
    public UserRole Role {get;set;} 
}