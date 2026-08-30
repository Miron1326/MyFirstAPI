using Microsoft.EntityFrameworkCore;

namespace MyAPI.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) //КОНСРУКТОР КОТОРЫЙ ПРИНИМАЕТ НАСТРОЙКИ (DI)
        {

        }

        public DbSet<Item> Items { get; set; } //таблица Items в базе данных
    }
}
