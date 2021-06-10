using System;
using System.Collections.Generic;
using System.Text;
using System.Process.Infrastructure.Adapters;
using System.Proxy.Rtdx.PaymentInquiry.Messages;

namespace System.Process.Application.Commands.CreditCardMakePayment
{
    public class CreditCardMakePaymentAdapter : IAdapter<CreditCardMakePaymentResponse, PaymentInquiryResult>
    {
        #region IAdapter

        public CreditCardMakePaymentResponse Adapt(PaymentInquiryResult input)
        {
            return new CreditCardMakePaymentResponse
            {
                AlreadyPaid = int.Parse(input.TotalDueNow) == 0,
                DueDate = input.PaymentDueDate,
                PastDue = int.Parse(input.TotalPastDueAmount) != 0,
            };
        }

        #endregion
    }
}
