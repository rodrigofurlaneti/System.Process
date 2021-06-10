using System.Process.Domain.Repositories.ErrorMessages;
using System.Phoenix.DataAccess.MongoDb;

namespace System.Process.Infrastructure.Repositories.MongoDb.ErrorMessages
{
    public class ErrorMessagesReadRepository : IErrorMessagesReadRepository
    {
        private MongoDbClient<Domain.Entities.ErrorMessages, string> ErrorCodeClient { get; }

        public ErrorMessagesReadRepository(MongoDbClient<Domain.Entities.ErrorMessages, string> errorCodeClient)
        {
            ErrorCodeClient = errorCodeClient;
        }

        public Domain.Entities.ErrorMessages FindErrorCodeByCode(string code)
        {
            return ErrorCodeClient.Find(x => x.ErrorCode == code);
        }
    }
}