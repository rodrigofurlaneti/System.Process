using Microsoft.EntityFrameworkCore;
using System.Process.Infrastructure.Data.Maps;

namespace System.Process.Infrastructure.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new ReceiverMap());
        }
    }
}
