using System;

namespace System.Process.Domain.Enums
{
    public enum AccountType
    {
        [Name("Deposit")]
        [ElementInfo("x_DepInfoRec")]
        [StatusProperty("StatusDep")]
        D,

        [Name("Safe")]
        [ElementInfo("x_SafeDepInfoRec")]
        [StatusProperty("StatusSafe")]
        S

        //[Name("TimeDeposit")]
        //[ElementInfo("x_TimeDepInfoRec")]
        //[StatusProperty("StatusCode")]
        //T,

        //[Name("Loan")]
        //[ElementInfo("x_LnInfoRec")]
        //[StatusProperty("CurrentStatus")]
        //L
    }

    public class StatusPropertyAttribute : Attribute
    {
        private string Value { get; }

        public StatusPropertyAttribute(string value)
        {
            Value = value;
        }
        public string GetValue
        {
            get { return Value; }
        }

    }

    public class NameAttribute : Attribute
    {
        private string Value { get; }

        public NameAttribute(string value)
        {
            Value = value;
        }
        public string GetValue
        {
            get { return Value; }
        }

    }

    public class ElementInfoAttribute : Attribute
    {
        private string Value { get; }

        public ElementInfoAttribute(string value)
        {
            Value = value;
        }
        public string GetValue
        {
            get { return Value; }
        }
    }
}
