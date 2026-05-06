using System;
using System.Collections.Generic;
using System.Linq;
using AnalyticsModuleDiplomMVC.Models;
using Npgsql;
using Microsoft.Extensions.Configuration;

public class DataService
{
    private readonly IConfiguration _configuration;
    private bool _useMockData = false;

    public List<Employee> Employees { get; private set; }
    public List<AttendanceRecord> AttendanceRecords { get; private set; }
    public List<WorkSchedule> WorkSchedules { get; private set; }
    public List<ActionLog> ActionLogs { get; private set; }

    public DataService(IConfiguration configuration)
    {
        _configuration = configuration;
        InitializeData();
    }

    private void InitializeData()
    {
        try
        {
            // Пробуем подключиться к PostgreSQL
            using (var connection = new NpgsqlConnection(_configuration.GetConnectionString("PostgresConnection")))
            {
                connection.Open();
                Console.WriteLine("Успешное подключение к базе данных PostgreSQL.");

                // Загружаем данные из базы данных
                Employees = GetEmployeesFromDatabase(connection);
                //WorkSchedules = GetWorkSchedulesFromDatabase(connection);
                //AttendanceRecords = GetAttendanceRecordsFromDatabase(connection);
                //ActionLogs = GetActionLogsFromDatabase(connection);
                WorkSchedules = GenerateWorkSchedules(Employees);
                AttendanceRecords = GenerateAttendanceRecords(Employees, WorkSchedules);
                ActionLogs = new List<ActionLog>();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка подключения к базе данных: {ex.Message}. Используются тестовые данные.");
            _useMockData = true;

            // Используем тестовые данные
            Employees = GenerateEmployees();
            //WorkSchedules = GenerateWorkSchedules(Employees);
            //AttendanceRecords = GenerateAttendanceRecords(Employees, WorkSchedules);
            //ActionLogs = new List<ActionLog>();
        }
    }
    
    public void AddEmployee(Employee employee)
    {
        if (employee == null)
            throw new ArgumentNullException(nameof(employee), "Сотрудник не может быть null");
 
        if (string.IsNullOrWhiteSpace(employee.FullName))
            throw new ArgumentException("ФИО сотрудника не может быть пустым", nameof(employee.FullName));
 
        if (string.IsNullOrWhiteSpace(employee.Position))
            throw new ArgumentException("Должность не может быть пустой", nameof(employee.Position));
 
        if (string.IsNullOrWhiteSpace(employee.Department))
            throw new ArgumentException("Отдел не может быть пустым", nameof(employee.Department));
 
        try
        {
            if (_useMockData)
            {
                // Если используются тестовые данные, добавляем в список
                employee.Id = Employees.Count > 0 ? Employees.Max(e => e.Id) + 1 : "1";
                Employees.Add(employee);
                Console.WriteLine($"Сотрудник {employee.FullName} добавлен в список тестовых данных.");
            }
            else
            {
                // Добавляем в базу данных PostgreSQL
                AddEmployeeToDatabase(employee);
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Ошибка при добавлении сотрудника: {ex.Message}", ex);
        }
    }
    
    private void AddEmployeeToDatabase(Employee employee)
    {
        try
        {
            using (var connection = new NpgsqlConnection(_configuration.GetConnectionString("PostgresConnection")))
            {
                connection.Open();
 
                var query = @"INSERT INTO employees (full_name, position, department) 
                            VALUES (@fullName, @position, @department) 
                            RETURNING id";
 
                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@fullName", employee.FullName);
                    command.Parameters.AddWithValue("@position", employee.Position);
                    command.Parameters.AddWithValue("@department", employee.Department);
 
                    var result = command.ExecuteScalar();
                    if (result != null)
                    {
                        employee.Id = Convert.ToString(result);
                        Console.WriteLine($"Сотрудник {employee.FullName} успешно добавлен в БД с ID: {employee.Id}");
 
                        // Перезагружаем список сотрудников из БД
                        Employees = GetEmployeesFromDatabase(connection);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Ошибка при добавлении сотрудника в БД: {ex.Message}", ex);
        }
    }
    
    private List<Employee> GetEmployeesFromDatabase(NpgsqlConnection connection)
    {
        var employees = new List<Employee>();
        var query = "SELECT id, name FROM employee";

        using (var command = new NpgsqlCommand(query, connection))
        using (var reader = command.ExecuteReader())
        {
            while (reader.Read())
            {
                employees.Add(new Employee
                {
                    Id = reader.GetGuid(0).ToString(),
                    FullName = reader.GetString(1)
                });
            }
        }

        return employees;
    }

    private List<WorkSchedule> GetWorkSchedulesFromDatabase(NpgsqlConnection connection)
    {
        var schedules = new List<WorkSchedule>();
        var query = "SELECT id, employee_id, start_time, end_time, shift_type FROM work_schedules";

        using (var command = new NpgsqlCommand(query, connection))
        using (var reader = command.ExecuteReader())
        {
            while (reader.Read())
            {
                schedules.Add(new WorkSchedule
                {
                    Id = reader.GetGuid(0).ToString(),
                    EmployeeId = reader.GetGuid(1).ToString(),
                    StartTime = reader.GetDateTime(2),
                    EndTime = reader.GetDateTime(3),
                    ShiftType = reader.GetString(4)
                });
            }
        }

        return schedules;
    }

    private List<AttendanceRecord> GetAttendanceRecordsFromDatabase(NpgsqlConnection connection)
    {
        var records = new List<AttendanceRecord>();
        var query = "SELECT id, employee_id, date, arrival_time, departure_time, status FROM attendance_records";

        using (var command = new NpgsqlCommand(query, connection))
        using (var reader = command.ExecuteReader())
        {
            while (reader.Read())
            {
                records.Add(new AttendanceRecord
                {
                    Id = reader.GetGuid(0).ToString(),
                    EmployeeId = reader.GetGuid(1).ToString(),
                    Date = reader.GetDateTime(2),
                    ArrivalTime = reader.IsDBNull(3) ? (DateTime?)null : reader.GetDateTime(3),
                    DepartureTime = reader.IsDBNull(4) ? (DateTime?)null : reader.GetDateTime(4),
                    Status = (AttendanceStatus)reader.GetInt32(5)
                });
            }
        }

        return records;
    }

    private List<ActionLog> GetActionLogsFromDatabase(NpgsqlConnection connection)
    {
        var logs = new List<ActionLog>();
        var query = "SELECT id, action_date, action_type, user_name, description FROM action_logs";

        using (var command = new NpgsqlCommand(query, connection))
        using (var reader = command.ExecuteReader())
        {
            while (reader.Read())
            {
                logs.Add(new ActionLog
                {
                    Id = reader.GetInt32(0),
                    ActionDate = reader.GetDateTime(1),
                    ActionType = reader.GetString(2),
                    User = reader.GetString(3),
                    Description = reader.GetString(4)
                });
            }
        }

        return logs;
    }

    private List<Employee> GenerateEmployees()
    {
        return new List<Employee>
        {
            new Employee { Id = "1", FullName = "Иванов Иван Иванович", Position = "Менеджер", Department = "Отдел продаж" },
            new Employee { Id = "2", FullName = "Петров Петр Петрович", Position = "Инженер", Department = "Производство" },
            new Employee { Id = "3", FullName = "Сидорова Анна Сергеевна", Position = "Бухгалтер", Department = "Финансы" }
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

                if (random.Next(0, 10) < 2)
                {
                    records.Add(new AttendanceRecord
                    {
                        Id = (records.Count + 1).ToString(),
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
                    Id = (records.Count + 1).ToString(),
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
    
    public void AddWorkSchedule(WorkSchedule schedule, int workingDays = 5, int daysOff = 2)
    {
        if (schedule == null)
            throw new ArgumentNullException(nameof(schedule));

        //if (schedule.EmployeeId <= 0)
        //    throw new ArgumentException("ID сотрудника должен быть больше 0");

        if (workingDays < 1 || workingDays > 7)
            throw new ArgumentException("Количество рабочих дней должно быть от 1 до 7");

        if (daysOff < 0 || daysOff > 7)
            throw new ArgumentException("Количество выходных дней должно быть от 0 до 7");

        try
        {
            if (_useMockData)
            {
                schedule.Id = WorkSchedules.Count > 0 ? WorkSchedules.Max(s => s.Id) + 1 : "1";
                WorkSchedules.Add(schedule);
                Console.WriteLine($"Расписание для сотрудника {schedule.EmployeeId} добавлено в тестовые данные.");
            }
            else
            {
                AddWorkScheduleToDatabase(schedule, workingDays, daysOff);
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Ошибка при добавлении расписания: {ex.Message}", ex);
        }
    }

    private void AddWorkScheduleToDatabase(WorkSchedule schedule, int workingDays, int daysOff)
    {
        try
        {
            using (var connection = new NpgsqlConnection(_configuration.GetConnectionString("PostgresConnection")))
            {
                connection.Open();

                // Проверяем, существует ли уже такой тип смены
                var checkQuery = "SELECT id FROM schedule_type WHERE name = @name LIMIT 1";
                Guid scheduleTypeId = Guid.Empty;
                bool typeExists = false;

                using (var command = new NpgsqlCommand(checkQuery, connection))
                {
                    command.Parameters.AddWithValue("@name", schedule.ShiftType);
                    var result = command.ExecuteScalar();
                    if (result != null)
                    {
                        scheduleTypeId = (Guid)result;
                        typeExists = true;
                    }
                }

                if (typeExists)
                {
                    // Обновляем существующий тип смены
                    var updateQuery = @"UPDATE schedule_type 
                                       SET working_days = @workingDays, 
                                           days_off = @daysOff,
                                           start_time = @startTime,
                                           end_time = @endTime
                                       WHERE id = @id";

                    using (var command = new NpgsqlCommand(updateQuery, connection))
                    {
                        command.Parameters.AddWithValue("@id", scheduleTypeId);
                        command.Parameters.AddWithValue("@workingDays", workingDays);
                        command.Parameters.AddWithValue("@daysOff", daysOff);
                        command.Parameters.AddWithValue("@startTime", schedule.StartTime.TimeOfDay);
                        command.Parameters.AddWithValue("@endTime", schedule.EndTime.TimeOfDay);

                        command.ExecuteNonQuery();
                        Console.WriteLine($"Расписание '{schedule.ShiftType}' успешно обновлено в БД");
                    }
                }
                else
                {
                    // Создаём новый тип смены
                    var insertQuery = @"INSERT INTO schedule_type (name, working_days, days_off, start_time, end_time) 
                                       VALUES (@name, @workingDays, @daysOff, @startTime, @endTime) 
                                       RETURNING id";

                    using (var command = new NpgsqlCommand(insertQuery, connection))
                    {
                        command.Parameters.AddWithValue("@name", schedule.ShiftType);
                        command.Parameters.AddWithValue("@workingDays", workingDays);
                        command.Parameters.AddWithValue("@daysOff", daysOff);
                        command.Parameters.AddWithValue("@startTime", schedule.StartTime.TimeOfDay);
                        command.Parameters.AddWithValue("@endTime", schedule.EndTime.TimeOfDay);

                        var result = command.ExecuteScalar();
                        if (result != null)
                        {
                            scheduleTypeId = (Guid)result;
                            Console.WriteLine($"Расписание '{schedule.ShiftType}' успешно добавлено в БД");
                        }
                    }
                }

                var updateEmployeeQuery = @"UPDATE employee 
                                       SET schedule_type_id = @scheduleTypeId
                                       WHERE id = (
                                           SELECT id FROM employee 
                                           ORDER BY id 
                                           LIMIT 1 
                                           OFFSET @employeeOffset
                                       )";

                // Или если нужно обновить конкретного сотрудника по его порядковому номеру:
                var updateEmployeeByIdQuery = @"UPDATE employee 
                                           SET schedule_type_id = @scheduleTypeId
                                           WHERE id = (
                                               SELECT id FROM employee 
                                               ORDER BY id 
                                               LIMIT 1
                                           )";

                using (var command = new NpgsqlCommand(updateEmployeeByIdQuery, connection))
                {
                    command.Parameters.AddWithValue("@scheduleTypeId", scheduleTypeId);
                
                    int affectedRows = command.ExecuteNonQuery();
                    if (affectedRows > 0)
                    {
                        Console.WriteLine($"Сотрудник обновлен. schedule_type_id установлен на {scheduleTypeId}");
                    }
                    else
                    {
                        Console.WriteLine("Сотрудник не найден для обновления");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Ошибка при добавлении расписания в БД: {ex.Message}", ex);
        }
    }

    public bool IsDatabaseConnected()
    {
        return !_useMockData;
    }
}
