using System;
using System.Collections.Generic;
using System.Text;

namespace System.Process.Application.Commands.CreditCardMakePayment
{
    public class CreditCardMakePaymentResponse
    {
        public string DueDate { get; set; }
        public bool AlreadyPaid { get; set; }
        public bool PastDue { get; set; }

    }
}
