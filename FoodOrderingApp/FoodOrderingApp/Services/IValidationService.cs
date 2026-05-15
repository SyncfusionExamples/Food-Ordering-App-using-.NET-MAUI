namespace FoodOrderingApp.Services;

/// <summary>
/// Provides comprehensive validation methods for user inputs across the application.
/// Supports password strength, email format, phone numbers, and custom validation.
/// </summary>
public interface IValidationService
{
    /// <summary>
    /// Validates email format according to RFC 5322 simplified rules
    /// </summary>
    /// <param name="email">Email to validate</param>
    /// <returns>True if email format is valid</returns>
    bool IsValidEmail(string? email);

    /// <summary>
    /// Validates password strength:
    /// - Minimum 8 characters
    /// - Optional: at least one uppercase letter
    /// - Optional: at least one digit
    /// - Optional: at least one special character
    /// </summary>
    /// <param name="password">Password to validate</param>
    /// <param name="requireUppercase">Whether uppercase is required (default: false)</param>
    /// <param name="requireDigits">Whether digits are required (default: false)</param>
    /// <param name="requireSpecialChar">Whether special characters are required (default: false)</param>
    /// <returns>Tuple with (isValid: bool, errorMessage: string)</returns>
    (bool isValid, string errorMessage) ValidatePassword(
        string? password,
        bool requireUppercase = false,
        bool requireDigits = false,
        bool requireSpecialChar = false);

    /// <summary>
    /// Validates phone number format (10 digits for Indian numbers)
    /// </summary>
    /// <param name="phoneNumber">Phone number to validate</param>
    /// <returns>True if phone number format is valid</returns>
    bool IsValidPhoneNumber(string? phoneNumber);

    /// <summary>
    /// Validates postal code format (Indian: 6 digits)
    /// </summary>
    /// <param name="postalCode">Postal code to validate</param>
    /// <returns>True if postal code format is valid</returns>
    bool IsValidPostalCode(string? postalCode);

    /// <summary>
    /// Validates that a string is not empty or whitespace
    /// </summary>
    /// <param name="value">String to validate</param>
    /// <param name="fieldName">Name of field (for error messages)</param>
    /// <returns>Tuple with (isValid: bool, errorMessage: string)</returns>
    (bool isValid, string errorMessage) ValidateRequired(string? value, string fieldName = "Field");

    /// <summary>
    /// Validates string length is within bounds
    /// </summary>
    /// <param name="value">String to validate</param>
    /// <param name="minLength">Minimum length (0 for no minimum)</param>
    /// <param name="maxLength">Maximum length (int.MaxValue for no maximum)</param>
    /// <param name="fieldName">Name of field (for error messages)</param>
    /// <returns>Tuple with (isValid: bool, errorMessage: string)</returns>
    (bool isValid, string errorMessage) ValidateLength(
        string? value,
        int minLength = 0,
        int maxLength = int.MaxValue,
        string fieldName = "Field");

    /// <summary>
    /// Validates that two passwords match
    /// </summary>
    /// <param name="password">First password</param>
    /// <param name="confirmPassword">Confirmation password</param>
    /// <returns>Tuple with (isValid: bool, errorMessage: string)</returns>
    (bool isValid, string errorMessage) ValidatePasswordMatch(string? password, string? confirmPassword);

    /// <summary>
    /// Validates URL format
    /// </summary>
    /// <param name="url">URL to validate</param>
    /// <returns>True if URL format is valid</returns>
    bool IsValidUrl(string? url);

    /// <summary>
    /// Validates latitude coordinate (-90 to 90)
    /// </summary>
    /// <param name="latitude">Latitude value</param>
    /// <returns>True if latitude is valid</returns>
    bool IsValidLatitude(double latitude);

    /// <summary>
    /// Validates longitude coordinate (-180 to 180)
    /// </summary>
    /// <param name="longitude">Longitude value</param>
    /// <returns>True if longitude is valid</returns>
    bool IsValidLongitude(double longitude);

    /// <summary>
    /// Validates coordinate pair for geographic validity
    /// </summary>
    /// <param name="latitude">Latitude value</param>
    /// <param name="longitude">Longitude value</param>
    /// <returns>True if coordinate pair is valid</returns>
    bool IsValidCoordinate(double latitude, double longitude);
}
