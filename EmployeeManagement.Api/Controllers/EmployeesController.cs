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
        // Sir, we cannot use string.Empty here because default parameter values must be compile time constants, string.Empty is a static readonly field whose value is assigned at runtime, whereas "" is a string literal, and string literals are compile time constants
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
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var employee = await _employeeService.GetById(id);
                return Ok(employee);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
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
                return BadRequest(new { message = ex.Message });
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
                return BadRequest(new { message = ex.Message });
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
                return BadRequest(new { message = ex.Message });
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
                return BadRequest(new { message = ex.Message });
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


        [HttpPost("upload-excel")]
        public async Task<IActionResult> UploadExcel(IFormFile file)
        {
            try
            {
                using var stream = file.OpenReadStream();
                var result = await _employeeService.UploadEmployeesFromExcel(stream, file.FileName);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
