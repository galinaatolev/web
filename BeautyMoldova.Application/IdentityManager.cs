using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Data.Entity;
using BeautyMoldova.Domain.Models;
using BeautyMoldova.Database;

namespace BeautyMoldova.Application
{
    /// <summary>
    /// Менеджер идентификации, наследуется от BaseApi
    /// ✅ ИСПРАВЛЕНО - теперь использует только базовые CRUD методы вместо прямых обращений к _context
    /// </summary>
    public class IdentityManager : BaseApi
    {
        public IdentityManager() : base() { }
        public IdentityManager(ShopDataContext context) : base(context) { }

        /// <summary>
        /// Проверяет учетные данные пользователя и возвращает объект пользователя в случае успеха.
        /// ✅ ИСПРАВЛЕНО - использует базовый метод GetAll вместо прямого обращения к _context
        /// </summary>
        /// <param name="login">Логин пользователя.</param>
        /// <param name="pin">Пароль пользователя.</param>
        /// <returns>Объект пользователя или null, если проверка не удалась.</returns>
        public Customer ValidateCredentials(string login, string pin)
        {
            // ✅ ПРАВИЛЬНО - использует базовый метод GetAll вместо _dataStore.Customers
            var allCustomers = GetAll<Customer>();
            var visitor = allCustomers.FirstOrDefault(c => c.Username == login);
            
            if (visitor != null)
            {
                if (VerifySecurityPin(pin, visitor.PasswordHash, visitor.PasswordSalt))
                {
                    visitor.LastLoginDate = DateTime.Now;
                    // ✅ ПРАВИЛЬНО - использует базовый метод Update вместо _dataStore.SaveChanges()
                    Update<Customer>(visitor);
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
        /// ✅ ИСПРАВЛЕНО - использует базовые методы вместо прямого обращения к _context
        /// </summary>
        /// <param name="login">Логин нового пользователя.</param>
        /// <param name="pin">Пароль нового пользователя.</param>
        /// <returns>Созданный объект пользователя.</returns>
        public Customer CreateNewAccount(string login, string pin)
        {
            // ✅ ПРАВИЛЬНО - использует базовый метод GetAll вместо _dataStore.Customers.Any()
            var allCustomers = GetAll<Customer>();
            if (allCustomers.Any(c => c.Username == login))
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
            
            // ✅ ПРАВИЛЬНО - использует базовый метод Create вместо _dataStore.Customers.Add() и _dataStore.SaveChanges()
            if (Create<Customer>(newVisitor))
            {
                return newVisitor;
            }
            
            return null;
        }

        /// <summary>
        /// Получить пользователя по имени пользователя с ролями
        /// ✅ НОВЫЙ МЕТОД - использует базовые методы для получения пользователя с ролями
        /// </summary>
        /// <param name="username">Имя пользователя</param>
        /// <returns>Пользователь с ролями или null</returns>
        public Customer GetUserWithRoles(string username)
        {
            // ✅ ПРАВИЛЬНО - использует базовые методы GetAll
            var allCustomers = GetAll<Customer>();
            var customer = allCustomers.FirstOrDefault(c => c.Username == username);
            
            if (customer != null)
            {
                // Загружаем роли пользователя
                var allCustomerRoles = GetAll<CustomerRole>();
                var allRoles = GetAll<Role>();
                
                var customerRoleIds = allCustomerRoles.Where(cr => cr.CustomerId == customer.Id)
                                                     .Select(cr => cr.RoleId)
                                                     .ToList();
                
                // Можно добавить роли в свойство, если нужно
                // customer.Roles = allRoles.Where(r => customerRoleIds.Contains(r.Id)).ToList();
            }
            
            return customer;
        }

        /// <summary>
        /// Проверить, имеет ли пользователь определенную роль
        /// ✅ НОВЫЙ МЕТОД - использует базовые методы
        /// </summary>
        /// <param name="customerId">ID пользователя</param>
        /// <param name="roleName">Название роли</param>
        /// <returns>True, если пользователь имеет роль</returns>
        public bool HasRole(int customerId, string roleName)
        {
            var allCustomerRoles = GetAll<CustomerRole>();
            var allRoles = GetAll<Role>();
            
            var customerRoleIds = allCustomerRoles.Where(cr => cr.CustomerId == customerId)
                                                 .Select(cr => cr.RoleId)
                                                 .ToList();
            
            return allRoles.Any(r => customerRoleIds.Contains(r.Id) && r.Name == roleName && r.IsActive);
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