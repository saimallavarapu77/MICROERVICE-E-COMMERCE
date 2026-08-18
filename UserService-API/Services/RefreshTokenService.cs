using System.Security.Cryptography;
using System.Text;

namespace UserService.API.Services;

public class RefreshTokenService
{
    public string GenerateToken()
    {
        var randomBytes = RandomNumberGenerator.GetBytes(64);

        return Convert.ToBase64String(randomBytes);
    }

    public string HashToken(string token)
    {
        var bytes = SHA256.HashData(
            Encoding.UTF8.GetBytes(token));

        return Convert.ToBase64String(bytes);
    }
}