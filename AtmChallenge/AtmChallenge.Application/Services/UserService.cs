using AtmChallenge.Application.Interfaces;
using AtmChallenge.Domain.Entities;
using AtmChallenge.Infrastructure.Repositories;
using System.Security.Cryptography;
using System.Text;

namespace AtmChallenge.Application.Services
{
    public class UserService : IUserService
    {
        private readonly UserRepository _userRepository;

        public UserService(UserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<User?> AuthenticateUserAsync(string username, string password)
        {
            var user = await _userRepository.GetByUsernameAsync(username);
            if (user == null || !VerifyPassword(password, user.PasswordHash))
                return null;

            return user;
        }

        public async Task<bool> IsUserLockedOutAsync(string username)
        {
            var user = await _userRepository.GetByUsernameAsync(username);
            return user != null && user.LockoutEnd > DateTime.UtcNow;
        }

        public async Task RecordFailedLoginAsync(string username)
        {
            var user = await _userRepository.GetByUsernameAsync(username);
            if (user == null) return;

            user.FailedLoginAttempts++;
            if (user.FailedLoginAttempts >= 5)
            {
                user.LockoutEnd = DateTime.UtcNow.AddMinutes(15);
            }

            await _userRepository.UpdateAsync(user);
        }

        public async Task ResetFailedAttemptsAsync(string username)
        {
            var user = await _userRepository.GetByUsernameAsync(username);
            if (user == null) return;

            user.FailedLoginAttempts = 0;
            user.LockoutEnd = null;
            await _userRepository.UpdateAsync(user);
        }

        private bool VerifyPassword(string enteredPassword, string storedHash)
        {
            using var sha256 = SHA256.Create();
            var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(enteredPassword));
            return Convert.ToBase64String(hashBytes) == storedHash;
        }
    }
}