namespace EmployeeManagement.Api.Application.DTO
{
    public class CreateEmployeeDto
    {
        public string Name { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}