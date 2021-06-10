using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Process.Domain.Entities;

namespace System.Process.Infrastructure.Data.Maps
{
    internal sealed class ReceiverMap : IEntityTypeConfiguration<Receiver>
    {
        public void Configure(EntityTypeBuilder<Receiver> builder)
        {
            builder.ToTable("RECEIVERS");

            builder.HasKey(x => x.ReceiverId);
            builder.HasAlternateKey(c => new { c.CustomerId, c.AccountNumber, c.RoutingNumber });
            builder.Property(x => x.ReceiverId).HasColumnName("RECEIVERID"); 
            builder.Property(x => x.CustomerId).HasColumnName("CUSTOMERID");
            builder.Property(x => x.AccountNumber).HasColumnName("ACCOUNTNUMBER");
            builder.Property(x => x.RoutingNumber).HasColumnName("ROUTINGNUMBER");
            builder.Property(x => x.FirstName).HasColumnName("FIRSTNAME");
            builder.Property(x => x.LastName).HasColumnName("LASTNAME");
            builder.Property(x => x.NickName).HasColumnName("NICKNAME");
            builder.Property(x => x.CompanyName).HasColumnName("COMPANYNAME");
            builder.Property(x => x.Phone).HasColumnName("PHONE");
            builder.Property(x => x.Email).HasColumnName("EMAIL");
            builder.Property(x => x.BankType).HasColumnName("BANKTYPE");
            builder.Property(x => x.AccountType).HasColumnName("ACCOUNTTYPE");
            builder.Property(x => x.ReceiverType).HasColumnName("RECEIVERTYPE");
            builder.Property(x => x.Ownership).HasColumnName("OWNERSHIP");
        }
    }
}
