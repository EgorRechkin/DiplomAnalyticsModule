namespace AnalyticsModuleDiplomMVC.Models;

public class WorkSchedule
{
    public string Id { get; set; }
    public string EmployeeName { get; set; }
    public string EmployeeId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string ShiftType { get; set; } // "День", "Ночь", "Скользящий"
}
