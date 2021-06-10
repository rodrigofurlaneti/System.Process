using System.Process.Application.DataTransferObjects;
using System.Process.Domain.Constants;
using System.Process.Domain.Enums;
using System.Process.Domain.ValueObjects;
using System.Process.Infrastructure.Adapters;
using System.Proxy.Silverlake.Inquiry.Messages.Request;
using System.Proxy.Silverlake.Inquiry.Messages.Response;
using System;
using System.Collections.Generic;
using System.Text;

namespace System.Process.Application.Queries.GetAccountHistory
{
    public class GetAccountHistoryAdapter :
        IAdapter<ProcessearchRequest, string>,
        IAdapter<AccountHistorySearchRequest, GetAccountHistoryRequest>,
        IAdapter<GetAccountHistoryResponse, AccountHistorySearchResponse>,
        IAdapter<TransactionCategory, TransactionHistoryDto>
    {
        private int LastDaysSearch { get; } = -90;

        private static IDictionary<TransactionCodes, Func<AccountHistorySearchInfo, TransferMessage>> EftDescriptionFunctions { get; set; } =
            new Dictionary<TransactionCodes, Func<AccountHistorySearchInfo, TransferMessage>>()
        {
                //EftDescriptionsAch
                {TransactionCodes.ACHCredit, (accountHistorySearchInfoResult) => EftDescriptionsAch(accountHistorySearchInfoResult)},
                {TransactionCodes.ACHDebit,  (accountHistorySearchInfoResult) => EftDescriptionsAch(accountHistorySearchInfoResult)},
                //EftDescriptionsInternalTransferCredit
                {TransactionCodes.InternalTransferCredit, (accountHistorySearchInfoResult) => EftDescriptionsInternalTransferCredit(accountHistorySearchInfoResult)},
                //EftDescriptionsInternalTransferDebit
                {TransactionCodes.InternalTransferDebit, (accountHistorySearchInfoResult) => EftDescriptionsInternalTransferDebit(accountHistorySearchInfoResult)},
                //EftDescriptionsWireTransferCredit
                {TransactionCodes.WireTransferCredit, (accountHistorySearchInfoResult) => EftDescriptionsWireTransferCredit(accountHistorySearchInfoResult)},
                //EftDescriptionsWireTransferDebit
                {TransactionCodes.WireTransferDebit, (accountHistorySearchInfoResult) => EftDescriptionsWireTransferDebit(accountHistorySearchInfoResult)},
                //EftDescriptionsInternalWireTransferFee
                {TransactionCodes.WireTransferFee, (accountHistorySearchInfoResult) => EftDescriptionsInternalWireTransferFee(accountHistorySearchInfoResult)},
                {TransactionCodes.RemoteDepositCaptureMobile, (accountHistorySearchInfoResult) => EftDescriptionsInternalWireTransferFee(accountHistorySearchInfoResult)},
                //EftDescriptionsATM
                {TransactionCodes.ATMDeposit, (accountHistorySearchInfoResult) => EftDescriptionsATM(accountHistorySearchInfoResult)},
                {TransactionCodes.ATMWithdrawal, (accountHistorySearchInfoResult) => EftDescriptionsATM(accountHistorySearchInfoResult)},
                {TransactionCodes.ATMDepositDDA, (accountHistorySearchInfoResult) => EftDescriptionsATM(accountHistorySearchInfoResult)},
                //EftDescriptionsDebitCard
                {TransactionCodes.DebitCard, (accountHistorySearchInfoResult) => EftDescriptionsDebitCard(accountHistorySearchInfoResult)},
                //EftDescriptionsAccountAnalysisCharge
                {TransactionCodes.AccountAnalysisCharge, (accountHistorySearchInfoResult) => EftDescriptionsAccountAnalysisCharge(accountHistorySearchInfoResult)},
                {TransactionCodes.NSFItemPaid, (accountHistorySearchInfoResult) => EftDescriptionsNSFItemPaid(accountHistorySearchInfoResult)},
                //EftDescriptionsMemo
                {TransactionCodes.MemoCredit, (accountHistorySearchInfoResult) => EftDescriptionsMemo(accountHistorySearchInfoResult)},
                {TransactionCodes.MemoDebit, (accountHistorySearchInfoResult) => EftDescriptionsMemo(accountHistorySearchInfoResult)},
                //EftDescriptionDefault
                {TransactionCodes.Default, (accountHistorySearchInfoResult) => EftDescriptionDefault(accountHistorySearchInfoResult)},
                {TransactionCodes.POSPreAuthorizedDebitDDA, (accountHistorySearchInfoResult) => EftDescriptionDefault(accountHistorySearchInfoResult)},
                {TransactionCodes.POSPreAuthorizedDebitSavings, (accountHistorySearchInfoResult) => EftDescriptionDefault(accountHistorySearchInfoResult)},
                {TransactionCodes.PreAuthMemoHold, (accountHistorySearchInfoResult) => EftDescriptionDefault(accountHistorySearchInfoResult)}

        };

        public ProcessearchRequest Adapt(string input)
        {
            return new ProcessearchRequest
            {
                IncXtendElemArray = new List<IncXtendElemInfoRequest>
                {
                    new IncXtendElemInfoRequest
                    {
                        XtendElem = "x_CondNotfInfoRec"
                    }
                },
                MaximumRecords = 4000,
                AccountId = input
            };
        }

        public AccountHistorySearchRequest Adapt(GetAccountHistoryRequest input)
        {
            if (input == null)
            {
                return null;
            }

            var date = DateTime.UtcNow;

            return new AccountHistorySearchRequest
            {
                StartDate = input.StartDate != null ? input.StartDate : date.AddDays(LastDaysSearch),
                EndingDate = input.EndDate != null ? input.EndDate : date,
                AccountNumber = input.AccountNumber,
                AccountType = input.AccountType,
                MonetaryTransactionsType = input.TransactionsType,
                MemoPostedInc = "true",
                MaximumRecords = 4000
            };
        }

        public TransactionCategory Adapt(TransactionHistoryDto input)
        {
            int.TryParse(input.GroupName, out int code);
            return new TransactionCategory
            {
                Code = code
            };
        }

        public GetAccountHistoryResponse Adapt(AccountHistorySearchResponse input)
        {
            return new GetAccountHistoryResponse
            {
                TransactionHistory = GetResult(input.AccountHistorySearchInfo)
            };
        }

        private IList<TransactionHistoryDto> GetResult(IList<AccountHistorySearchInfo> input)
        {
            if (input == null)
            {
                return null;
            }

            var responseList = new List<TransactionHistoryDto>();

            foreach (var item in input)
            {
                if (item.DepHistorySearchRec?.Amount != null)
                {
                    var responseItem = new TransactionHistoryDto
                    {
                        Type = item.DepHistorySearchRec.DepTransactionType,
                        Amount = item.DepHistorySearchRec.Amount,
                        AmountCurrency = "USD",
                        PostedDate = item.DepHistorySearchRec.PostedDate,
                        TransactionDate = item.DepHistorySearchRec.DifferentPostingDate,
                        GroupName = item.DepHistorySearchRec.MonetaryTransactionCode,
                        Description = item.DepHistorySearchRec.SourceCodeDescription,
                        Status = GetTransactionStatus(item.DepHistorySearchRec.MemoPosted.MemoPosted),
                        MemoMessage = item.DepHistorySearchRec.EftDescInfo.Count > 3 ? item.DepHistorySearchRec.EftDescInfo[0].FreeFormatDescription : string.Empty,
                        TransferMessage = GetTransferMessage(item)
                    };
                    responseList.Add(responseItem);
                }

                if (item.LnHistorySearchRec?.LnAmount != null)
                {
                    var responseItem = new TransactionHistoryDto
                    {
                        Type = item.LnHistorySearchRec.LnTransactionType,
                        Amount = item.LnHistorySearchRec.LnAmount,
                        AmountCurrency = "USD",
                        PostedDate = item.LnHistorySearchRec.LnPostedDate,
                        TransactionDate = item.LnHistorySearchRec.LnDifferentPostingDate,
                        GroupName = item.LnHistorySearchRec.LnMonetaryTransactionCode,
                        Description = item.LnHistorySearchRec.LnSourceCodeDescription,
                        Status = GetTransactionStatus(item.LnHistorySearchRec.LnMemoPosted.MemoPosted),
                        TransferMessage = GetTransferMessage(item)
                    };
                    responseList.Add(responseItem);
                }

                if (item.SafeDepHistorySearchRec?.SafeAmount != null)
                {
                    var responseItem = new TransactionHistoryDto
                    {
                        Type = item.SafeDepHistorySearchRec.SafeTransactionType,
                        Amount = item.SafeDepHistorySearchRec.SafeAmount,
                        AmountCurrency = "USD",
                        PostedDate = item.SafeDepHistorySearchRec.SafePostedDate,
                        GroupName = item.SafeDepHistorySearchRec.SafeMonetaryTransactionCode,
                        Description = item.SafeDepHistorySearchRec.SafeSourceCodeDescription,
                        Status = GetTransactionStatus(item.SafeDepHistorySearchRec.SafeMemoPosted.MemoPosted),
                        TransferMessage = GetTransferMessage(item)
                    };
                    responseList.Add(responseItem);
                }

                if (item.TimeDepHistorySearchRec?.TimeAmount != null)
                {
                    var responseItem = new TransactionHistoryDto
                    {
                        Type = item.TimeDepHistorySearchRec.TimeTransactionType,
                        Amount = item.TimeDepHistorySearchRec.TimeAmount,
                        AmountCurrency = "USD",
                        PostedDate = item.TimeDepHistorySearchRec.TimePostedDate,
                        GroupName = item.TimeDepHistorySearchRec.TimeMonetaryTransactionCode,
                        Description = item.TimeDepHistorySearchRec.TimeSourceCodeDescription,
                        Status = GetTransactionStatus(item.TimeDepHistorySearchRec.TimeMemoPosted.MemoPosted),
                        TransferMessage = GetTransferMessage(item)
                    };
                    responseList.Add(responseItem);
                }
            }

            return responseList;
        }

        private TransferMessage GetTransferMessage(AccountHistorySearchInfo AccountHistorySearchInfoResult)
        {
            var transactionCode = TransactionCodes.Default;

            var monetaryTransactionCode = Convert.ToInt32(AccountHistorySearchInfoResult.DepHistorySearchRec.MonetaryTransactionCode);

            if (Enum.IsDefined(typeof(TransactionCodes), monetaryTransactionCode))
            {
                transactionCode = (TransactionCodes)monetaryTransactionCode;
            }

            var eftDescriptionFunctions = EftDescriptionFunctions[transactionCode];

            return eftDescriptionFunctions.Invoke(AccountHistorySearchInfoResult);
        }

        private string GetTransactionStatus(string memoPost)
        {
            if (String.Equals(memoPost, "y", StringComparison.OrdinalIgnoreCase))
            {
                return "Pending";
            }
            return "Posted";
        }

        private static TransferMessage EftDescriptionDefault(AccountHistorySearchInfo accountHistorySearchInfoResult)
        {
            var transferMessage = new TransferMessage();
            transferMessage.MemoDescription = "-";

            var customTransactionDetail = new StringBuilder();

            if (accountHistorySearchInfoResult.DepHistorySearchRec.EftDescInfo.Count > 0)
            {
                foreach (var eftDescription in accountHistorySearchInfoResult.DepHistorySearchRec.EftDescInfo)
                {
                    customTransactionDetail.Append(eftDescription.FreeFormatDescription + " ");
                }

                transferMessage.TransactionDetail = customTransactionDetail.ToString();
            }
            else
            {
                transferMessage.TransactionDetail = accountHistorySearchInfoResult.DepHistorySearchRec.MonetaryTransactionCodeDesc;
            }


            return transferMessage;
        }

        private static TransferMessage EftDescriptionsAch(AccountHistorySearchInfo accountHistorySearchInfoResult)
        {
            var transferMessage = new TransferMessage();
            transferMessage.MemoDescription = "-";

            if (accountHistorySearchInfoResult.DepHistorySearchRec.EftDescInfo.ToArray().Length == 1)
            {
                transferMessage.TransactionDetail = accountHistorySearchInfoResult.DepHistorySearchRec.EftDescInfo[0].FreeFormatDescription;
                transferMessage.MemoDescription =
                    string.IsNullOrWhiteSpace(accountHistorySearchInfoResult.DepHistorySearchRec.EftDescInfo[0].FreeFormatDescription)
                    ? "-" : accountHistorySearchInfoResult.DepHistorySearchRec.EftDescInfo[0].FreeFormatDescription;

            }
            else if (accountHistorySearchInfoResult.DepHistorySearchRec.EftDescInfo.ToArray().Length == 4)
            {
                var customTransactionDetail = new StringBuilder();

                foreach (var eftDescription in accountHistorySearchInfoResult.DepHistorySearchRec.EftDescInfo)
                {
                    customTransactionDetail.Append(eftDescription.FreeFormatDescription + " ");
                }
                transferMessage.TransactionDetail = customTransactionDetail.ToString();
                transferMessage.ToBank = accountHistorySearchInfoResult.DepHistorySearchRec.EftDescInfo[0].FreeFormatDescription;
                transferMessage.TransferType = accountHistorySearchInfoResult.DepHistorySearchRec.EftDescInfo[1].FreeFormatDescription;
                transferMessage.Card = accountHistorySearchInfoResult.DepHistorySearchRec.EftDescInfo[2].FreeFormatDescription?.Substring(accountHistorySearchInfoResult.DepHistorySearchRec.EftDescInfo[2].FreeFormatDescription.Length - 4);
                transferMessage.TransferDate = accountHistorySearchInfoResult.DepHistorySearchRec.EftDescInfo[3].FreeFormatDescription;
                transferMessage.HasDetails = true;
            }
            else
            {
                transferMessage.TransactionDetail = accountHistorySearchInfoResult.DepHistorySearchRec.MonetaryTransactionCodeDesc;
            }

            return transferMessage;

        }

        private static TransferMessage EftDescriptionsInternalTransferCredit(AccountHistorySearchInfo accountHistorySearchInfoResult)
        {
            var transferMessage = new TransferMessage();
            transferMessage.MemoDescription = "-";

            if (accountHistorySearchInfoResult.DepHistorySearchRec.EftDescInfo.ToArray().Length == 4)
            {
                transferMessage.TransactionDetail = $"{accountHistorySearchInfoResult.DepHistorySearchRec.EftDescInfo[1].FreeFormatDescription} {accountHistorySearchInfoResult.DepHistorySearchRec.EftDescInfo[2].FreeFormatDescription}";
                transferMessage.MemoDescription =
                    string.IsNullOrWhiteSpace(accountHistorySearchInfoResult.DepHistorySearchRec.EftDescInfo[0].FreeFormatDescription)
                    ? "-" : accountHistorySearchInfoResult.DepHistorySearchRec.EftDescInfo[0].FreeFormatDescription;
                transferMessage.TransferType = accountHistorySearchInfoResult.DepHistorySearchRec.EftDescInfo[1].FreeFormatDescription;
                transferMessage.FromAccountOwnerName = accountHistorySearchInfoResult.DepHistorySearchRec.EftDescInfo[2].FreeFormatDescription?.Replace("from ", "").Replace("From ", "");
                transferMessage.HasDetails = true;
            }
            else
            {
                transferMessage.TransactionDetail = accountHistorySearchInfoResult.DepHistorySearchRec.MonetaryTransactionCodeDesc;
            }

            return transferMessage;

        }

        private static TransferMessage EftDescriptionsInternalTransferDebit(AccountHistorySearchInfo accountHistorySearchInfoResult)
        {
            var transferMessage = new TransferMessage();
            transferMessage.MemoDescription = "-";

            if (accountHistorySearchInfoResult.DepHistorySearchRec.EftDescInfo.ToArray().Length == 4)
            {
                transferMessage.TransactionDetail = $"{accountHistorySearchInfoResult.DepHistorySearchRec.EftDescInfo[1].FreeFormatDescription} {accountHistorySearchInfoResult.DepHistorySearchRec.EftDescInfo[3].FreeFormatDescription}";
                transferMessage.MemoDescription =
                    string.IsNullOrWhiteSpace(accountHistorySearchInfoResult.DepHistorySearchRec.EftDescInfo[0].FreeFormatDescription)
                    ? "-" : accountHistorySearchInfoResult.DepHistorySearchRec.EftDescInfo[0].FreeFormatDescription; ;
                transferMessage.TransferType = accountHistorySearchInfoResult.DepHistorySearchRec.EftDescInfo[1].FreeFormatDescription;
                transferMessage.AccountOwnerName = accountHistorySearchInfoResult.DepHistorySearchRec.EftDescInfo[3].FreeFormatDescription?.Replace("to ", "").Replace("To ", "");
                transferMessage.HasDetails = true;
            }
            else
            {
                transferMessage.TransactionDetail = accountHistorySearchInfoResult.DepHistorySearchRec.MonetaryTransactionCodeDesc;
            }

            return transferMessage;

        }

        private static TransferMessage EftDescriptionsWireTransferCredit(AccountHistorySearchInfo accountHistorySearchInfoResult)
        {
            var transferMessage = new TransferMessage();
            transferMessage.MemoDescription = "-";

            if (accountHistorySearchInfoResult.DepHistorySearchRec.EftDescInfo.ToArray().Length == 1)
            {
                transferMessage.TransactionDetail = accountHistorySearchInfoResult.DepHistorySearchRec.EftDescInfo[0].FreeFormatDescription;
                transferMessage.MemoDescription =
                    string.IsNullOrWhiteSpace(accountHistorySearchInfoResult.DepHistorySearchRec.EftDescInfo[0].FreeFormatDescription)
                    ? "-" : accountHistorySearchInfoResult.DepHistorySearchRec.EftDescInfo[0].FreeFormatDescription;
            }
            else if (accountHistorySearchInfoResult.DepHistorySearchRec.EftDescInfo.ToArray().Length == 10)
            {
                transferMessage.TransactionDetail = $"{accountHistorySearchInfoResult.DepHistorySearchRec.EftDescInfo[0].FreeFormatDescription} {accountHistorySearchInfoResult.DepHistorySearchRec.EftDescInfo[1].FreeFormatDescription}";
                transferMessage.TransferType = accountHistorySearchInfoResult.DepHistorySearchRec.EftDescInfo[0].FreeFormatDescription;
                transferMessage.FromAccountOwnerName = accountHistorySearchInfoResult.DepHistorySearchRec.EftDescInfo[1].FreeFormatDescription?.Replace("from ", "").Replace("From ", "");
                transferMessage.FromBank = accountHistorySearchInfoResult.DepHistorySearchRec.EftDescInfo[4].FreeFormatDescription;
                transferMessage.Receipt = accountHistorySearchInfoResult.DepHistorySearchRec.EftDescInfo[8].FreeFormatDescription;
                transferMessage.HasDetails = true;
            }
            else
            {
                transferMessage.TransactionDetail = accountHistorySearchInfoResult.DepHistorySearchRec.MonetaryTransactionCodeDesc;
            }

            return transferMessage;

        }

        private static TransferMessage EftDescriptionsWireTransferDebit(AccountHistorySearchInfo accountHistorySearchInfoResult)
        {
            var transferMessage = new TransferMessage();
            transferMessage.MemoDescription = "-";

            if (accountHistorySearchInfoResult.DepHistorySearchRec.EftDescInfo.ToArray().Length == 10)
            {
                transferMessage.TransactionDetail = $"{accountHistorySearchInfoResult.DepHistorySearchRec.EftDescInfo[0].FreeFormatDescription} {accountHistorySearchInfoResult.DepHistorySearchRec.EftDescInfo[8].FreeFormatDescription}";
                transferMessage.MemoDescription =
                    string.IsNullOrWhiteSpace(accountHistorySearchInfoResult.DepHistorySearchRec.EftDescInfo[5].FreeFormatDescription)
                    ? "-" : accountHistorySearchInfoResult.DepHistorySearchRec.EftDescInfo[5].FreeFormatDescription;
                transferMessage.TransferType = accountHistorySearchInfoResult.DepHistorySearchRec.EftDescInfo[0].FreeFormatDescription;
                transferMessage.ToAba = accountHistorySearchInfoResult.DepHistorySearchRec.EftDescInfo[2].FreeFormatDescription;
                transferMessage.ToAccountNumber = accountHistorySearchInfoResult.DepHistorySearchRec.EftDescInfo[3].FreeFormatDescription;
                transferMessage.ToBank = accountHistorySearchInfoResult.DepHistorySearchRec.EftDescInfo[4].FreeFormatDescription;
                transferMessage.AccountOwnerName = accountHistorySearchInfoResult.DepHistorySearchRec.EftDescInfo[8].FreeFormatDescription?.Replace("to ", "").Replace("To ", "");
                transferMessage.Receipt = accountHistorySearchInfoResult.DepHistorySearchRec.EftDescInfo[9].FreeFormatDescription;
                transferMessage.HasDetails = true;
            }
            else
            {
                transferMessage.TransactionDetail = accountHistorySearchInfoResult.DepHistorySearchRec.MonetaryTransactionCodeDesc;

            }

            return transferMessage;

        }

        private static TransferMessage EftDescriptionsInternalWireTransferFee(AccountHistorySearchInfo accountHistorySearchInfoResult)
        {
            var transferMessage = new TransferMessage();
            transferMessage.MemoDescription = "-";

            if (accountHistorySearchInfoResult.DepHistorySearchRec.EftDescInfo.ToArray().Length > 0)
            {
                var customTransactionDetail = new StringBuilder();

                foreach (var eftDescription in accountHistorySearchInfoResult.DepHistorySearchRec.EftDescInfo)
                {
                    customTransactionDetail.Append(eftDescription.FreeFormatDescription + " ");
                    transferMessage.TransactionDetail = customTransactionDetail.ToString();
                }
            }
            else
            {
                transferMessage.TransactionDetail = accountHistorySearchInfoResult.DepHistorySearchRec.MonetaryTransactionCodeDesc;
            }

            return transferMessage;

        }

        private static TransferMessage EftDescriptionsATM(AccountHistorySearchInfo accountHistorySearchInfoResult)
        {
            var transferMessage = new TransferMessage();
            transferMessage.MemoDescription = "-";

            if (accountHistorySearchInfoResult.DepHistorySearchRec.EftDescInfo.ToArray().Length == 4)
            {
                transferMessage.TransactionDetail = accountHistorySearchInfoResult.DepHistorySearchRec.EftDescInfo[0].FreeFormatDescription;
                transferMessage.Location = accountHistorySearchInfoResult.DepHistorySearchRec.EftDescInfo[1].FreeFormatDescription + accountHistorySearchInfoResult.DepHistorySearchRec.EftDescInfo[2].FreeFormatDescription;
                transferMessage.Card = accountHistorySearchInfoResult.DepHistorySearchRec.EftDescInfo[3].FreeFormatDescription?.Substring(accountHistorySearchInfoResult.DepHistorySearchRec.EftDescInfo[3].FreeFormatDescription.Length - 4);
                transferMessage.HasDetails = true;
            }
            else
            {
                transferMessage.TransactionDetail = accountHistorySearchInfoResult.DepHistorySearchRec.MonetaryTransactionCodeDesc;
            }

            return transferMessage;

        }

        private static TransferMessage EftDescriptionsDebitCard(AccountHistorySearchInfo accountHistorySearchInfoResult)
        {
            var transferMessage = new TransferMessage();
            transferMessage.MemoDescription = "-";

            if (accountHistorySearchInfoResult.DepHistorySearchRec.EftDescInfo.ToArray().Length == 1)
            {
                transferMessage.TransactionDetail = accountHistorySearchInfoResult.DepHistorySearchRec.EftDescInfo[0].FreeFormatDescription;
            }
            else if (accountHistorySearchInfoResult.DepHistorySearchRec.EftDescInfo.ToArray().Length == 4)
            {
                transferMessage.TransactionDetail = accountHistorySearchInfoResult.DepHistorySearchRec.EftDescInfo[0].FreeFormatDescription;
                transferMessage.AccountOwnerName = accountHistorySearchInfoResult.DepHistorySearchRec.EftDescInfo[1].FreeFormatDescription?.Replace("to ", "").Replace("To ", "");
                transferMessage.Location = accountHistorySearchInfoResult.DepHistorySearchRec.EftDescInfo[2].FreeFormatDescription;
                transferMessage.Card = accountHistorySearchInfoResult.DepHistorySearchRec.EftDescInfo[3].FreeFormatDescription?.Substring(accountHistorySearchInfoResult.DepHistorySearchRec.EftDescInfo[3].FreeFormatDescription.Length - 4);
                transferMessage.HasDetails = true;
            }
            else
            {
                transferMessage.TransactionDetail = accountHistorySearchInfoResult.DepHistorySearchRec.MonetaryTransactionCodeDesc;
            }

            return transferMessage;

        }

        private static TransferMessage EftDescriptionsAccountAnalysisCharge(AccountHistorySearchInfo accountHistorySearchInfoResult)
        {
            var transferMessage = new TransferMessage();
            transferMessage.MemoDescription = "-";

            if (accountHistorySearchInfoResult.DepHistorySearchRec.EftDescInfo.ToArray().Length == 1)
            {
                transferMessage.TransactionDetail = accountHistorySearchInfoResult.DepHistorySearchRec.EftDescInfo[0].FreeFormatDescription ?? Constants.BankingPackageFeesDescription; ;
            }
            else
            {
                transferMessage.TransactionDetail = accountHistorySearchInfoResult.DepHistorySearchRec.MonetaryTransactionCodeDesc;
            }

            return transferMessage;

        }

        private static TransferMessage EftDescriptionsNSFItemPaid(AccountHistorySearchInfo accountHistorySearchInfoResult)
        {
            var transferMessage = new TransferMessage();
            transferMessage.MemoDescription = "-";

            if (accountHistorySearchInfoResult.DepHistorySearchRec.EftDescInfo.ToArray().Length == 1)
            {
                transferMessage.TransactionDetail = accountHistorySearchInfoResult.DepHistorySearchRec.EftDescInfo[0].FreeFormatDescription ?? Constants.NSFFeeDescription;
            }
            else
            {
                transferMessage.TransactionDetail = accountHistorySearchInfoResult.DepHistorySearchRec.MonetaryTransactionCodeDesc;
            }

            return transferMessage;

        }

        private static TransferMessage EftDescriptionsMemo(AccountHistorySearchInfo accountHistorySearchInfoResult)
        {
            var transferMessage = new TransferMessage();
            transferMessage.MemoDescription = "-";

            if (accountHistorySearchInfoResult.DepHistorySearchRec.EftDescInfo.ToArray().Length == 1)
            {
                transferMessage.TransactionDetail = accountHistorySearchInfoResult.DepHistorySearchRec.EftDescInfo[0].FreeFormatDescription;
            }
            else
            {
                transferMessage.TransactionDetail = accountHistorySearchInfoResult.DepHistorySearchRec.MonetaryTransactionCodeDesc;
            }

            return transferMessage;

        }
    }
}