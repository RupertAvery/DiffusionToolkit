using SparseBitsets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SparseBitsetUnitTests
{
    public class BitsetHelpers
    {
        public static IEnumerable<uint> ToValues(uint start, string expression)
        {
            expression = expression.Replace(" ", "", StringComparison.InvariantCulture);
            int ptr = 0;
            while (ptr < expression.Length)
            {
                if (expression[ptr] == '*')
                {
                    yield return start + (uint)ptr;
                }
                ptr++;
            }
        }

        public static uint ToValue(string expression)
        {
            expression = expression.Replace(" ", "", StringComparison.InvariantCulture);
            expression = new string(expression.Reverse().ToArray());
            uint value = 0;
            int ptr = 0;
            while (ptr < expression.Length)
            {
                if (expression[ptr] == '*')
                {
                    value |= ((uint)1 << ptr);
                }
                ptr++;
            }

            return value;
        }

        public static IEnumerable<Run> ToRuns(uint startKey, string expression)
        {
            return ToRuns(startKey, expression, null);
        }

        public static IEnumerable<Run> ToRuns(uint startKey, string expression, Dictionary<char, uint> valueLookup)
        {
            expression = expression.Replace(" ", "", StringComparison.InvariantCulture);
            int ptr = 0;
            Run currentRun = null;
            uint[] buffer = new uint[256];
            var bufferPtr = 0;
            var sb = new StringBuilder();
            while (ptr < expression.Length)
            {
                if (expression[ptr] != '-')
                {

                    if (currentRun == null)
                    {
                        currentRun = new Run()
                        {
                            Start = (ushort)(startKey + ptr),
                        };
                    }

                    uint value;
                    if (expression[ptr] == '*')
                    {
                        value = uint.MaxValue;
                    }
                    else
                    {
                        value = valueLookup[expression[ptr]];
                    }
                    buffer[bufferPtr] = value;
                    bufferPtr++;

                }
                else if (expression[ptr] == '-')
                {
                    if (currentRun != null)
                    {
                        currentRun.Values = new uint[bufferPtr];
                        Array.Copy(buffer, currentRun.Values, bufferPtr);
                        currentRun.End = (ushort)(startKey + ptr - 1);
                        yield return currentRun;
                        currentRun = null;
                        bufferPtr = 0;
                    }
                }
                ptr++;
            }

            if (currentRun != null)
            {
                currentRun.Values = new uint[bufferPtr];
                Array.Copy(buffer, currentRun.Values, bufferPtr);
                currentRun.End = (ushort)(startKey + ptr - 1);
                yield return currentRun;
            }
        }
    }
}