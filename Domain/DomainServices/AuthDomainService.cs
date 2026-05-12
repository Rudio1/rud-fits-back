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
            throw new ArgumentException("E-mail é obrigatório.", nameof(normalizedEmail));
        }

        if (!EmailRegex().IsMatch(normalizedEmail))
        {
            throw new ArgumentException("O formato do e-mail é inválido.", nameof(normalizedEmail));
        }

        if (string.IsNullOrEmpty(password) || password.Length < 8)
        {
            throw new ArgumentException("A senha deve ter no mínimo 8 caracteres.", nameof(password));
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
            throw new ArgumentException("Nome completo é obrigatório.", nameof(fullName));
        }

        if (fullName.Length > 120)
        {
            throw new ArgumentException("Nome completo deve ter no máximo 120 caracteres.", nameof(fullName));
        }

        EnsureEmailAndPassword(normalizedEmail, password);

        if (normalizedUsername != null)
        {
            if (normalizedUsername.Length < 3 || normalizedUsername.Length > 50)
            {
                throw new ArgumentException("Username deve ter entre 3 e 50 caracteres.", nameof(normalizedUsername));
            }

            if (!UsernameRegex().IsMatch(normalizedUsername))
            {
                throw new ArgumentException(
                    "Username pode conter apenas letras minúsculas, números e underscore.",
                    nameof(normalizedUsername));
            }
        }
    }

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled | RegexOptions.CultureInvariant)]
    private static partial Regex EmailRegex();

    [GeneratedRegex(@"^[a-z0-9_]+$", RegexOptions.Compiled | RegexOptions.CultureInvariant)]
    private static partial Regex UsernameRegex();
}
