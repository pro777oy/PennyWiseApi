using BCrypt.Net;
using PennyWiseApi.Services.Interfaces;

namespace PennyWiseApi.Services.Implementations;

public class PasswordHasherService : IPasswordHasherService
{
    public string Hash(string password) => BCrypt.Net.BCrypt.HashPassword(password);

    public bool Verify(string password, string hash) =>
        BCrypt.Net.BCrypt.Verify(password, hash);
}