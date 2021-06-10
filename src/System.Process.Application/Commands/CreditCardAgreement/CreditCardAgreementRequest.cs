using System.Collections.Generic;
using MediatR;
using System.Process.Application.Commands.CreditCardAgreement.Request;
using System.Process.Application.DataTransferObjects;

namespace System.Process.Application.Commands.CreditCardAgreement
{
    public class CreditCardAgreementRequest : IRequest<CreditCardAgreementResponse>
    {
        public string AssetId { get; set; }
        public string SystemId { get; set; }
        public IList<TermDto> Terms { get; set; }
        public CreditCardAddress Address { get; set; }
    }
}
