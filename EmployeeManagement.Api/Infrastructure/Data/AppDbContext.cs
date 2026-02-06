using EmployeeManagement.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;


namespace EmployeeManagement.Api.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Employee> Employees { get; set; }
    }
}