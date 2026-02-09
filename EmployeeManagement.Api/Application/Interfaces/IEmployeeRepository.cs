using System.Collections.Generic;
using EmployeeManagement.Api.Domain.Entities;

namespace EmployeeManagement.Api.Application.Interfaces
{

    public interface IEmployeeRepository
    {
        Task<List<Employee>> GetAllEmployee();
        Task<Employee?> GetByIdEmployee(int id);
        Task<Employee?> GetByEmail(string email);
        Task<HashSet<string>> GetAllEmails();
        Task<List<string>> GetAllDepartmentsEmployee();


        Task AddEmployee(Employee employee);
        Task AddEmployeesRange(List<Employee> employees);
        Task UpdateEmployee(Employee employee);
        Task DeleteEmployee(Employee employee);
    }
}