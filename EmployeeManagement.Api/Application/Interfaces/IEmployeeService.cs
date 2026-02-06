
using EmployeeManagement.Api.Application.DTO;

namespace  EmployeeManagement.Api.Application.Interfaces
{
    public interface IEmployeeService
    {
        Task<List<EmployeeDto>> GetEmployees(string department, string status, string search);
        Task<EmployeeDto?> GetById(int id);
        Task<List<DepartmentCountDto>> GetDepartmentCounts();
        Task<List<string>> GetAllDepartments();
        
        
        Task CreateEmployee(CreateEmployeeDto val);
        Task<bool> UpdateEmployee(UpdateEmployeeDto val);
        Task<bool> DeleteEmployee(int id);
        Task<int> UploadEmployeesFromExcel(Stream excelStream);
    }
}