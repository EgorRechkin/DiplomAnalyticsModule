using AnalyticsModuleDiplomMVC.Models;

namespace AnalyticsModuleDiplomMVC.Services;

using System;
using System.Collections.Generic;
using System.Linq;

public class MockDataService
{
    public List<Employee> Employees { get; }
    public List<AttendanceMark> AttendanceMarks { get; }
    public List<AttendanceRecord> AttendanceRecords { get; }
    public List<WorkSchedule> WorkSchedules { get; }
    public List<ActionLog> ActionLogs { get; }

    public MockDataService()
    {
        Employees = GenerateEmployees();
        WorkSchedules = GenerateWorkSchedules(Employees);
        AttendanceMarks = GenerateAttendanceMarks(Employees, WorkSchedules);
        AttendanceRecords = GenerateAttendanceRecords(Employees, WorkSchedules);
        ActionLogs = new List<ActionLog>();
    }

    private List<Employee> GenerateEmployees()
    {
        return new List<Employee>
        {
            new Employee { Id = 1, FullName = "Иванов Иван Иванович", Position = "Менеджер", Department = "Отдел продаж" },
            new Employee { Id = 2, FullName = "Петров Петр Петрович", Position = "Инженер", Department = "Производство" },
            new Employee { Id = 3, FullName = "Сидорова Анна Сергеевна", Position = "Бухгалтер", Department = "Финансы" }
        };
    }

    private List<WorkSchedule> GenerateWorkSchedules(List<Employee> employees)
    {
        return employees.Select(emp => new WorkSchedule
        {
            Id = emp.Id,
            EmployeeId = emp.Id,
            StartTime = DateTime.Today.AddHours(9),
            EndTime = DateTime.Today.AddHours(18),
            ShiftType = "День"
        }).ToList();
    }

    private List<AttendanceMark> GenerateAttendanceMarks(List<Employee> employees, List<WorkSchedule> schedules)
    {
        var marks = new List<AttendanceMark>();
        var random = new Random();

        foreach (var emp in employees)
        {
            var schedule = schedules.First(s => s.EmployeeId == emp.Id);

            for (int day = -7; day <= 0; day++)
            {
                var date = DateTime.Today.AddDays(day);
                var arrivalTime = schedule.StartTime.AddMinutes(random.Next(-30, 30));
                var departureTime = schedule.EndTime.AddMinutes(random.Next(-30, 30));

                string status;
                if (arrivalTime > schedule.StartTime.AddMinutes(15))
                    status = "Опоздание";
                else if (departureTime > schedule.EndTime.AddMinutes(30))
                    status = "Переработка";
                else
                    status = "Норма";

                marks.Add(new AttendanceMark
                {
                    Id = marks.Count + 1,
                    EmployeeId = emp.Id,
                    Date = date,
                    ArrivalTime = arrivalTime,
                    DepartureTime = departureTime,
                    Status = status
                });
            }
        }

        return marks;
    }
    private List<AttendanceRecord> GenerateAttendanceRecords(List<Employee> employees, List<WorkSchedule> schedules)
    {
        var records = new List<AttendanceRecord>();
        var random = new Random();

        foreach (var emp in employees)
        {
            var schedule = schedules.First(s => s.EmployeeId == emp.Id);

            for (int day = -14; day <= 0; day++)
            {
                var date = DateTime.Today.AddDays(day);

                // Имитация пропусков
                if (random.Next(0, 10) < 2)
                {
                    records.Add(new AttendanceRecord
                    {
                        Id = records.Count + 1,
                        EmployeeId = emp.Id,
                        Date = date,
                        ArrivalTime = null,
                        DepartureTime = null,
                        Status = AttendanceStatus.Absent
                    });
                    continue;
                }

                var arrivalTime = schedule.StartTime.AddMinutes(random.Next(-30, 60));
                var departureTime = schedule.EndTime.AddMinutes(random.Next(-30, 60));

                var status = AttendanceStatus.Normal;

                if (arrivalTime > schedule.StartTime.AddMinutes(15))
                    status = AttendanceStatus.Late;
                else if (departureTime > schedule.EndTime.AddHours(1))
                    status = AttendanceStatus.Overwork;

                records.Add(new AttendanceRecord
                {
                    Id = records.Count + 1,
                    EmployeeId = emp.Id,
                    Date = date,
                    ArrivalTime = arrivalTime,
                    DepartureTime = departureTime,
                    Status = status
                });
            }
        }

        return records;
    }

}
