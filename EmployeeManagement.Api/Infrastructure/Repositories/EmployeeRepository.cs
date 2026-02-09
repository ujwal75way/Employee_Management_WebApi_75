using EmployeeManagement.Api.Infrastructure.Data;
using EmployeeManagement.Api.Domain.Entities;
using EmployeeManagement.Api.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagement.Api.Infrastructure.Repositories
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly AppDbContext _context;

        public EmployeeRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Employee>> GetAllEmployee()
        {
            return await _context.Employees
                .Where(e => !e.IsDeleted)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Employee?> GetByIdEmployee(int id)
        {
            return await _context.Employees
                .FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);
        }

        public async Task<Employee?> GetByEmail(string email)
        {
            return await _context.Employees
                .FirstOrDefaultAsync(e => e.Email == email && !e.IsDeleted);
        }

        public async Task<List<string>> GetAllDepartmentsEmployee()
        {
            return await _context.Employees
                .Select(e => e.Department)
                .Distinct()
                .ToListAsync();
        }

        public async Task AddEmployee(Employee employee)
        {
            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();
        }

        public async Task<HashSet<string>> GetAllEmails()
        {
            var emails = await _context.Employees
                .Select(e => e.Email)
                .ToListAsync();
            
            return new HashSet<string>(emails);
        }


        public async Task AddEmployeesRange(List<Employee> employees)
        {
            _context.Employees.AddRange(employees);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateEmployee(Employee employee)
        {
            _context.Employees.Update(employee);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteEmployee(Employee employee)
        {
            employee.IsDeleted = true;
            await _context.SaveChangesAsync();
        }
    }
}