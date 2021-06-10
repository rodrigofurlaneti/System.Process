using System.Process.Domain.Entities;
using System.Process.Domain.ValueObjects;
using System.Proxy.Rtdx.BalanceInquiry.Messages;

namespace System.Process.Application.Commands.CreditCardBalance
{
    public class CreditCardBalanceAdapter
    {
        #region Properties

        private ProcessConfig ProcessConfig { get; }

        #endregion

        #region Constructor

        public CreditCardBalanceAdapter(ProcessConfig config)
        {
            ProcessConfig = config;
        }

        public CreditCardBalanceAdapter()
        {

        }

        #endregion


        public BalanceInquiryParams Adapt(Card input, string token)
        {
            return new BalanceInquiryParams
            {
                SecurityToken = token,
                AccountNumber = input.Pan,
                Application = ProcessConfig.BalanceInquiryAplication
            };
        }
    }
}