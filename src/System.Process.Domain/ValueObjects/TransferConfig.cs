namespace System.Process.Domain.ValueObjects
{
    public class TransferConfig
    {
        public string AchPaymentType { get; set; }
        public string AchDirection { get; set; }
        public string ConsumerNameACH { get; set; }
        public string ConsumerProductACH { get; set; }
        public string WirePaymentType { get; set; }
        public string WireDirection { get; set; }
        public string WirePaymentSubType { get; set; }
        public string XferPaymentType { get; set; }
        public string XferDirection { get; set; }
        public string XferPaymentSubType { get; set; }
        public string RdcPaymentType { get; set; }
        public string RdcDirection { get; set; }
        public string RdcPaymentSubType { get; set; }
        public string ReceiverBankFeeType { get; set; }
        public string SenderProcesstatus { get; set; }
        public string Frequency { get; set; }
        public string SenderCorrespondentBankFeeType { get; set; }
        public string ReceiverTransactionFeeType { get; set; }
        public string GeographicScope { get; set; }
        public string ReceiverCorrespondentBankFeeType { get; set; }
        public string SenderTransactionFeeType { get; set; }
        public string ReceiverProcesstatus { get; set; }
        public string ChannelType { get; set; }
        public string SenderBankFeeType { get; set; }
        public string PaymentStatus { get; set; }
        public string XferConsumerName { get; set; }
        public string XferConsumerProd { get; set; }
    }
}