using System.Process.Infrastructure.Adapters;
using System.Proxy.Fis.TransactionDetail.Messages;
using System.Proxy.Fis.ValueObjects;

namespace System.Process.Application.Commands.TransactionDetail
{
    public class TransactionDetailAdapter : IAdapter<TransactionDetailParams, TransactionDetailRequest>
    {
        public TransactionDetailParams Adapt(TransactionDetailRequest input)
        {
            return new TransactionDetailParams
            {
                Pan = new Pan
                {
                    Alias = "",
                    PlainText = input.Pan,
                    CipherText = ""
                },
                PrimaryKey = input.PrimaryKey
            };
        }
    }
}
