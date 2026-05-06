namespace AnalyticsModuleDiplomMVC.Models;

public class AttendanceRecord
{
    public string Id { get; set; }
    public string EmployeeId { get; set; }
    public DateTime Date { get; set; }
    public DateTime? ArrivalTime { get; set; }
    public DateTime? DepartureTime { get; set; }
    public AttendanceStatus Status { get; set; }
}
