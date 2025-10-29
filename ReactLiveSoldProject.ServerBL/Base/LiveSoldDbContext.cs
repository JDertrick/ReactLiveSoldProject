using Microsoft.EntityFrameworkCore;

namespace ReactLiveSoldProject.ServerBL.Base
{
    public class LiveSoldDbContext : DbContext
    {
        public LiveSoldDbContext(DbContextOptions<LiveSoldDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
