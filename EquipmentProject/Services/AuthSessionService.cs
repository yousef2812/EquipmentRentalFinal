using EquipmentProject.Data;
using Microsoft.EntityFrameworkCore;
using SharedEquipment.Models;

namespace EquipmentProject.Services
{
    public sealed class AuthSessionService
    {
        private readonly LocalDbContext _db;
        private readonly PasswordHasher _passwordHasher;

        public AuthSessionService(LocalDbContext db, PasswordHasher passwordHasher)
        {
            _db = db;
            _passwordHasher = passwordHasher;
        }

        public event Action? StateChanged;

        public AuthenticatedUser? CurrentUser { get; private set; }

        public bool IsAuthenticated => CurrentUser is not null;

        public async Task<LoginResult> SignInAsync(string phoneNumber, string password)
        {
            phoneNumber = phoneNumber.Trim();
            password = password.Trim();

            if (string.IsNullOrWhiteSpace(phoneNumber) || string.IsNullOrWhiteSpace(password))
            {
                return LoginResult.Failed("أدخل رقم الهاتف وكلمة المرور.");
            }

            var normalizedPhone = NormalizePhoneNumber(phoneNumber);
            if (string.IsNullOrWhiteSpace(normalizedPhone))
            {
                return LoginResult.Failed("أدخل رقم هاتف صحيح.");
            }

            var activeUsers = await _db.Users
                .Where(u => u.IsActive)
                .ToListAsync();

            var user = activeUsers.FirstOrDefault(u =>
                u.PhoneNumber == normalizedPhone || NormalizePhoneNumber(u.PhoneNumber) == normalizedPhone);

            if (user is null)
            {
                return LoginResult.Failed("رقم الهاتف غير مسجل.");
            }

            if (!_passwordHasher.VerifyPassword(password, user.PasswordHash))
            {
                return LoginResult.Failed("كلمة المرور غير صحيحة.");
            }

            if (_passwordHasher.NeedsRehash(user.PasswordHash))
            {
                user.PasswordHash = _passwordHasher.HashPassword(password);
            }

            user.LastLoginDate = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            CurrentUser = new AuthenticatedUser(
                user.Id,
                GetDisplayName(user),
                user.Username,
                user.PhoneNumber,
                user.Role);

            StateChanged?.Invoke();
            return LoginResult.Success();
        }

        public void SignOut()
        {
            if (CurrentUser is null)
            {
                return;
            }

            CurrentUser = null;
            StateChanged?.Invoke();
        }

        private static string GetDisplayName(User user) =>
            !string.IsNullOrWhiteSpace(user.FullName) ? user.FullName : user.Username;

        private static string NormalizePhoneNumber(string value)
        {
            var digits = new string(value.Where(char.IsDigit).ToArray());
            if (digits.StartsWith("218", StringComparison.Ordinal) && digits.Length >= 11)
            {
                return $"0{digits[3..]}";
            }

            return digits;
        }

        public sealed record AuthenticatedUser(
            int Id,
            string DisplayName,
            string Username,
            string PhoneNumber,
            UserRole Role);

        public sealed record LoginResult(bool Succeeded, string Message)
        {
            public static LoginResult Success() => new(true, string.Empty);

            public static LoginResult Failed(string message) => new(false, message);
        }
    }
}
