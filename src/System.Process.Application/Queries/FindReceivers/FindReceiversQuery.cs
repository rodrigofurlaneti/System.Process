using MediatR;
using Microsoft.Extensions.Logging;
using System.Process.Domain.Entities;
using System.Process.Domain.Enums;
using System.Process.Domain.Repositories;
using System.Phoenix.Common.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace System.Process.Application.Queries.FindReceivers
{
    public class FindReceiversQuery : IRequestHandler<FindReceiversRequest, FindReceiversResponse>
    {
        #region Properties
        private ILogger<FindReceiversQuery> Logger { get; }
        private IReceiverReadRepository ReceiverReadRepository { get; }

        #endregion

        #region Constructor
        public FindReceiversQuery(
            ILogger<FindReceiversQuery> logger,
            IReceiverReadRepository receiverReadRepository)
        {
            Logger = logger;
            ReceiverReadRepository = receiverReadRepository;
        }

        #endregion

        #region IRequestHandler implementation

        public async Task<FindReceiversResponse> Handle(FindReceiversRequest request, CancellationToken cancellationToken)
        {
            try
            {
                Logger.LogInformation("Starting process for retrieving receiver list");

                return await Task.FromResult(RetrieveReceiverList(request));
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                throw;
            }
        }

        private FindReceiversResponse RetrieveReceiverList(FindReceiversRequest request)
        {
            try
            {
                var response = new FindReceiversResponse();
                var adapter = new FindReceiversAdapter();
                List<Receiver> result;

                var adaptedInquiryType = (OriginAccount)Enum.Parse(typeof(OriginAccount), request.InquiryType);
                var adaptedOwnership = (Ownership)Enum.Parse(typeof(Ownership), request.Ownership);
                // Search All BankType and All Ownership
                if (adaptedInquiryType.Equals(OriginAccount.A) && adaptedOwnership.Equals(Ownership.A))
                {
                    result = ReceiverReadRepository.FindByCustomerId(request.CustomerId).ToList();
                }
                // Search By BankType Only
                else if (!adaptedInquiryType.Equals(OriginAccount.A) && adaptedOwnership.Equals(Ownership.A))
                {
                    result = ReceiverReadRepository.FindByBankType(request.CustomerId, request.InquiryType).ToList();
                }
                // Search By OwnerShip Only
                else if (!adaptedOwnership.Equals(Ownership.A) && adaptedInquiryType.Equals(OriginAccount.A))
                {
                    result = ReceiverReadRepository.FindByOwnerShip(request.CustomerId, request.Ownership).ToList();
                }
                // Search By BankType And Onwership Specific
                else
                {
                    result = ReceiverReadRepository.FindByBankTypeAndOwnership(request.CustomerId, request.InquiryType, request.Ownership).ToList();
                }

                if (result.Count == 0)
                {
                    throw new NotFoundException($"No receiver found under {request.InquiryType} and {request.Ownership} filter");
                }

                response.ReceiverList = adapter.Adapt(result);

                return response;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                throw;
            }
        }

        #endregion
    }
}
