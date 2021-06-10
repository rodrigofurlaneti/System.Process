using MediatR;
using Microsoft.Extensions.Logging;
using System.Process.Domain.Constants;
using System.Process.Domain.Enums;
using System.Process.Domain.Repositories;
using System.Phoenix.Common.Exceptions;
using System.Proxy.Silverlake.Inquiry;
using System.Proxy.Silverlake.Inquiry.Messages.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace System.Process.Application.Queries.GetAccountHistory
{
    public class GetAccountHistoryQuery : IRequestHandler<GetAccountHistoryRequest, GetAccountHistoryResponse>
    {
        #region Properties
        private IInquiryOperation InquiryOperation { get; }
        private ILogger<GetAccountHistoryQuery> Logger { get; }
        private ITransactionReadRepository TransactionReadRepository { get; }
        #endregion

        #region Constructor
        public GetAccountHistoryQuery(
            IInquiryOperation inquiryOperation,
            ILogger<GetAccountHistoryQuery> logger,
            ITransactionReadRepository transactionReadRepository)
        {
            InquiryOperation = inquiryOperation;
            Logger = logger;
            TransactionReadRepository = transactionReadRepository;
        }
        #endregion

        #region INotificationHandler implementation

        public async Task<GetAccountHistoryResponse> Handle(GetAccountHistoryRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var adapt = new GetAccountHistoryAdapter();
                var paramsProcessearch = adapt.Adapt(request.AccountNumber);
                var ProcessearchResult = await InquiryOperation.ProcessearchAsync(paramsProcessearch, cancellationToken);

                if (ProcessearchResult?.ProcessearchRecInfo?.Count == 0)
                {
                    Logger.LogInformation($"AccountId: {request.AccountNumber} not found.");
                    throw new NotFoundException($"AccountId: {request.AccountNumber} not found.");
                }

                var result = new AccountHistorySearchResponse();
                var acctStatus = Enum.Parse<Processtatus>(ProcessearchResult?.ProcessearchRecInfo.FirstOrDefault().Processtatus);
                switch (acctStatus)
                {
                    case Processtatus.Escheat:
                    case Processtatus.Closed:
                    case Processtatus.Dormant:
                    case Processtatus.ChargedOff:
                    case Processtatus.NoCredits:
                        Logger.LogInformation($"Account Status: {acctStatus} not valid.");
                        throw new UnprocessableEntityException($"Account Status: {acctStatus} not valid for querying transaction history.");
                    case Processtatus.Active:
                    case Processtatus.NewToday:
                    case Processtatus.PendingClosed:
                    case Processtatus.Restricted:
                    case Processtatus.NoPost:
                        result = await InquiryOperation.AccountHistorySearchAsync(adapt.Adapt(request), cancellationToken);
                        break;
                }

                result = Filter(request.Filter, result);
                var response = adapt.Adapt(result);

                var transactionInfo = TransactionReadRepository.Find();

                foreach (var item in response.TransactionHistory)
                {
                    var adaptedItem = adapt.Adapt(item);

                    item.GroupName = transactionInfo
                        .Where(i => i.TransactionCategories.Where(x => x.Code == adaptedItem.Code).ToList().Count > 0)
                        .FirstOrDefault()?.Label;
                }

                if (request.TransactionGroupName != null)
                {
                    response.TransactionHistory = response.TransactionHistory.Where(x => x.GroupName == request.TransactionGroupName).ToList();
                }

                switch (request.SortMethod)
                {
                    case SortMethod.NewestDate:
                        response.TransactionHistory = response.TransactionHistory.OrderByDescending(item => item.PostedDate).ToList();
                        break;
                    case SortMethod.OldestDate:
                        response.TransactionHistory = response.TransactionHistory.OrderBy(item => item.PostedDate).ToList();
                        break;
                    case SortMethod.HighestAmount:
                        response.TransactionHistory = response.TransactionHistory.OrderByDescending(item => item.Type == Constants.CreditTransactionType ? item.Amount : item.Amount * -1).ToList();
                        break;
                    case SortMethod.LowestAmount:
                        response.TransactionHistory = response.TransactionHistory.OrderBy(item => item.Type == Constants.CreditTransactionType ? item.Amount : item.Amount * -1).ToList();
                        break;
                    default:
                        response.TransactionHistory = response.TransactionHistory.OrderByDescending(item => item.PostedDate).ToList();
                        break;
                }

                return response;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Unexpected error during GetAccountHistoryQuery");
                throw;
            }
        }

        #endregion

        private AccountHistorySearchResponse Filter(string filter, AccountHistorySearchResponse result)
        {
            if (filter == null)
            {
                return result;
            }

            var amount = new decimal();
            var isAmount = decimal.TryParse(filter, out amount);

            var filtered = new AccountHistorySearchResponse();
            filtered.AccountHistorySearchInfo = new List<AccountHistorySearchInfo>();

            filtered.AccountHistorySearchInfo = result.AccountHistorySearchInfo.FindAll(history =>
                (isAmount ? history.DepHistorySearchRec.Amount == amount : false) ||
                history.DepHistorySearchRec.SourceCodeDescription.ToUpper().Contains(filter.ToUpper()));

            return filtered;
        }
    }
}

