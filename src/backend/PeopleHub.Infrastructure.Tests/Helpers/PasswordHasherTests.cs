using System.Security.Cryptography;
using PeopleHub.Infrastructure.Helpers;

namespace PeopleHub.Infrastructure.Tests.Helpers;

public sealed class PasswordHasherTests
{
    private const string Password = "qweasd123";

    private readonly PasswordHasher _hasher = new();

    [Fact]
    public void Hash_ProducesVersionedHashOfExpectedLength()
    {
        var hashBytes = Convert.FromBase64String(_hasher.Hash(Password));

        Assert.Equal(0x31, hashBytes.Length);
        Assert.Equal(1, hashBytes[0]);
    }

    [Fact]
    public void Verify_CorrectPassword_ReturnsTrue()
    {
        var hash = _hasher.Hash(Password);

        Assert.True(_hasher.Verify(hash, Password));
    }

    [Fact]
    public void Verify_WrongPassword_ReturnsFalse()
    {
        var hash = _hasher.Hash(Password);

        Assert.False(_hasher.Verify(hash, "wrong-password"));
    }

    [Fact]
    public void Verify_LegacyVersion0Hash_ReturnsTrue()
    {
        Assert.True(_hasher.Verify(CreateLegacyHash(Password), Password));
    }

    [Fact]
    public void Verify_LegacyVersion0Hash_WrongPassword_ReturnsFalse()
    {
        Assert.False(_hasher.Verify(CreateLegacyHash(Password), "wrong-password"));
    }

    [Fact]
    public void Verify_UnknownVersion_ReturnsFalse()
    {
        var hashBytes = Convert.FromBase64String(_hasher.Hash(Password));
        hashBytes[0] = 0x7f;

        Assert.False(_hasher.Verify(Convert.ToBase64String(hashBytes), Password));
    }

    [Fact]
    public void Hash_SamePasswordTwice_ProducesDifferentHashes()
    {
        Assert.NotEqual(_hasher.Hash(Password), _hasher.Hash(Password));
    }

    // Хеш старого формата: [0x00][16 байт соли][32 байта ключа], 1000 итераций.
    private static string CreateLegacyHash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(0x10);
        var key = Rfc2898DeriveBytes.Pbkdf2(password, salt, 1_000, HashAlgorithmName.SHA256, 0x20);

        var dst = new byte[0x31];
        Buffer.BlockCopy(salt, 0, dst, 1, 0x10);
        Buffer.BlockCopy(key, 0, dst, 0x11, 0x20);
        return Convert.ToBase64String(dst);
    }
}
