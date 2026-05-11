using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using RudFitAI.Application.Abstractions;

namespace RudFitAI.Web.Authentication;

public sealed class Pbkdf2PasswordHasher : IPasswordHasher
{
    private const int IterationCount = 600_000;
    private const int SaltLength = 16;
    private const int SubkeyLength = 32;
    private const string FormatVersion = "v1";

    public string Hash(string password)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(SaltLength);
        byte[] subkey = KeyDerivation.Pbkdf2(password, salt, KeyDerivationPrf.HMACSHA512, IterationCount, SubkeyLength);
        string saltBase64 = Convert.ToBase64String(salt);
        string subkeyBase64 = Convert.ToBase64String(subkey);
        return $"{FormatVersion}.{IterationCount}.{saltBase64}.{subkeyBase64}";
    }

    public bool Verify(string password, string storedHash)
    {
        if (string.IsNullOrWhiteSpace(storedHash))
        {
            return false;
        }

        string[] parts = storedHash.Split('.', StringSplitOptions.None);
        if (parts.Length != 4)
        {
            return false;
        }

        if (!string.Equals(parts[0], FormatVersion, StringComparison.Ordinal))
        {
            return false;
        }

        if (!int.TryParse(parts[1], out int iterations))
        {
            return false;
        }

        byte[] salt;
        byte[] expectedSubkey;
        try
        {
            salt = Convert.FromBase64String(parts[2]);
            expectedSubkey = Convert.FromBase64String(parts[3]);
        }
        catch (FormatException)
        {
            return false;
        }

        byte[] actualSubkey = KeyDerivation.Pbkdf2(
            password,
            salt,
            KeyDerivationPrf.HMACSHA512,
            iterations,
            expectedSubkey.Length);

        return CryptographicOperations.FixedTimeEquals(actualSubkey, expectedSubkey);
    }
}
