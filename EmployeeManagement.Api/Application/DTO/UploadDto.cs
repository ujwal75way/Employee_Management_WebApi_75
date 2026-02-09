namespace EmployeeManagement.Api.Application.DTO
{
    public class UploadResult
    {
        public int SuccessCount { get; set; }
        public List<string> Errors { get; set; } = new();
    }
}