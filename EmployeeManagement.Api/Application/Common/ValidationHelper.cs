using System.Net.Mail;

namespace EmployeeManagement.Api.Application.Common
{
    public static class ValidationHelper
    {
        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            return MailAddress.TryCreate(email, out _);
        }
    }
}
