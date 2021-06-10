using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Process.Domain.Entities;

namespace System.Process.Infrastructure.Data.Maps
{
    public class TransferItemsMap : IEntityTypeConfiguration<TransferItem>
    {
        public void Configure(EntityTypeBuilder<TransferItem> builder)
        {
            builder.ToTable("TRANSFERITEMS");

            builder.HasKey(x => x.TransferItemId);
            builder.HasAlternateKey(x => x.LifeCycleId);
            builder.Property(x => x.TransferItemId).HasColumnName("TRANSFERITEMID");
            builder.Property(x => x.LifeCycleId).HasColumnName("LIFECYCLEID");
            builder.Property(x => x.SystemId).HasColumnName("SystemID");
            builder.Property(x => x.ReferenceId).HasColumnName("REFERENCEID");
            builder.Property(x => x.Amount).HasColumnName("AMOUNT");
            builder.Property(x => x.FrontImage).HasColumnName("FRONTIMAGE").HasColumnType("CLOB");
            builder.Property(x => x.RearImage).HasColumnName("REARIMAGE").HasColumnType("CLOB");
        }
    }
}
