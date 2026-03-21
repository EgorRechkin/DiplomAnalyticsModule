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
    public IActionResult AttendanceChart()
    {
        var employees = _mockDataService.Employees;
        var attendanceRecords = _mockDataService.AttendanceRecords;

        // Группируем данные по датам и рассчитываем общее количество отработанных часов в день
        var attendanceByDate = attendanceRecords
            .Where(r => r.ArrivalTime.HasValue && r.DepartureTime.HasValue)
            .GroupBy(r => r.Date)
            .Select(g => new
            {
                Date = g.Key.ToString("yyyy-MM-dd"),
                TotalHours = g.Sum(r => (r.DepartureTime.Value - r.ArrivalTime.Value).TotalHours),
                EmployeeCount = g.Select(r => r.EmployeeId).Distinct().Count()
            })
            .OrderBy(r => r.Date)
            .ToList();

        var dates = attendanceByDate.Select(r => r.Date).ToList();
        var totalHours = attendanceByDate.Select(r => r.TotalHours).ToList();
        var employeeCounts = attendanceByDate.Select(r => r.EmployeeCount).ToList();

        var model = new AttendanceChartViewModel
        {
            Dates = dates,
            TotalHours = totalHours,
            EmployeeCounts = employeeCounts
        };

        return View(model);
    }


}

public class AttendanceChartViewModel
{
    public List<string> Dates { get; set; }
    public List<double> TotalHours { get; set; }
    public List<int> EmployeeCounts { get; set; }
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
