using System;
using System.Process.Infrastructure.Adapters;
using System.Proxy.Rda.Authenticate.Messages;
using System.Proxy.Rda.Common;
using System.Proxy.Rda.CreateBatch.Messages;

namespace System.Process.Application.Commands.RemoteDepositCapture.Adapters
{
    public class CreateBatchAdapter : IAdapter<CreateBatchRequest, AuthenticateResponse>
    {
        public CreateBatchRequest Adapt(AuthenticateResponse input)
        {
            return new CreateBatchRequest
            {
                BatchNumber = new Random().Next(1, 999999).ToString(),
                Credentials = new TokenCredentials
                {
                    SecurityToken = input.Credentials.SecurityToken,
                    Type = input.Credentials.Type
                }
            };
        }
    }
}
