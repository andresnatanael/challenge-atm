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
        
        
        
        

        /*public async Task RecordFailedLoginAsync(string username)
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
        }*/
        public Task<User?> AuthenticateUserAsync(string cardNumber, string pin)
        {
            
            throw new NotImplementedException();
        }

        public Task<bool> IsCardNumberLockedOutAsync(string cardNumber)
        {
            throw new NotImplementedException();
        }

        public Task RecordFailedLoginAsync(string cardNumber)
        {
            throw new NotImplementedException();
        }

        public Task ResetFailedAttemptsAsync(string cardNumber)
        {
            throw new NotImplementedException();
        }
    }
}