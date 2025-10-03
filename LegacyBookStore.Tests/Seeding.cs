using LegacyBookStore.Data;
using LegacyBookStore.Models;

namespace LegacyBookStore.Tests
{
    public class Seeding
    {
        public static void InitializeTestDb(AppDbContext db)
        {
            // Удаление существующих данных
            RemoveData(db);

            // Добавление тестовых книг
            db.Books.AddRange(
                new Book { Title = "Clean Code", Author = "Robert Martin", Price = 29.99m },
                new Book { Title = "Domain-Driven Design", Author = "Eric Evans", Price = 39.99m }
            );

            // Добавление тестовых пользователей
            db.Users.AddRange(
                new User { Name = "Admin User", Email = "admin@example.com", Role = "Admin" },
                new User { Name = "Regular User", Email = "user@example.com", Role = "User" }
            );

            // Сохранение изменений
            db.SaveChanges();
        }

        public static void RemoveData(AppDbContext db)
        {
            db.Books?.RemoveRange(db.Books);
            db.Users?.RemoveRange(db.Users);
            db.SaveChanges();
        }
    }
}
