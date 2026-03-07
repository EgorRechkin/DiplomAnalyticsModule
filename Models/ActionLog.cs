namespace AnalyticsModuleDiplomMVC.Models;

public class ActionLog
{
    public int Id { get; set; }
    public DateTime ActionDate { get; set; }
    public string ActionType { get; set; }
    public string User { get; set; }
    public string Description { get; set; }
}
