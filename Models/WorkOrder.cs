namespace FieldServiceApp.Models;

public class WorkOrder

{
    
         public int ID {get;set;}
        public string JobTitle {get;set;}
        public string Description {get;set;}
        public Priority Priority {get;set;}
        public Status Status {get;set;}
        public string AssignedToId {get;set;}
        public ApplicationUser AssignedTo {get;set;}
        public DateTime DateCreated {get;set;}
    
}