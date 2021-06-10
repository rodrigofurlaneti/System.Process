using Microsoft.EntityFrameworkCore;
using System.Process.Infrastructure.Data.Maps;

namespace System.Process.Infrastructure.Data
{
    public class DataTransferContext : DbContext
    {
        public DataTransferContext(DbContextOptions<DataTransferContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new TransferMap());
            modelBuilder.ApplyConfiguration(new TransferItemsMap());
        }
    }
}
