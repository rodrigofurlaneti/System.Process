using System;

namespace System.Process.Application.DataTransferObjects
{
    public sealed class DepositInformationRecordDto
    {
        public string AccountClassificationCode { get; set; }
        public string ServiceChargeAccountReason { get; set; }
        public string ServiceChargeAccountReasonCode { get; set; }
        public string SignatureVerifyCode { get; set; }
        public string BusinessCIF { get; set; }
        public string OverDraftPrivilegeOptionType { get; set; }
        public DateTime OpenDate { get; set; }
        public string ServiceChargeWaived { get; set; }
        public string SignatureVerificationCode { get; set; }
        public string ServiceChargeWaivedReasonCode { get; set; }
        public string OverdraftPrvgOption { get; set; }
    }
}