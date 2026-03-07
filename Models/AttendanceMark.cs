namespace AnalyticsModuleDiplomMVC.Models;

public class AttendanceMark
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public DateTime Date { get; set; }
    public DateTime ArrivalTime { get; set; }
    public DateTime DepartureTime { get; set; }
    public string Status { get; set; } // "Опоздание", "Переработка", "Норма"
}
