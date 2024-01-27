using SparseBitsets;
using System.Collections.Generic;

namespace SparseBitsetUnitTests
{
    public static class SparseBitsetTestsExtension
    {
        public static SparseBitset ToBitset(this IEnumerable<uint> bitValues)
        {
            var bitset = new SparseBitset();

            foreach (var bitValue in bitValues)
            {
                bitset.Add(bitValue);
            }
            
            return bitset;
        }

        public static SparseBitset ToOptimizedBitset(this IEnumerable<uint> bitValues)
        {
            var bitset = new SparseBitset();

            foreach (var bitValue in bitValues)
            {
                bitset.Add(bitValue);
            }

            bitset.Pack();
            
            return bitset;
        }

        public static SparseBitset ToOptimizedBitset(this IEnumerable<Run> runs)
        {
            return new SparseBitset(runs);

        }

    }
}
