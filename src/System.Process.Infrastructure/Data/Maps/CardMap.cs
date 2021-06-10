using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Process.Domain.Entities;

namespace System.Process.Infrastructure.Data.Maps
{
    internal sealed class CardsMap : IEntityTypeConfiguration<Card>
    {
        public void Configure(EntityTypeBuilder<Card> builder)
        {
            builder.ToTable("CARDS");

            builder.HasKey(x => new { x.CardId, x.Pan, x.LastFour });
            builder.Property(x => x.CardId).HasColumnName("CARDID"); 
            builder.Property(x => x.AssetId).HasColumnName("ASSETID"); 
            builder.Property(x => x.CustomerId).HasColumnName("CUSTOMERID");
            builder.Property(x => x.AccountBalance).HasColumnName("ACCOUNTBALANCE");
            builder.Property(x => x.CardType).HasColumnName("CARDTYPE");
            builder.Property(x => x.Bin).HasColumnName("BIN");
            builder.Property(x => x.Pan).HasColumnName("PAN");
            builder.Property(x => x.LastFour).HasColumnName("LASTFOUR");
            builder.Property(x => x.ExpirationDate).HasColumnName("EXPIRATIONDATE");
            builder.Property(x => x.CardHolder).HasColumnName("CARDHOLDER");
            builder.Property(x => x.BusinessName).HasColumnName("BUSINESSNAME");
            builder.Property(x => x.Locked).HasColumnName("LOCKED");
            builder.Property(x => x.CardStatus).HasColumnName("CARDSTATUS");
            builder.Property(x => x.Validated).HasColumnName("VALIDATED");
        }
    }
}
