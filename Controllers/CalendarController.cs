using AnalyticsModuleDiplomMVC.Models;
using AnalyticsModuleDiplomMVC.Services;

namespace AnalyticsModuleDiplomMVC.Controllers;

using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

public class CalendarController : Controller
{
    private readonly MockDataService _mockDataService;

    public CalendarController()
    {
        _mockDataService = new MockDataService();
    }

    public IActionResult Index(int employeeId = 1)
    {
        var employee = _mockDataService.Employees.FirstOrDefault(e => e.Id == employeeId);
        if (employee == null)
            employee = _mockDataService.Employees.First();

        var records = _mockDataService.AttendanceRecords
            .Where(r => r.EmployeeId == employee.Id)
            .OrderBy(r => r.Date)
            .ToList();

        // Рассчитываем агрегированные данные для дашборда
        var totalHoursWorked = records
            .Where(r => r.ArrivalTime.HasValue && r.DepartureTime.HasValue)
            .Sum(r => (r.DepartureTime.Value - r.ArrivalTime.Value).TotalHours);

        var totalLateArrivals = records.Count(r => r.Status == AttendanceStatus.Late);
        var totalOverworks = records.Count(r => r.Status == AttendanceStatus.Overwork);
        var totalAbsences = records.Count(r => r.Status == AttendanceStatus.Absent);

        // Подготовка данных для графика посещаемости
        var dates = records.Select(r => r.Date.ToString("yyyy-MM-dd")).ToList();
        var hoursWorked = records
            .Where(r => r.ArrivalTime.HasValue && r.DepartureTime.HasValue)
            .Select(r => (r.DepartureTime.Value - r.ArrivalTime.Value).TotalHours)
            .ToList();

        var model = new CalendarViewModel
        {
            Employee = employee,
            Records = records,
            Employees = _mockDataService.Employees,
            TotalHoursWorked = totalHoursWorked,
            TotalLateArrivals = totalLateArrivals,
            TotalOverworks = totalOverworks,
            TotalAbsences = totalAbsences,
            Dates = dates,
            HoursWorked = hoursWorked
        };

        return View(model);
    }

}

public class CalendarViewModel
{
    public Employee Employee { get; set; }
    public List<AttendanceRecord> Records { get; set; }
    public List<Employee> Employees { get; set; }

    // Данные для индивидуального дашборда
    public double TotalHoursWorked { get; set; }
    public int TotalLateArrivals { get; set; }
    public int TotalOverworks { get; set; }
    public int TotalAbsences { get; set; }

    // Данные для графика посещаемости
    public List<string> Dates { get; set; }
    public List<double> HoursWorked { get; set; }
}


