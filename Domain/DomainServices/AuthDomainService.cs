using System.Text.RegularExpressions;

namespace RudFitAI.Domain.DomainServices;

public sealed partial class AuthDomainService
{
    public string NormalizeEmail(string email)
    {
        return email.Trim().ToLowerInvariant();
    }

    public string? NormalizeUsername(string? username)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            return null;
        }

        return username.Trim().ToLowerInvariant();
    }

    public void EnsureEmailAndPassword(string normalizedEmail, string password)
    {
        if (string.IsNullOrWhiteSpace(normalizedEmail))
        {
            throw new ArgumentException("Email is required.", nameof(normalizedEmail));
        }

        if (!EmailRegex().IsMatch(normalizedEmail))
        {
            throw new ArgumentException("Email format is invalid.", nameof(normalizedEmail));
        }

        if (string.IsNullOrEmpty(password) || password.Length < 8)
        {
            throw new ArgumentException("Password must be at least 8 characters.", nameof(password));
        }
    }

    public void EnsureRegistration(
        string fullName,
        string normalizedEmail,
        string? normalizedUsername,
        string password)
    {
        if (string.IsNullOrWhiteSpace(fullName))
        {
            throw new ArgumentException("Full name is required.", nameof(fullName));
        }

        if (fullName.Length > 120)
        {
            throw new ArgumentException("Full name must be at most 120 characters.", nameof(fullName));
        }

        EnsureEmailAndPassword(normalizedEmail, password);

        if (normalizedUsername != null)
        {
            if (normalizedUsername.Length < 3 || normalizedUsername.Length > 50)
            {
                throw new ArgumentException("Username must be between 3 and 50 characters.", nameof(normalizedUsername));
            }

            if (!UsernameRegex().IsMatch(normalizedUsername))
            {
                throw new ArgumentException(
                    "Username may only contain lowercase letters, numbers, and underscores.",
                    nameof(normalizedUsername));
            }
        }
    }

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled | RegexOptions.CultureInvariant)]
    private static partial Regex EmailRegex();

    [GeneratedRegex(@"^[a-z0-9_]+$", RegexOptions.Compiled | RegexOptions.CultureInvariant)]
    private static partial Regex UsernameRegex();
}
