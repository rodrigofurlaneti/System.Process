using Microsoft.EntityFrameworkCore;
using System.Process.Infrastructure.Data.Maps;

namespace System.Process.Infrastructure.Data
{
    public class DataCardContext : DbContext
    {
        public DataCardContext(DbContextOptions<DataCardContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new CardsMap());
        }
    }
}
