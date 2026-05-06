using AnalyticsModuleDiplomMVC.Models;


namespace AnalyticsModuleDiplomMVC.Controllers;

using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

public class CalendarController : Controller
{
    private readonly DataService _dataService;
    private readonly ILogger<CalendarController> _logger;

    public CalendarController(DataService dataService, ILogger<CalendarController> logger)
    {
        _dataService = dataService;
        _logger = logger;
    }

    public IActionResult Index(string employeeId = "1")
    {
        if (!_dataService.IsDatabaseConnected())
        {
            ViewBag.ConnectionError = "Не удалось подключиться к базе данных. Проверьте настройки подключения.";
            return View(new CalendarViewModel());
        }

        var employee = _dataService.Employees.FirstOrDefault(e => e.Id == employeeId);
        if (employee == null)
            employee = _dataService.Employees.First();

        var records = _dataService.AttendanceRecords
            .Where(r => r.EmployeeId == employee.Id)
            .OrderBy(r => r.Date)
            .ToList();

        var totalHoursWorked = records
            .Where(r => r.ArrivalTime.HasValue && r.DepartureTime.HasValue)
            .Sum(r => (r.DepartureTime.Value - r.ArrivalTime.Value).TotalHours);

        var totalLateArrivals = records.Count(r => r.Status == AttendanceStatus.Late);
        var totalOverworks = records.Count(r => r.Status == AttendanceStatus.Overwork);
        var totalAbsences = records.Count(r => r.Status == AttendanceStatus.Absent);

        var dates = records.Select(r => r.Date.ToString("yyyy-MM-dd")).ToList();
        var hoursWorked = records
            .Where(r => r.ArrivalTime.HasValue && r.DepartureTime.HasValue)
            .Select(r => (r.DepartureTime.Value - r.ArrivalTime.Value).TotalHours)
            .ToList();

        var model = new CalendarViewModel
        {
            Employee = employee,
            Records = records,
            Employees = _dataService.Employees,
            TotalHoursWorked = totalHoursWorked,
            TotalLateArrivals = totalLateArrivals,
            TotalOverworks = totalOverworks,
            TotalAbsences = totalAbsences,
            Dates = dates,
            HoursWorked = hoursWorked
        };

        return View(model);
    }

    // ★★★ НОВЫЙ МЕТОД: Форма для добавления расписания ★★★
    public IActionResult AddSchedule(string employeeId = "1")
    {
        var employee = _dataService.Employees.FirstOrDefault(e => e.Id == employeeId);
        if (employee == null)
            employee = _dataService.Employees.First();

        var model = new WorkScheduleViewModel
        {
            EmployeeId = employee.Id,
            EmployeeName = employee.FullName,
            Employees = _dataService.Employees,
            StartTime = "09:00",
            EndTime = "18:00",
            ShiftType = "День"
        };

        return View(model);
    }
    
    public IActionResult AddEmployee()
    {
        return View();
    }

    [HttpPost]
    public IActionResult AddEmployee(Employee employee)
    {
        if (!ModelState.IsValid)
        {
            return View(employee);
        }

        try
        {
            _dataService.AddEmployee(employee);
            TempData["SuccessMessage"] = $"Сотрудник {employee.FullName} успешно добавлен в базу данных!";
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"Ошибка при добавлении сотрудника: {ex.Message}");
            _logger.LogError($"Ошибка добавления сотрудника: {ex.Message}");
            return View(employee);
        }
    }

    [HttpPost]
    public IActionResult AddSchedule(WorkScheduleViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.Employees = _dataService.Employees;
            var employee = _dataService.Employees.FirstOrDefault(e => e.Id == model.EmployeeId);
            if (employee != null)
                model.EmployeeName = employee.FullName;
            return View(model);
        }

        try
        {
            // Парсим время
            var timeParts = model.StartTime.Split(':');
            var startHour = int.Parse(timeParts[0]);
            var startMinute = int.Parse(timeParts[1]);

            timeParts = model.EndTime.Split(':');
            var endHour = int.Parse(timeParts[0]);
            var endMinute = int.Parse(timeParts[1]);

            var schedule = new WorkSchedule
            {
                EmployeeId = model.EmployeeId,
                EmployeeName = model.EmployeeName,
                StartTime = DateTime.Today.AddHours(startHour).AddMinutes(startMinute),
                EndTime = DateTime.Today.AddHours(endHour).AddMinutes(endMinute),
                ShiftType = model.ShiftType
            };

            _dataService.AddWorkSchedule(schedule, model.WorkingDays, model.DaysOff);

            var employee = _dataService.Employees.FirstOrDefault(e => e.Id == model.EmployeeId);
            TempData["SuccessMessage"] = $"Расписание '{model.ShiftType}' ({model.StartTime}-{model.EndTime}) для {employee?.FullName} успешно добавлено!";
        
            return RedirectToAction("Index", new { employeeId = model.EmployeeId });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"Ошибка при добавлении расписания: {ex.Message}");
            _logger.LogError($"Ошибка добавления расписания: {ex.Message}");
            model.Employees = _dataService.Employees;
            return View(model);
        }
    }
}

public class WorkScheduleViewModel
{
    public string EmployeeId { get; set; }
    public string EmployeeName { get; set; }
    public string StartTime { get; set; } // "HH:mm"
    public string EndTime { get; set; }   // "HH:mm"
    public string ShiftType { get; set; } // "День", "Ночь", "Скользящий"
    public int WorkingDays { get; set; } = 5; // Количество рабочих дней
    public int DaysOff { get; set; } = 2;     // Количество выходных дней
    public List<Employee> Employees { get; set; } = new List<Employee>();
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
