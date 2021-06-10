using System.Process.Domain.Repositories.ErrorMessages;

namespace System.Process.Infrastructure.Repositories.MongoDb.ErrorMessages
{
    public class ErrorMessagesRepository : IErrorMessagesRepository
    {
        private IErrorMessagesReadRepository ReadRepository { get; }

        public ErrorMessagesRepository(IErrorMessagesReadRepository readRepository)
        {
            ReadRepository = readRepository;
        }

        public Domain.Entities.ErrorMessages FindErrorCodeByCode(string code)
        {
            return ReadRepository.FindErrorCodeByCode(code);
        }
    }
}