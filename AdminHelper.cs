using System;
using System.Linq;
using System.Threading.Tasks;
using dagstore.Data;
using dagstore.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace dagstore
{
    public class AdminHelper
    {
        public static async Task CreateAdminUser(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Veritabanında hiç kullanıcı var mı kontrol et
            var userExists = await dbContext.Users.AnyAsync();
            
            if (!userExists)
            {
                // Admin kullanıcı oluştur
                var admin = new User
                {
                    UserName = "admin",
                    Email = "admin@example.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                    FirstName = "Admin",
                    LastName = "User",
                    Address = "Admin Address",
                    PhoneNumber = "05xxxxxxxxx",
                    IsAdmin = true,
                    IsEmailVerified = true,
                    CreatedAt = DateTime.Now
                };

                dbContext.Users.Add(admin);
                await dbContext.SaveChangesAsync();
                Console.WriteLine("Admin kullanıcısı oluşturuldu.");
            }
            else
            {
                // Mevcut bir kullanıcıyı admin yap
                var user = await dbContext.Users.FirstOrDefaultAsync();
                if (user != null && !user.IsAdmin)
                {
                    user.IsAdmin = true;
                    await dbContext.SaveChangesAsync();
                    Console.WriteLine($"Kullanıcı '{user.UserName}' admin yapıldı.");
                }
                else if (user != null && user.IsAdmin)
                {
                    Console.WriteLine($"'{user.UserName}' kullanıcısı zaten admin.");
                }
            }
        }
    }
} 