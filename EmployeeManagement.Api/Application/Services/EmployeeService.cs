using EmployeeManagement.Api.Application.DTO;
using EmployeeManagement.Api.Application.Interfaces;
using EmployeeManagement.Api.Domain.Entities;
using OfficeOpenXml;

namespace EmployeeManagement.Api.Application.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IEmployeeRepository _repository;

        public EmployeeService(IEmployeeRepository repository)
        {
            _repository = repository;
        }

        private static EmployeeDto MapToDto(Employee employee)
        {
            return new EmployeeDto
            {
                Id = employee.Id,
                Name = employee.Name,
                Department = employee.Department,
                Email = employee.Email,
                IsActive = employee.IsActive
            };
        }

        public async Task<List<EmployeeDto>> GetEmployees(string department, string status, string search)
        {
            var employees = await _repository.GetAllEmployee();
            var query = employees.AsQueryable();

            if (!string.IsNullOrWhiteSpace(department))
                query = query.Where(e => e.Department == department);

            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(e => e.IsActive == (status == "Active"));

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(e => e.Name.Contains(search));

            return query.Select(MapToDto).ToList();
        }

        public async Task<EmployeeDto?> GetById(int id)
        {
            var employee = await _repository.GetByIdEmployee(id);
            return employee == null ? null : MapToDto(employee);
        }

        public async Task<List<string>> GetAllDepartments()
        {
            return await _repository.GetAllDepartmentsEmployee();
        }

        public async Task<List<DepartmentCountDto>> GetDepartmentCounts()
        {
            var employees = await _repository.GetAllEmployee();

            return employees
                .Where(e => !e.IsDeleted)
                .GroupBy(e => e.Department)
                .Select(g => new DepartmentCountDto
                {
                    Department = g.Key,
                    TotalEmployees = g.Count()
                })
                .ToList();
        }

        public async Task CreateEmployee(CreateEmployeeDto val)
        {
            if (string.IsNullOrWhiteSpace(val.Name) ||
                string.IsNullOrWhiteSpace(val.Email) ||
                string.IsNullOrWhiteSpace(val.Department))
            {
                throw new Exception("Invalid employee data");
            }

            var exists = await _repository.GetByEmail(val.Email);
            if (exists != null)
            {
                throw new Exception("Email already exists");
            }

            var employee = new Employee
            {
                Name = val.Name,
                Department = val.Department,
                Email = val.Email,
                IsActive = val.IsActive,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "Admin",
                UpdatedAt = DateTime.UtcNow,
                UpdatedBy = "Admin"
            };

            await _repository.AddEmployee(employee);
        }

        public async Task<bool> UpdateEmployee(UpdateEmployeeDto val)
        {
            var employee = await _repository.GetByIdEmployee(val.Id);
            if (employee == null)
            {
                throw new Exception("Employee not found");
            }

            if (employee.Email != val.Email)
            {
                var emailUsed = await _repository.GetByEmail(val.Email);
                if (emailUsed != null)
                {
                    throw new Exception("Email already exists");
                }
            }

            employee.Name = val.Name;
            employee.Department = val.Department;
            employee.Email = val.Email;
            employee.IsActive = val.IsActive;
            employee.UpdatedAt = DateTime.UtcNow;
            employee.UpdatedBy = "Admin";

            await _repository.UpdateEmployee(employee);
            return true;
        }

        public async Task<bool> DeleteEmployee(int id)
        {
            var employee = await _repository.GetByIdEmployee(id);
            if (employee == null)
            {
                throw new Exception("Employee not found");
            }

            await _repository.DeleteEmployee(employee);
            return true;
        }

        public async Task<int> UploadEmployeesFromExcel(Stream excelStream)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage(excelStream);
            var worksheet = package.Workbook.Worksheets[0];
            var rowCount = worksheet.Dimension.Rows;
            var processedCount = 0;

            for (int row = 2; row <= rowCount; row++)
            {
                var name = worksheet.Cells[row, 1].Value?.ToString();
                var department = worksheet.Cells[row, 2].Value?.ToString();
                var email = worksheet.Cells[row, 3].Value?.ToString();
                var isActiveStr = worksheet.Cells[row, 4].Value?.ToString();

                if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email) ||
                    string.IsNullOrWhiteSpace(department)) continue;

                var existingEmployee = await _repository.GetByEmail(email);
                if (existingEmployee != null) continue;

                var employee = new Employee
                {
                    Name = name,
                    Department = department,
                    Email = email,
                    IsActive = bool.TryParse(isActiveStr, out var isActive) && isActive,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "Admin",
                    UpdatedAt = DateTime.UtcNow,
                    UpdatedBy = "Admin"
                };

                await _repository.AddEmployee(employee);
                processedCount++;
            }

            return processedCount;
        }

    }
}