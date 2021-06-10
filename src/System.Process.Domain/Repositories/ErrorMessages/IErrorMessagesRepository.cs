namespace System.Process.Domain.Repositories.ErrorMessages
{
    public interface IErrorMessagesRepository
    {
        Entities.ErrorMessages FindErrorCodeByCode(string code);
    }
}