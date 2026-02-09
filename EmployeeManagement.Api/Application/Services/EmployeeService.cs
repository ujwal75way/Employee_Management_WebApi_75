using EmployeeManagement.Api.Application.Common;
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
                IsActive = employee.IsActive,
                IsDeleted = employee.IsDeleted
            };
        }

        private static Employee MapToEntity(CreateEmployeeDto dto)
        {
            return new Employee
            {
                Name = dto.Name,
                Department = dto.Department,
                Email = dto.Email,
                IsActive = dto.IsActive,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "Admin",
                UpdatedAt = DateTime.UtcNow,
                UpdatedBy = "Admin"
            };
        }

        private static void UpdateEntityFromDto(Employee employee, UpdateEmployeeDto dto)
        {
            employee.Name = dto.Name;
            employee.Department = dto.Department;
            employee.Email = dto.Email;
            employee.IsActive = dto.IsActive;
            employee.UpdatedAt = DateTime.UtcNow;
            employee.UpdatedBy = "Admin";
        }
        


        public async Task<List<EmployeeDto>> GetEmployees(string department, string status, string search)
        {
            var employees = await _repository.GetAllEmployee();
            var query = employees.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(department)){
                query = query.Where(e => e.Department.Equals(department, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(status)){
                var active = status.Equals("Active", StringComparison.OrdinalIgnoreCase);
                query = query.Where(e => e.IsActive == active);
            }

            if (!string.IsNullOrWhiteSpace(search)){
                query = query.Where(e => e.Name.Contains(search, StringComparison.OrdinalIgnoreCase));
            }

            var result = query.Select(MapToDto).ToList();

            if (!result.Any())
            {
                throw new Exception("Employee does not exist");
            }
            return result;
        }

        public async Task<EmployeeDto> GetById(int id)
        {
            var employee = await _repository.GetByIdEmployee(id);

            if (employee == null)
            {
                throw new Exception("Employee not found");
            }

            return MapToDto(employee);
        }

        public async Task<List<string>> GetAllDepartments()
        {
            return await _repository.GetAllDepartmentsEmployee();
        }

        public async Task<List<DepartmentCountDto>> GetDepartmentCounts()
        {
            var employees = await _repository.GetAllEmployee();
            var query = employees.AsEnumerable();

            return query
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
            if (string.IsNullOrWhiteSpace(val.Name)){
                throw new Exception("Name is required");
            }   
            if (string.IsNullOrWhiteSpace(val.Email) || !ValidationHelper.IsValidEmail(val.Email)){
                throw new Exception("Email is required");
            }
            if (string.IsNullOrWhiteSpace(val.Department)){
                throw new Exception("Department is required");
            }

            var userExists = await _repository.GetByEmail(val.Email);

            if (userExists != null)
            {
                throw new Exception("Email already exists");
            }

            var employee = MapToEntity(val);

            await _repository.AddEmployee(employee);
        }


        public async Task<bool> UpdateEmployee(UpdateEmployeeDto val)
        {
            var employee = await _repository.GetByIdEmployee(val.Id);

            if (employee == null)
            {
                throw new Exception("Employee not found");
            }
            if (string.IsNullOrWhiteSpace(val.Name)){
                throw new Exception("Name is required");
            }
            if (string.IsNullOrWhiteSpace(val.Email) || !ValidationHelper.IsValidEmail(val.Email)){
                throw new Exception("A valid email is required");
            }
            if (string.IsNullOrWhiteSpace(val.Department)){
                throw new Exception("Department is required");
            }
            if (employee.Email != val.Email)
            {
                var emailUsed = await _repository.GetByEmail(val.Email);
                if (emailUsed != null)
                {
                    throw new Exception("Email already exists");
                }
            }

            UpdateEntityFromDto(employee, val);

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


        public async Task<UploadResult> UploadEmployeesFromExcel(Stream excelStream, string fileName)
        {
            if (excelStream == null || excelStream.Length == 0){
                throw new Exception("Please upload a valid Excel file");
            }

            var extension = Path.GetExtension(fileName).ToLower();
            if (extension != ".xlsx"){
                throw new Exception("Invalid file type. Please upload an .xlsx file");
            }

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage(excelStream);

            var existingEmails = new HashSet<string>(
                await _repository.GetAllEmails(),
                StringComparer.OrdinalIgnoreCase
            );  
            var result = new UploadResult();
            var employeeBatch = new List<Employee>();
            const int BATCH_SIZE = 100;
            
            var worksheet = package.Workbook.Worksheets.FirstOrDefault();
            if (worksheet == null){
                throw new Exception("Excel file has no worksheets");
            }

            var rowCount = worksheet.Dimension.Rows;
            var columnCount = worksheet.Dimension.Columns;
            
            var columnMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            for (int col = 1; col <= columnCount; col++)
            {
                var header = worksheet.Cells[1, col].Value?.ToString()?.Trim();
                if (!string.IsNullOrWhiteSpace(header))
                {
                    columnMap[header.ToLower()] = col;
                }
            }
            
            string[] requiredHeaders = { "name", "department", "email", "isactive" };

            foreach (var header in requiredHeaders)
            {
                if (!columnMap.ContainsKey(header))
                {
                    throw new Exception($"Missing required column: {header}");
                }
            }
            
            for (int row = 2; row <= rowCount; row++)
            {
                var name = worksheet.Cells[row, columnMap["name"]].Value?.ToString()?.Trim();
                var department = worksheet.Cells[row, columnMap["department"]].Value?.ToString()?.Trim();
                var email = worksheet.Cells[row, columnMap["email"]].Value?.ToString()?.Trim();
                var isActiveStr = worksheet.Cells[row, columnMap["isactive"]].Value?.ToString()?.Trim();
                
                if (string.IsNullOrWhiteSpace(name))
                {
                    result.Errors.Add($"Row {row}: Name is required.");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(department))
                {
                    result.Errors.Add($"Row {row}: Department is required.");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(email))
                {
                    result.Errors.Add($"Row {row}: Email is required.");
                    continue;
                }

                if (!ValidationHelper.IsValidEmail(email))
                {
                    result.Errors.Add($"Row {row}: '{email}' is not a valid email address.");
                    continue;
                }
                
                if (existingEmails.Contains(email))
                {
                    result.Errors.Add($"Row {row}: Email '{email}' already exists or is duplicated in the file.");
                    continue;
                }

                existingEmails.Add(email);

                var employee = MapToEntity(new CreateEmployeeDto
                {
                    Name = name,
                    Department = department,
                    Email = email,
                    IsActive = bool.TryParse(isActiveStr, out var isActive) && isActive
                });

                employeeBatch.Add(employee);
                result.SuccessCount++;

                if (employeeBatch.Count >= BATCH_SIZE)
                {
                    await _repository.AddEmployeesRange(employeeBatch);
                    employeeBatch.Clear();
                }
            }

            if (employeeBatch.Any())
            {
                await _repository.AddEmployeesRange(employeeBatch);
            }

            return result;
        }

    }
}