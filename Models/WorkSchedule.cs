namespace AnalyticsModuleDiplomMVC.Models;

public class WorkSchedule
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string ShiftType { get; set; } // "День", "Ночь", "Скользящий"
}
