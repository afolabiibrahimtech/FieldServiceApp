namespace FieldServiceApp.ViewModels;
using System.ComponentModel.DataAnnotations;
using FieldServiceApp.Models;
public class WorkOrderViewModel
{
        [Required]
        public string JobTitle {get;set;}
        [Required]
        public string Description {get;set;}
        [Required]
        public Priority Priority {get;set;}
        [Required]
        public Status Status {get;set;}
        [Required]
        public string AssignedToID {get;set;}
        
}