using System.Diagnostics;

namespace SparseBitsets
{
    [DebuggerDisplay("Start = {Start}, End = {End}")]
    public class Run
    {
        public uint Start { get; set; }
        public uint End { get; set; }
        public uint[] Values { get; set; }
    }
}