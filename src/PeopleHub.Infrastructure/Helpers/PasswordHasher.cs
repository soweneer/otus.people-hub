using System.Security.Cryptography;
using PeopleHub.Domain.Services;

namespace PeopleHub.Infrastructure.Helpers;

internal class PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 0x10;
    private const int KeySize = 0x20;
    private const int Iterations = 0x3e8;

    public string Hash(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new ArgumentNullException(nameof(password));
        }

        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var key = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, KeySize);

        var dst = new byte[0x31];
        Buffer.BlockCopy(salt, 0, dst, 1, SaltSize);
        Buffer.BlockCopy(key, 0, dst, 0x11, KeySize);
        return Convert.ToBase64String(dst);
    }

    public bool Verify(string hash, string password)
    {
        if (string.IsNullOrWhiteSpace(hash))
        {
            return false;
        }
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new ArgumentNullException(nameof(password));
        }

        var src = Convert.FromBase64String(hash);
        if (src.Length != 0x31 || src[0] != 0)
        {
            return false;
        }

        var salt = new byte[SaltSize];
        Buffer.BlockCopy(src, 1, salt, 0, SaltSize);
        var storedKey = new byte[KeySize];
        Buffer.BlockCopy(src, 0x11, storedKey, 0, KeySize);

        var computedKey = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, KeySize);
        return storedKey.SequenceEqual(computedKey);
    }
}
