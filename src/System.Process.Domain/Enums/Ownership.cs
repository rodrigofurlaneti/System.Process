using System;
using System.Collections.Generic;
using System.Text;

namespace System.Process.Domain.Enums
{
    public enum Ownership
    {
        [Name("Same")]
        S,

        [Name("Other")]
        O,

        [Name("All")]
        A,

    }
}
