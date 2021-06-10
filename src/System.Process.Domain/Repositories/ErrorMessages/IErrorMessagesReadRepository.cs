namespace System.Process.Domain.Repositories.ErrorMessages
{
    public interface IErrorMessagesReadRepository
    {
        Entities.ErrorMessages FindErrorCodeByCode(string code);
    }
}