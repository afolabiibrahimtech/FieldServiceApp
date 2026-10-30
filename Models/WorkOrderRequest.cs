namespace FieldServiceApp.Models;

public class WorkOrderRequest
{
    public int ID { get; set; }
    public int WorkOrderId { get; set; }
    public WorkOrder? WorkOrder { get; set; }
    public string TechnicianId { get; set; } = string.Empty;
    public ApplicationUser? Technician { get; set; }
    public RequestStatus Status { get; set; }
    public DateTime DateRequested { get; set; }
}