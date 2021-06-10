using System;
using System.Collections.Generic;
using System.Text;

namespace System.Process.Domain.ValueObjects
{
    public class Term
    {
        public string Type { get; set; }
        public string Version { get; set; }
        public bool Accept { get; set; } = false;
        public DateTime AcceptanceDate { get; set; }
    }
}
