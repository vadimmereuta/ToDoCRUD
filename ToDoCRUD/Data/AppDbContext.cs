using Microsoft.EntityFrameworkCore;

namespace ToDoCRUD
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<ToDoItem> TodoItems { get; set; }
    }
}
