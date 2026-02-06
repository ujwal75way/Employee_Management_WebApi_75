using EmployeeManagement.Api.Application.DTO;
using EmployeeManagement.Api.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagement.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmployeesController : ControllerBase
    {
        private readonly IEmployeeService _employeeService;

        public EmployeesController(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        [HttpGet]
        public async Task<IActionResult> GetEmployees(
            [FromQuery] string department = "",
            [FromQuery] string status = "",
            [FromQuery] string search = "")
        {
            try
            {
                var employees = await _employeeService.GetEmployees(department, status, search);
                return Ok(employees);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var employee = await _employeeService.GetById(id);
                if (employee == null)
                    return BadRequest("Employee not found");

                return Ok(employee);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("departments")]
        public async Task<IActionResult> GetDepartments()
        {
            try
            {
                var departments = await _employeeService.GetAllDepartments();
                return Ok(departments);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("department-count")]
        public async Task<IActionResult> GetDepartmentCount()
        {
            try
            {
                var result = await _employeeService.GetDepartmentCounts();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateEmployeeDto dto)
        {
            try
            {
                await _employeeService.CreateEmployee(dto);
                return Ok("Employee created");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut]
        public async Task<IActionResult> Update(UpdateEmployeeDto dto)
        {
            try
            {
                await _employeeService.UpdateEmployee(dto);
                return Ok("Employee updated");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _employeeService.DeleteEmployee(id);
                return Ok(new { message = "Employee deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // POST: api/employees/upload-excel
        [HttpPost("upload-excel")]
        public async Task<IActionResult> UploadExcel(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "Please upload a valid Excel file" });

            var extension = Path.GetExtension(file.FileName).ToLower();
            if (extension != ".xlsx")
                return BadRequest(new { message = "Invalid file type. Please upload an .xlsx file" });

            try
            {
                using var stream = file.OpenReadStream();
                var count = await _employeeService.UploadEmployeesFromExcel(stream);
                return Ok(new { message = $"Successfully processed {count} records" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Error processing Excel file: {ex.Message}" });
            }
        }
    }
}
