using System.Text.RegularExpressions;

namespace FoodOrderingApp.Services;

public class ValidationService : IValidationService
{
    /// <summary>
    /// Validates email format using regex pattern
    /// Pattern allows: alphanumeric, dots, hyphens, underscores before @, domain with dots
    /// </summary>
    public bool IsValidEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        try
        {
            var pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Regex.IsMatch(email.Trim(), pattern, RegexOptions.IgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Validates password strength with configurable requirements
    /// Default: minimum 8 characters
    /// Optional: uppercase, digits, special characters
    /// </summary>
    public (bool isValid, string errorMessage) ValidatePassword(
        string? password,
        bool requireUppercase = false,
        bool requireDigits = false,
        bool requireSpecialChar = false)
    {
        if (string.IsNullOrWhiteSpace(password))
            return (false, "Password is required");

        if (password.Length < 8)
            return (false, "Password must be at least 8 characters long");

        if (requireUppercase && !Regex.IsMatch(password, @"[A-Z]"))
            return (false, "Password must contain at least one uppercase letter");

        if (requireDigits && !Regex.IsMatch(password, @"[0-9]"))
            return (false, "Password must contain at least one digit");

        if (requireSpecialChar && !Regex.IsMatch(password, @"[!@#$%^&*()_+\-=\[\]{};':"",.<>?/\\|`~]"))
            return (false, "Password must contain at least one special character");

        return (true, string.Empty);
    }

    /// <summary>
    /// Validates Indian phone number format (10 digits)
    /// </summary>
    public bool IsValidPhoneNumber(string? phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return false;

        try
        {
            // Remove any non-digit characters
            var digitsOnly = Regex.Replace(phoneNumber, @"\D", string.Empty);

            // Indian phone numbers: 10 digits
            return digitsOnly.Length == 10 && Regex.IsMatch(digitsOnly, @"^[6-9]\d{9}$");
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Validates Indian postal code format (6 digits)
    /// </summary>
    public bool IsValidPostalCode(string? postalCode)
    {
        if (string.IsNullOrWhiteSpace(postalCode))
            return false;

        try
        {
            // Indian postal codes: 6 digits
            return Regex.IsMatch(postalCode.Trim(), @"^\d{6}$");
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Validates that a field is not empty or whitespace
    /// </summary>
    public (bool isValid, string errorMessage) ValidateRequired(string? value, string fieldName = "Field")
    {
        if (string.IsNullOrWhiteSpace(value))
            return (false, $"{fieldName} is required");

        return (true, string.Empty);
    }

    /// <summary>
    /// Validates string length within specified bounds
    /// </summary>
    public (bool isValid, string errorMessage) ValidateLength(
        string? value,
        int minLength = 0,
        int maxLength = int.MaxValue,
        string fieldName = "Field")
    {
        if (string.IsNullOrEmpty(value))
            return (false, $"{fieldName} is required");

        var length = value.Length;

        if (length < minLength)
            return (false, $"{fieldName} must be at least {minLength} characters");

        if (length > maxLength)
            return (false, $"{fieldName} must not exceed {maxLength} characters");

        return (true, string.Empty);
    }

    /// <summary>
    /// Validates that two passwords match (case-sensitive)
    /// </summary>
    public (bool isValid, string errorMessage) ValidatePasswordMatch(string? password, string? confirmPassword)
    {
        if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirmPassword))
            return (false, "Both password fields are required");

        if (password != confirmPassword)
            return (false, "Passwords do not match");

        return (true, string.Empty);
    }

    /// <summary>
    /// Validates URL format (HTTP, HTTPS, FTP schemes)
    /// </summary>
    public bool IsValidUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return false;

        try
        {
            var pattern = @"^(https?|ftp)://[^\s/$.?#].[^\s]*$";
            return Regex.IsMatch(url.Trim(), pattern, RegexOptions.IgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Validates latitude coordinate must be between -90 and 90
    /// </summary>
    public bool IsValidLatitude(double latitude)
    {
        return latitude >= -90 && latitude <= 90;
    }

    /// <summary>
    /// Validates longitude coordinate must be between -180 and 180
    /// </summary>
    public bool IsValidLongitude(double longitude)
    {
        return longitude >= -180 && longitude <= 180;
    }

    /// <summary>
    /// Validates both latitude and longitude for geographic validity
    /// </summary>
    public bool IsValidCoordinate(double latitude, double longitude)
    {
        return IsValidLatitude(latitude) && IsValidLongitude(longitude);
    }
}
