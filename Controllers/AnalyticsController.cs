using AnalyticsModuleDiplomMVC.Models;
using AnalyticsModuleDiplomMVC.Services;

namespace AnalyticsModuleDiplomMVC.Controllers;

using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

public class AnalyticsController : Controller
{
    private readonly DataService _dataService;

    public AnalyticsController(DataService dataService)
    {
        _dataService = dataService;
    }

    public IActionResult Index()
    {
        if (!_dataService.IsDatabaseConnected())
        {
            ViewBag.ConnectionError = "Не удалось подключиться к базе данных. Проверьте настройки подключения.";
            return View(new AnalyticsViewModel());
        }

        var model = new AnalyticsViewModel
        {
            Employees = _dataService.Employees,
            AttendanceMarks = _dataService.AttendanceRecords,
            WorkSchedules = _dataService.WorkSchedules,
            ActionLogs = _dataService.ActionLogs
        };
        return View(model);
    }

    public IActionResult AttendanceChart()
    {
        if (!_dataService.IsDatabaseConnected())
        {
            ViewBag.ConnectionError = "Не удалось подключиться к базе данных. Проверьте настройки подключения.";
            return View(new AttendanceChartViewModel());
        }

        var employees = _dataService.Employees;
        var attendanceRecords = _dataService.AttendanceRecords;

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
    public List<AttendanceRecord> AttendanceMarks { get; set; }
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
