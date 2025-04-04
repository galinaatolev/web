namespace BeautyMoldova.Database.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;
    using BeautyMoldova.Domain.Models;

    internal sealed class Configuration : DbMigrationsConfiguration<BeautyMoldova.Database.ShopDataContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(BeautyMoldova.Database.ShopDataContext context)
        {
            // Создаем роли если они не существуют
            if (!context.Roles.Any())
            {
                // Роль администратора
                context.Roles.AddOrUpdate(r => r.Name,
                    new Role
                    {
                        Name = "Admin",
                        Description = "Полный доступ к системе управления",
                        IsActive = true,
                        CreatedDate = DateTime.Now,
                        Permissions = "{ \"fullAccess\": true }"
                    });

                // Роль обычного пользователя
                context.Roles.AddOrUpdate(r => r.Name,
                    new Role
                    {
                        Name = "Customer",
                        Description = "Стандартный доступ клиента",
                        IsActive = true,
                        CreatedDate = DateTime.Now,
                        Permissions = "{ \"canShop\": true, \"canReview\": true }"
                    });

                context.SaveChanges();
            }

            // Создаем пользователей, если их еще нет
            if (!context.Customers.Any(c => c.Username == "admin" || c.Username == "user"))
            {
                // Получаем ID ролей
                var adminRoleId = context.Roles.FirstOrDefault(r => r.Name == "Admin")?.Id;
                var customerRoleId = context.Roles.FirstOrDefault(r => r.Name == "Customer")?.Id;

                if (adminRoleId.HasValue && customerRoleId.HasValue)
                {
                    // Создаем пользователя admin с паролем admin
                    if (!context.Customers.Any(c => c.Username == "admin"))
                    {
                        var adminSalt = CreateSalt();
                        var adminHash = GenerateSecurePinHash("admin", adminSalt);

                        var adminUser = new Customer
                        {
                            Username = "admin",
                            PasswordHash = adminHash,
                            PasswordSalt = adminSalt,
                            AccessLevel = "Admin",
                            FirstName = "Admin",
                            LastName = "User",
                            Email = "admin@example.com",
                            RegistrationDate = DateTime.Now,
                            IsActive = true,
                            LoyaltyPoints = 0
                        };

                        context.Customers.Add(adminUser);
                        context.SaveChanges();

                        // Связываем с ролью администратора
                        context.CustomerRoles.Add(new CustomerRole
                        {
                            CustomerId = adminUser.Id,
                            RoleId = adminRoleId.Value,
                            AssignedDate = DateTime.Now
                        });
                    }

                    // Создаем пользователя user с паролем user
                    if (!context.Customers.Any(c => c.Username == "user"))
                    {
                        var userSalt = CreateSalt();
                        var userHash = GenerateSecurePinHash("user", userSalt);

                        var regularUser = new Customer
                        {
                            Username = "user",
                            PasswordHash = userHash,
                            PasswordSalt = userSalt,
                            AccessLevel = "Customer",
                            FirstName = "Regular",
                            LastName = "User",
                            Email = "user@example.com",
                            RegistrationDate = DateTime.Now,
                            IsActive = true,
                            LoyaltyPoints = 0
                        };

                        context.Customers.Add(regularUser);
                        context.SaveChanges();

                        // Связываем с ролью обычного пользователя
                        context.CustomerRoles.Add(new CustomerRole
                        {
                            CustomerId = regularUser.Id,
                            RoleId = customerRoleId.Value,
                            AssignedDate = DateTime.Now
                        });
                    }

                    context.SaveChanges();
                }
            }
        }

        // Создает случайную соль для пароля
        private byte[] CreateSalt()
        {
            byte[] salt = new byte[16];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(salt);
            }
            return salt;
        }
        
        // Создает хеш пароля с использованием SHA256 и соли
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
    }
}
