using System.Linq;
using System.Process.Domain.Containers;
using System.Process.Infrastructure.Adapters;
using System.Process.Infrastructure.Configs;
using System.Proxy.RdaAdmin.Common;
using System.Proxy.RdaAdmin.GetCustomersCriteria.Messages;
using System.Proxy.RdaAdmin.Messages;
using System.Proxy.RdaAdmin.UpdateCustomer.Messages;

namespace System.Process.Application.Commands.CreateCustomerId.Adapters
{
    public class UpdateCustomerAdapter : IAdapter<UpdateCustomerParams, PipelineMessageContainer<AdminBaseResult<GetCustomersCriteriaResponse>>>
    {
        #region Properties
        private RdaCredentialsConfig RdaConfig { get; set; }

        #endregion

        #region Constructor

        public UpdateCustomerAdapter
            (
            RdaCredentialsConfig rdaConfig
            )
        {
            RdaConfig = rdaConfig;
        }

        #endregion

        public UpdateCustomerParams Adapt(PipelineMessageContainer<AdminBaseResult<GetCustomersCriteriaResponse>> input)
        {
            return new UpdateCustomerParams
            {
                IsEnabled = true,
                City = input.Message.Result.Customers.First().City,
                AddressLine1 = input.Message.Result.Customers.First().AddressLine1,
                AddressLine2 = input.Message.Result.Customers.First().AddressLine2,
                CompanyName = input.Message.Result.Customers.First().CompanyName,
                Country = input.Message.Result.Customers.First().Country,
                EmailAddress = input.Message.Result.Customers.First().EmailAddress,
                FirstName = input.Message.Result.Customers.First().FirstName,
                HomeBankingId = input.Message.Result.Customers.First().HomeBankingId,
                IsDirectLoginEnabled = input.Message.Result.Customers.First().IsDirectLoginEnabled,
                LastName = input.Message.Result.Customers.First().LastName,
                PostalCode = input.Message.Result.Customers.First().PostalCode,
                State = input.Message.Result.Customers.First().State,
                Username = input.Message.Result.Customers.First().Username,
                Criteria = new Criteria
                {
                    HomeBankingId = input.Message.Result.Customers.First().HomeBankingId
                },
                Credentials = new Proxy.RdaAdmin.Common.Credentials
                {
                    EntityId = RdaConfig.Credentials.EntityId,
                    StoreId = RdaConfig.Credentials.StoreId,
                    StoreKey = RdaConfig.Credentials.StoreKey
                }
            };
        }
    }
}
