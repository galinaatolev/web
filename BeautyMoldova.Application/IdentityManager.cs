using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Data.Entity;
using BeautyMoldova.Domain.Models;
using BeautyMoldova.Database;

namespace BeautyMoldova.Application
{
    public class IdentityManager
    {
        private readonly ShopDataContext _dataStore;

        public IdentityManager()
        {
            _dataStore = new ShopDataContext();
        }

        /// <summary>
        /// Проверяет учетные данные пользователя и возвращает объект пользователя в случае успеха.
        /// </summary>
        /// <param name="login">Логин пользователя.</param>
        /// <param name="pin">Пароль пользователя.</param>
        /// <returns>Объект пользователя или null, если проверка не удалась.</returns>
        public Customer ValidateCredentials(string login, string pin)
        {
            // Загружаем пользователя с его ролями
            var visitor = _dataStore.Customers
                .Include(c => c.CustomerRoles.Select(cr => cr.Role))
                .FirstOrDefault(c => c.Username == login);
            
            if (visitor != null)
            {
                if (VerifySecurityPin(pin, visitor.PasswordHash, visitor.PasswordSalt))
                {
                    visitor.LastLoginDate = DateTime.Now;
                    _dataStore.SaveChanges();
                    return visitor;
                }
            }
            return null;
        }
        
        /// <summary>
        /// Проверяет, имеет ли пользователь указанный уровень доступа.
        /// </summary>
        /// <param name="visitor">Проверяемый пользователь.</param>
        /// <param name="accessLevel">Требуемый уровень доступа.</param>
        /// <returns>True, если пользователь имеет доступ, иначе false.</returns>
        public bool VerifyAccessLevel(Customer visitor, string accessLevel)
        {
            return visitor.AccessLevel == accessLevel;
        }

        /// <summary>
        /// Регистрирует нового пользователя в системе.
        /// </summary>
        /// <param name="login">Логин нового пользователя.</param>
        /// <param name="pin">Пароль нового пользователя.</param>
        /// <returns>Созданный объект пользователя.</returns>
        public Customer CreateNewAccount(string login, string pin)
        {
            // Проверяем, существует ли уже пользователь с таким логином
            if (_dataStore.Customers.Any(c => c.Username == login))
            {
                return null;
            }

            // Создаем соль и хеш пароля
            byte[] passwordSalt = CreateSalt();
            byte[] passwordHash = GenerateSecurePinHash(pin, passwordSalt);

            var newVisitor = new Customer
            {
                Username = login,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                AccessLevel = "Customer",
                RegistrationDate = DateTime.Now,
                IsActive = true,
                LoyaltyPoints = 0
            };
            
            _dataStore.Customers.Add(newVisitor);
            _dataStore.SaveChanges();
            
            return newVisitor;
        }
        
        /// <summary>
        /// Создает случайную соль для пароля.
        /// </summary>
        private byte[] CreateSalt()
        {
            byte[] salt = new byte[16];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(salt);
            }
            return salt;
        }
        
        /// <summary>
        /// Создает хеш пароля с использованием SHA256 и соли.
        /// </summary>
        private byte[] GenerateSecurePinHash(string pin, byte[] salt)
        {
            using (var hashAlgorithm = SHA256.Create())
            {
                byte[] passwordBytes = Encoding.UTF8.GetBytes(pin);
                byte[] saltedPassword = new byte[passwordBytes.Length + salt.Length];
                
                Buffer.BlockCopy(passwordBytes, 0, saltedPassword, 0, passwordBytes.Length);
                Buffer.BlockCopy(salt, 0, saltedPassword, passwordBytes.Length, salt.Length);
                
                return hashAlgorithm.ComputeHash(saltedPassword);
            }
        }

        /// <summary>
        /// Проверяет соответствие введенного пароля хранимому хешу.
        /// </summary>
        private bool VerifySecurityPin(string pin, byte[] storedHash, byte[] salt)
        {
            byte[] computedHash = GenerateSecurePinHash(pin, salt);
            return computedHash.SequenceEqual(storedHash);
        }
    }
} 