using Microsoft.AspNetCore.Identity;

namespace RestaurantKitchen.Data
{
    /// <summary>Українські тексти помилок Identity (реєстрація, пароль, email).</summary>
    public class UkrainianIdentityErrorDescriber : IdentityErrorDescriber
    {
        public override IdentityError PasswordTooShort(int length) =>
            new() { Code = nameof(PasswordTooShort), Description = $"Пароль має містити щонайменше {length} символів" };

        public override IdentityError PasswordRequiresDigit() =>
            new() { Code = nameof(PasswordRequiresDigit), Description = "Пароль має містити хоча б одну цифру" };

        public override IdentityError PasswordRequiresLower() =>
            new() { Code = nameof(PasswordRequiresLower), Description = "Пароль має містити хоча б одну малу латинську літеру" };

        public override IdentityError PasswordRequiresUpper() =>
            new() { Code = nameof(PasswordRequiresUpper), Description = "Пароль має містити хоча б одну велику латинську літеру" };

        public override IdentityError PasswordRequiresNonAlphanumeric() =>
            new() { Code = nameof(PasswordRequiresNonAlphanumeric), Description = "Пароль має містити хоча б один спецсимвол" };

        public override IdentityError DuplicateUserName(string userName) =>
            new() { Code = nameof(DuplicateUserName), Description = $"Користувач «{userName}» вже зареєстрований" };

        public override IdentityError DuplicateEmail(string email) =>
            new() { Code = nameof(DuplicateEmail), Description = $"Email «{email}» вже використовується" };

        public override IdentityError InvalidEmail(string? email) =>
            new() { Code = nameof(InvalidEmail), Description = $"Некоректний email «{email}»" };

        public override IdentityError InvalidUserName(string? userName) =>
            new() { Code = nameof(InvalidUserName), Description = $"Некоректне ім'я користувача «{userName}»" };
    }
}