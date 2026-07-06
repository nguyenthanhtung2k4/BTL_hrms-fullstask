namespace Hrms.Shared.Security;

public static class PasswordHasher
{
    public static string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    public static bool VerifyPassword(string password, string hashedPassword)
    {
        return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
    }

    public static (bool IsValid, string Message) ValidatePassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            return (false, "Mật khẩu không được để trống.");

        if (password.Length < 8)
            return (false, "Mật khẩu phải chứa ít nhất 8 ký tự.");

        if (!System.Linq.Enumerable.Any(password, char.IsUpper))
            return (false, "Mật khẩu phải chứa ít nhất một chữ cái in hoa (A-Z).");

        if (!System.Linq.Enumerable.Any(password, char.IsLower))
            return (false, "Mật khẩu phải chứa ít nhất một chữ cái thường (a-z).");

        if (!System.Linq.Enumerable.Any(password, char.IsDigit))
            return (false, "Mật khẩu phải chứa ít nhất một chữ số (0-9).");

        string specialCharacters = @"%!@#$%^&*()_+{}|:""<>?`\-=\[\]\\;',./";
        if (!System.Linq.Enumerable.Any(password, c => specialCharacters.Contains(c)))
            return (false, "Mật khẩu phải chứa ít nhất một ký tự đặc biệt (ví dụ: @, #, $, %, v.v.).");

        return (true, string.Empty);
    }
}
