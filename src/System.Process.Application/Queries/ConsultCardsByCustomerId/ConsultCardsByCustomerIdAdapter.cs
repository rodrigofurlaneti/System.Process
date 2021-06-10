using System.Process.Application.DataTransferObjects;
using System.Process.Infrastructure.Adapters;
using System.Collections.Generic;

namespace System.Process.Application.Queries.ConsultCardsByCustomerId
{
    public class ConsultCardsByCustomerIdAdapter :
        IAdapter<ConsultCardsByCustomerIdResponse, IList<CustomerCardDto>>
    {
        #region IAdapter implementation
        public ConsultCardsByCustomerIdResponse Adapt(IList<CustomerCardDto> cards)
        {
            return new ConsultCardsByCustomerIdResponse
            {
                CardRecords = cards
            };
        }

        #endregion

        #region Methods
        #endregion
    }
}
