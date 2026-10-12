namespace eLibrary.Common;

public static class AuthInputValidator
{
    private const string SpecialCharacters = "!@#$%^&*()_+-=[]{}|;':\",./<>?";

    public static string? ValidatePassword(string? password)
    {
        if (string.IsNullOrWhiteSpace(password))
            return "Password is required.";

        if (password.Length < 8 || password.Length > 12)
            return "Password must be 8-12 characters long.";

        if (!password.Any(character => character is >= 'A' and <= 'Z'))
            return "Password must contain at least one uppercase letter.";

        if (!password.Any(character => character is >= 'a' and <= 'z'))
            return "Password must contain at least one lowercase letter.";

        if (!password.Any(character => character is >= '0' and <= '9'))
            return "Password must contain at least one number.";

        if (!password.Any(character => SpecialCharacters.Contains(character)))
            return "Password must contain at least one special character.";

        return null;
    }

    public static bool IsValidSixDigitCode(string? code) =>
        code is { Length: 6 } && code.All(character => character is >= '0' and <= '9');
}
