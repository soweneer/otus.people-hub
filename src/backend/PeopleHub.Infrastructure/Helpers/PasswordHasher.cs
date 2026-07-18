using System.Security.Cryptography;
using PeopleHub.Domain.Services;

namespace PeopleHub.Infrastructure.Helpers;

internal class PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 0x10;
    private const int KeySize = 0x20;
    private const int HashSize = 1 + SaltSize + KeySize;
    private const byte CurrentVersion = 1;

    // Версия формата (первый байт хеша) -> число итераций PBKDF2.
    // Версия 0 — legacy-хеши, выпущенные до перехода на 600k итераций.
    private static readonly IReadOnlyDictionary<byte, int> IterationsByVersion = new Dictionary<byte, int>
    {
        [0] = 1_000,
        [1] = 600_000
    };

    public string Hash(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new ArgumentNullException(nameof(password));
        }

        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var key = Rfc2898DeriveBytes.Pbkdf2(password, salt, IterationsByVersion[CurrentVersion],
            HashAlgorithmName.SHA256, KeySize);

        var dst = new byte[HashSize];
        dst[0] = CurrentVersion;
        Buffer.BlockCopy(salt, 0, dst, 1, SaltSize);
        Buffer.BlockCopy(key, 0, dst, 1 + SaltSize, KeySize);
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
        if (src.Length != HashSize || !IterationsByVersion.TryGetValue(src[0], out var iterations))
        {
            return false;
        }

        var salt = new byte[SaltSize];
        Buffer.BlockCopy(src, 1, salt, 0, SaltSize);
        var storedKey = new byte[KeySize];
        Buffer.BlockCopy(src, 1 + SaltSize, storedKey, 0, KeySize);

        var computedKey = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, KeySize);
        return CryptographicOperations.FixedTimeEquals(storedKey, computedKey);
    }
}
