using System.Process.Domain.Entities;

namespace System.Process.Domain.ValueObjects
{
    public class RtdxCardToken
    {
        public Card Card { get; set; }
        public string SecurityToken { get; set; }
    }
}
