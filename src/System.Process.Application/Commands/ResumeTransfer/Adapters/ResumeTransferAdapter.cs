using System.Process.Application.Commands.AchTransferMoney;
using System.Process.Application.Commands.RemoteDepositCapture;
using System.Process.Application.Commands.TransferMoney;
using System.Process.Application.Commands.WireTransfer;
using System.Process.Domain.Entities;
using System.Process.Infrastructure.Adapters;
using System;
using System.Collections.Generic;

namespace System.Process.Application.Commands.ResumeTransfer.Adapters
{
    public class ResumeTransferAdapter : IAdapter<Transfer, TransferMoneyRequest>
    {
        private string LifeCycleId { get; set; }
        private int? StopSequence { get; set; }

        public ResumeTransferAdapter(string lifeCycle, int? stopSequence = null)
        {
            LifeCycleId = lifeCycle;
            StopSequence = stopSequence.HasValue ? stopSequence.Value : 0;
        }

        public Transfer Adapt(TransferMoneyRequest input)
        {
            return new Transfer
            {
                LifeCycleId = Guid.NewGuid().ToString(),
                TransferDirection = "OUT",
                TransferType = "INTERNAL",
                SystemId = input.SystemId,
                CustomerId = input.CustomerId,
                ReceiverId = input.ReceiverId,
                ReducedPrincipal = input.ReducedPrincipal,
                Amount = input.Amount,
                Message = input.Message,
                AccountFromNumber = input.AccountFrom.FromAccountNumber,
                AccountFromType = input.AccountFrom.FromAccountType,
                AccountToNumber = input.AccountTo.ToAccountNumber,
                AccountToType = input.AccountTo.ToAccountType,
                StopSequence = StopSequence.Value
            };
        }

        public Transfer Adapt(AchTransferMoneyRequest input)
        {
            return new Transfer
            {
                LifeCycleId = Guid.NewGuid().ToString(),
                TransferDirection = "OUT",
                TransferType = "ACH",
                SystemId = input.SystemId,
                CustomerId = input.CustomerId,
                ReceiverId = input.ReceiverId,
                ReducedPrincipal = input.ReducedPrincipal,
                Amount = input.Amount,
                AccountFromNumber = input.AccountFrom.AccountId,
                AccountFromType = input.AccountFrom.AccountType,
                AccountFromRoutingNumber = input.AccountFrom.RoutingNumber,
                SenderName = input.AccountFrom.Name,
                AccountToNumber = input.AccountTo.AccountId,
                AccountToType = input.AccountTo.AccountType,
                AccountToRoutingNumber = input.AccountTo.RoutingNumber,
                ReceiverFirstName = input.AccountTo.Name,
                ReceiverEmail = input.AccountTo.Email,
                ReceiverPhone = input.AccountTo.Phone,
                StopSequence = StopSequence.Value
            };
        }
        public Transfer Adapt(WireTransferAddRequest input)
        {
            return new Transfer
            {
                LifeCycleId = LifeCycleId,
                TransferDirection = "OUT",
                TransferType = "WIRE",
                SystemId = input.SystemId,
                CustomerId = input.CustomerId,
                ReceiverId = input.ReceiverId,
                Message = input.Message,
                Amount = input.Amount,
                AccountFromNumber = input.FromAccountId,
                AccountFromType = input.FromAccountType,
                AccountFromRoutingNumber = input.FromRoutingNumber,
                AccountToNumber = input.ToAccountId,
                AccountToType = input.ToAccountType,
                AccountToRoutingNumber = input.ToRoutingNumber,
                ReceiverFirstName = input.ReceiverFirstName,
                ReceiverLastName = input.ReceiverLastName,
                ReceiverEmail = input.ReceiverEmail,
                ReceiverPhone = input.ReceiverPhone,
                ReceiverAddressCity = input.ReceiverAddress.City,
                ReceiverAddressCountry = input.ReceiverAddress.Country,
                ReceiverAddressLine1 = input.ReceiverAddress.Line1,
                ReceiverAddressLine2 = input.ReceiverAddress.Line2,
                ReceiverAddressLine3 = input.ReceiverAddress.Line3,
                ReceiverAddressState = input.ReceiverAddress.State,
                ReceiverAddressZipCode = input.ReceiverAddress.ZipCode,
                BankName = input.BankName,
                BankAddressCity = input.BankAddress.City,
                BankAddressCountry = input.BankAddress.Country,
                BankAddressLine1 = input.BankAddress.Line1,
                BankAddressLine2 = input.BankAddress.Line2,
                BankAddressLine3 = input.BankAddress.Line3,
                BankAddressState = input.BankAddress.State,
                BankAddressZipCode = input.BankAddress.ZipCode,
                Geolocation = 0,
                NextDay = "",
                StopSequence = StopSequence.Value
            };
        }
        
        public Transfer Adapt(RemoteDepositCaptureRequest input)
        {
            return new Transfer
            {
                LifeCycleId = LifeCycleId,
                TransferDirection = "IN",
                TransferType = "RDC",
                SystemId = input.SystemId,
                AccountToNumber = input.ToAccount,
                AccountToRoutingNumber = input.ToRoutingNumber,
                Geolocation = input.GeoLocation.Value,
                Amount = input.TotalAmount,
                StopSequence = StopSequence.Value

            };
        }

        public List<TransferItem> AdaptItems(RemoteDepositCaptureRequest input, string lifeCycleId)
        {
            var transferItens = new List<TransferItem>();
            foreach (var item in input.Item) 
            {
                var transferItemAdapted =  new TransferItem
                {
                    SystemId = input.SystemId,
                    LifeCycleId = lifeCycleId,
                    Amount = item.Amount,
                    FrontImage = item.FrontImage,
                    RearImage = item.BackImage
                };
                transferItens.Add(transferItemAdapted);
            }
            return transferItens;
        }
    }
}
