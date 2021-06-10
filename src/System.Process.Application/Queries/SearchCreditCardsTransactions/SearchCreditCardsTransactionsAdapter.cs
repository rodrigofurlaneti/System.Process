using System;
using System.Collections.Generic;
using System.Globalization;
using System.Process.Application.Queries.SearchCreditCardsTransactions.Response;
using System.Process.Domain.Constants;
using System.Process.Domain.Entities;
using System.Process.Domain.Enums;
using System.Process.Infrastructure.Adapters;
using System.Process.Infrastructure.Configs;
using System.Proxy.Rtdx.PendingActivityDetails.Messages;
using System.Proxy.Rtdx.PendingActivityDetails.Messages.Results;
using System.Proxy.Rtdx.TransactionDetails.Messages;
using System.Proxy.Rtdx.TransactionDetails.Messages.Result;

namespace System.Process.Application.Queries.SearchCreditCardsTransactions
{

    public class SearchCreditCardsTransactionsAdapter :
       IAdapter<TransactionDetailsParams, Card>,
       IAdapter<SearchCreditCardsTransactionsResponse, CreditCardsAdapterResponse>
    {
        #region Properties

        private RecordTypesConfig RecordTypesConfig { get; }
        private string SecurityToken { get; }

        #endregion

        #region Constructor

        public SearchCreditCardsTransactionsAdapter(RecordTypesConfig recordTypesConfig, string rtdxToken)
        {
            RecordTypesConfig = recordTypesConfig;
            SecurityToken = rtdxToken;
        }

        #endregion
        public TransactionDetailsParams Adapt(Card input)
        {
            if (input == null)
            {
                return null;
            }

            return new TransactionDetailsParams
            {
                AccountNumber = input.Pan,
                BillingCycleNumber = "00",
                SecurityToken = SecurityToken,
            };
        }
        public PendingActivityDetailsParams AdaptPending(Card input)
        {
            if (input == null)
            {
                return null;
            }

            return new PendingActivityDetailsParams
            {
                AccountNumber = input.Pan,
                SecurityToken = SecurityToken,
            };
        }
        public SearchCreditCardsTransactionsResponse Adapt(CreditCardsAdapterResponse input)
        {

            var searchCreditCardsTransactions = new SearchCreditCardsTransactionsResponse()
            {
                CardId = input.CardId,
                PendingTransactions = new List<TransactionItem>(),
                PostedTransactions = new List<TransactionItem>()
            };

            foreach (AuthorizationItems authorization in input.AuthorizationItems)
            {
                var transaction = new TransactionItem()
                {
                    MerchantName = authorization.MerchantId,
                    PostedDate = "",
                    TransactionAmount = Convert.ToDecimal(authorization.AuthorizationAmount),
                    TransactionCategory = Constants.TransactionCategory,
                    TransactionDateTime = FormatDate(authorization.AuthorizationDate.ToString()),
                    PrimaryKey = authorization.MerchantCategoryCode,
                    TransactionType = GetType(authorization.AuthorizationAmount)
                };
                searchCreditCardsTransactions.PendingTransactions.Add(transaction);
            }

            foreach (var statementLine in input.StatementLines)
            {
                var transaction = new TransactionItem()
                {
                    MerchantName = statementLine.MessageText1,
                    PostedDate = FormatDate(statementLine.PostedDate),
                    TransactionAmount = Convert.ToDecimal(statementLine.ItemAmount),
                    TransactionCategory = GetCategory(statementLine.TransactionCode),
                    TransactionDateTime = FormatDate(statementLine.TransactionDate),
                    PrimaryKey = statementLine.MerchantCategoryCode,
                    TransactionType = GetType(statementLine.ItemAmount.ToString())
                };
                searchCreditCardsTransactions.PostedTransactions.Add(transaction);
            }

            return searchCreditCardsTransactions;
        }

        private string FormatDate(string date)
        {
            if (date.Length <= 4)
            {
                var currentDate = DateTime.Now;
                date = currentDate.Year + date;
            }

            string result = DateTime.ParseExact(date, "yyyyMMdd",
                CultureInfo.InvariantCulture).ToString("yyyy-MM-dd'T'HH:mm:ss");

            return result;
        }

        private string GetType(string amount)
        {
            if (amount.Trim().Replace(" ", "").IndexOf("-") != -1)
            {
                return "D";
            }
            return "C";
        }

        private string GetCategory(string category)
        {
            var transactionIconsEnum = (TransactionIcons)Enum.Parse(typeof(TransactionIcons), category);
            var transactionIcon = Enum.GetName(typeof(TransactionIcons), transactionIconsEnum).ToLower();

            string resposneCategory = Constants.Other;

            if (transactionIcon.IndexOf("fee") != -1)
            {
                resposneCategory = Constants.FeeCharge;
            }
            else if (transactionIcon.IndexOf("not") != -1)
            {
                resposneCategory = "Not Applicable to System";

            }
            else if (transactionIcon.IndexOf("statement") != -1)
            {
                resposneCategory = Constants.Statement;

            }
            else if (transactionIcon.IndexOf("cash") != -1)
            {
                resposneCategory = Constants.Cash;

            }
            else if (transactionIcon.IndexOf("refund") != -1)
            {
                resposneCategory = Constants.RefundReward;

            }
            return resposneCategory;
        }
    }
}