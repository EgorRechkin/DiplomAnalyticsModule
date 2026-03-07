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

        var model = new CalendarViewModel
        {
            Employee = employee,
            Records = records,
            Employees = _mockDataService.Employees // Передаем список всех сотрудников
        };

        return View(model);
    }
}

public class CalendarViewModel
{
    public Employee Employee { get; set; }
    public List<AttendanceRecord> Records { get; set; }
    public List<Employee> Employees { get; set; } // Список всех сотрудников
}
