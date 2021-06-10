using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Process.Domain.Entities;

namespace System.Process.Infrastructure.Data.Maps
{
    internal sealed class TransferMap : IEntityTypeConfiguration<Transfer>
    {
        public void Configure(EntityTypeBuilder<Transfer> builder)
        {
            builder.ToTable("TRANSFERS");

            builder.HasKey(x => x.LifeCycleId);
            builder.Property(x => x.LifeCycleId).HasColumnName("LIFECYCLEID");
            builder.Property(x => x.TransferType).HasColumnName("TRANSFERTYPE");
            builder.Property(x => x.TransferDirection).HasColumnName("TRANSFERDIRECTION");
            builder.Property(x => x.SystemId).HasColumnName("SystemID");
            builder.Property(x => x.CustomerId).HasColumnName("CUSTOMERID");
            builder.Property(x => x.CustomerIsEnabled).HasColumnName("CUSTOMERISENABLED");
            builder.Property(x => x.ReceiverId).HasColumnName("RECEIVERID");
            builder.Property(x => x.Message).HasColumnName("MESSAGE");
            builder.Property(x => x.Amount).HasColumnName("AMOUNT");
            builder.Property(x => x.AccountFromNumber).HasColumnName("ACCOUNTFROMNUMBER");
            builder.Property(x => x.AccountFromType).HasColumnName("ACCOUNTFROMTYPE");
            builder.Property(x => x.AccountFromRoutingNumber).HasColumnName("ACCOUNTFROMROUTINGNUMBER");
            builder.Property(x => x.SenderName).HasColumnName("SENDERNAME");
            builder.Property(x => x.AccountToNumber).HasColumnName("ACCOUNTTONUMBER");
            builder.Property(x => x.AccountToType).HasColumnName("ACCOUNTTOTYPE");
            builder.Property(x => x.AccountToRoutingNumber).HasColumnName("ACCOUNTTOROUTINGNUMBER");
            builder.Property(x => x.ReceiverFirstName).HasColumnName("RECEIVERFIRSTNAME");
            builder.Property(x => x.ReceiverLastName).HasColumnName("RECEIVERLASTNAME");
            builder.Property(x => x.ReceiverEmail).HasColumnName("RECEIVEREMAIL");
            builder.Property(x => x.ReceiverPhone).HasColumnName("RECEIVERPHONE");
            builder.Property(x => x.ReceiverAddressLine1).HasColumnName("RECEIVERADDRESSLINE1");
            builder.Property(x => x.ReceiverAddressLine2).HasColumnName("RECEIVERADDRESSLINE2");
            builder.Property(x => x.ReceiverAddressLine3).HasColumnName("RECEIVERADDRESSLINE3");
            builder.Property(x => x.ReceiverAddressCity).HasColumnName("RECEIVERADDRESSCITY");
            builder.Property(x => x.ReceiverAddressState).HasColumnName("RECEIVERADDRESSSTATE");
            builder.Property(x => x.ReceiverAddressCountry).HasColumnName("RECEIVERADDRESSCOUNTRY");
            builder.Property(x => x.ReceiverAddressZipCode).HasColumnName("RECEIVERADDRESSZIPCODE");
            builder.Property(x => x.BankName).HasColumnName("BANKNAME");
            builder.Property(x => x.BankAddressLine1).HasColumnName("BANKADDRESSLINE1");
            builder.Property(x => x.BankAddressLine2).HasColumnName("BANKADDRESSLINE2");
            builder.Property(x => x.BankAddressLine3).HasColumnName("BANKADDRESSLINE3");
            builder.Property(x => x.BankAddressCity).HasColumnName("BANKADDRESSCITY");
            builder.Property(x => x.BankAddressState).HasColumnName("BANKADDRESSSTATE");
            builder.Property(x => x.BankAddressCountry).HasColumnName("BANKADDRESSCOUNTRY");
            builder.Property(x => x.BankAddressZipCode).HasColumnName("BANKADDRESSZIPCODE");
            builder.Property(x => x.ReducedPrincipal).HasColumnName("REDUCEDPRINCIPAL");
            builder.Property(x => x.NextDay).HasColumnName("NEXTDAY");
            builder.Property(x => x.Geolocation).HasColumnName("GEOLOCATION");
            builder.Property(x => x.SessionId).HasColumnName("SESSIONID");
            builder.Property(x => x.StopSequence).HasColumnName("STOPSEQUENCE");
            builder.HasMany(x => x.TransferItems).WithOne().HasForeignKey(t => t.LifeCycleId).IsRequired(false);
        }
    }
}
