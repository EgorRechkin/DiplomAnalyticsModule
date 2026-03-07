using AnalyticsModuleDiplomMVC.Models;
using AnalyticsModuleDiplomMVC.Services;

namespace AnalyticsModuleDiplomMVC.Controllers;

using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

public class AnalyticsController : Controller
{
    private readonly MockDataService _mockDataService;

    public AnalyticsController()
    {
        _mockDataService = new MockDataService();
    }

    public IActionResult Index()
    {
        var model = new AnalyticsViewModel
        {
            Employees = _mockDataService.Employees,
            AttendanceMarks = _mockDataService.AttendanceMarks,
            WorkSchedules = _mockDataService.WorkSchedules,
            ActionLogs = _mockDataService.ActionLogs
        };
        return View(model);
    }

    [HttpPost]
    public IActionResult AddManualMark(int employeeId, DateTime date, DateTime arrivalTime, DateTime departureTime)
    {
        var newMark = new AttendanceMark
        {
            Id = _mockDataService.AttendanceMarks.Max(m => m.Id) + 1,
            EmployeeId = employeeId,
            Date = date,
            ArrivalTime = arrivalTime,
            DepartureTime = departureTime,
            Status = "Ручная отметка"
        };

        _mockDataService.AttendanceMarks.Add(newMark);

        _mockDataService.ActionLogs.Add(new ActionLog
        {
            Id = _mockDataService.ActionLogs.Count + 1,
            ActionDate = DateTime.Now,
            ActionType = "Добавление отметки",
            User = "Руководитель",
            Description = $"Добавлена отметка для сотрудника {employeeId}"
        });

        return RedirectToAction("Index");
    }


    public IActionResult GenerateReport()
    {
        var reportData = _mockDataService.AttendanceMarks
            .GroupBy(m => m.EmployeeId)
            .Select(g => new EmployeeReportViewModel
            {
                Employee = _mockDataService.Employees.First(e => e.Id == g.Key),
                TotalHours = g.Sum(m => (m.DepartureTime - m.ArrivalTime).TotalHours),
                OverworkHours = g.Where(m => m.Status == "Переработка").Sum(m => (m.DepartureTime - m.ArrivalTime).TotalHours - 8),
                LateArrivals = g.Count(m => m.Status == "Опоздание")
            })
            .ToList();

        return View("Reports", reportData);
    }
}

public class AnalyticsViewModel
{
    public List<Employee> Employees { get; set; }
    public List<AttendanceMark> AttendanceMarks { get; set; }
    public List<WorkSchedule> WorkSchedules { get; set; }
    public List<ActionLog> ActionLogs { get; set; }
}

public class EmployeeReportViewModel
{
    public Employee Employee { get; set; }
    public double TotalHours { get; set; }
    public double OverworkHours { get; set; }
    public int LateArrivals { get; set; }
}
