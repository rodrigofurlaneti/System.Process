using System.Process.Application.Queries.ConsultProcessByCustomerId;
using System.Proxy.Silverlake.Inquiry.Common;
using System.Proxy.Silverlake.Inquiry.Messages.Response;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace System.Process.UnitTests.Application.Queries.ConsultProcess
{
    public class ConsultProcessQueryTests
    {
        #region Tests

        //[Fact(DisplayName = "Should Send Handle Successfully")]
        //public async Task ShouldSendHandleSuccessfullyAsync()
        //{
        //    var inquiryOperation = new Mock<IInquiryOperation>();
        //    var ProcessearchResponse = GetProcessearchResponse();
        //    inquiryOperation.Setup(x => x.ProcessearchAsync(It.IsAny<ProcessearchRequest>())).Returns(ProcessearchResponse);
        //    var logger = new Mock<ILogger<ConsultProcessByCustomerIdQuery>>();            
        //    var consultProcessQuery = new ConsultProcessByCustomerIdQuery(inquiryOperation.Object, logger.Object);
        //    var request = GetConsultProcessRequest();
        //    var cancellationToken = new CancellationToken();

        //    var result = await consultProcessQuery.Handle(request, cancellationToken);

        //    Assert.NotNull(result);
        //}

        //[Fact(DisplayName = "Should Throw NotFoundException On ConsultAccount Method")]
        //public async Task ShouldThrowNotFoundExceptionOnConsultAccount()
        //{
        //    var inquiryOperation = new Mock<IInquiryOperation>();
        //    inquiryOperation.Setup(x => x.ProcessearchAsync(It.IsAny<ProcessearchRequest>())).Throws(new Exception());
        //    var logger = new Mock<ILogger<ConsultProcessByCustomerIdQuery>>();
        //    var consultProcessQuery = new ConsultProcessByCustomerIdQuery(inquiryOperation.Object, logger.Object);
        //    var request = GetConsultProcessRequest();
        //    var cancellationToken = new CancellationToken();

        //    await Assert.ThrowsAsync<NotFoundException>(() => consultProcessQuery.Handle(request, cancellationToken));
        //}

        #endregion

        #region Methods

        private ConsultProcessByCustomerIdRequest GetConsultProcessRequest()
        {
            return new ConsultProcessByCustomerIdRequest("string");
        }

        private async Task<ProcessearchResponse> GetProcessearchResponse()
        {
            return await Task.FromResult(new ProcessearchResponse
            {
                ProcessearchRecInfo = new List<ProcessearchRecInfo>
                {
                    new ProcessearchRecInfo
                    {
                        AccountId = new AccountId
                        {
                            AccountNumber = "string",
                            AccountType = "string"
                        },
                        Amount = 123,
                        Processtatus = "2",
                        AvailableBalance = 12,
                        ProductCode = "string",
                        ProductDesc = "string",
                        ProcesstatusDesc = "Closed"
                    }
                }
            });
        }

        #endregion
    }
}
