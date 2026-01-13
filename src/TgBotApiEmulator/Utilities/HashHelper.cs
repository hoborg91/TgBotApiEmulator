using System.Security.Cryptography;
using System.Text;

namespace TgBotApiEmulator.Utilities;

public static class HashHelper
{
    public static string Hash(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));

        var builder = new StringBuilder();

        foreach (var b in bytes)
        {
            builder.Append(b.ToString("x2"));
        }

        return builder.ToString();
    }
}