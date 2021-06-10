namespace System.Process.Infrastructure.Adapters
{
    public interface IAdapter<TResult, TInput>
    {
        TResult Adapt(TInput input);
    }
}
