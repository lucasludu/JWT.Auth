using JWT.Auth.Models;
using Microsoft.EntityFrameworkCore;

namespace JWT.Auth.Data
{
    public class Context : DbContext
    {
        public Context(DbContextOptions<Context> ops) : base(ops) 
        {
            
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Car> Cars { get; set; }
        public DbSet<UserRefreshToken> UserRefreshToken { get; set; }

    }
}
