using System.Process.Application.Commands.RemoteDepositCapture.Message;
using System.Collections.Generic;

namespace System.Process.Application.Commands.RemoteDepositCapture
{
    public class RemoteDepositCaptureResponse
    {
        public bool Success { get; set; }
        public string ToAccount { get; set; }
        public string ToRountingNumber { get; set; }
        public long? GeoLocation { get; set; }
        public int Count { get; set; }
        public decimal TotalAmount { get; set; }
        public List<ResponseItem> Item { get; set; }
        public List<string> Errors { get; set; }
        public bool OnHold { get; set; }

    }
}
