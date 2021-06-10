namespace System.Process.Domain.Constants
{
    /// <summary>
    /// This class should be used to add business error 
    /// If the current proxy does not return code we can create our own
    /// <para>
    /// MessageDecoderMiddleware is responsible for translating the code into a message
    /// </para>
    /// Example:
    /// <code>
    /// throw new UnprocessableEntityException("This message is replaced if found in message decoder", new ErrorStructure(ErrorCodes.AVAILABLE_BALANCE_EXCEEDED))
    /// </code>
    /// </summary>
    public static class ErrorCodes
    {
        public static readonly string AvailableBalanceExceeded = "Process:01";
        public static readonly string NoAddressAvailable = "Process:02";
    }

    public static class Providers
    {
        public static readonly string JackHenry = "JH";
    }
}