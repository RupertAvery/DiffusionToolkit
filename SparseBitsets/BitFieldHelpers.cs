using System.Runtime.CompilerServices;

namespace SparseBitsets
{
    public class BitFieldHelpers
    {
        public static uint CountSetBitsFastOld(ulong i)
        {
            unchecked
            {
                var j = count32((uint)(i & 0xffff_ffff));
                j += count32((uint)((i >> 32) & 0xffff_ffff));
                return j;

                uint count32(uint v)
                {
                    long c;
                    c = v - ((v >> 1) & 0x55555555);
                    c = ((c >> 2) & 0x33333333) + (c & 0x33333333);
                    c = ((c >> 4) + c) & 0x0F0F0F0F;
                    c = ((c >> 8) + c) & 0x00FF00FF;
                    c = ((c >> 16) + c) & 0x0000FFFF;
                    return (uint)c;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint CountSetBitsFast(uint v)
        {
            long c;
            c = v - ((v >> 1) & 0x55555555);
            c = ((c >> 2) & 0x33333333) + (c & 0x33333333);
            c = ((c >> 4) + c) & 0x0F0F0F0F;
            c = ((c >> 8) + c) & 0x00FF00FF;
            c = ((c >> 16) + c) & 0x0000FFFF;
            return (uint)c;

            //unchecked
            //{
            //    x -= (x >> 1) & 0x5555555555555555UL; //put count of each 2 bits into those 2 bits
            //    x = (x & 0x3333333333333333UL) + ((x >> 2) & 0x3333333333333333UL); //put count of each 4 bits into those 4 bits 
            //    x = (x + (x >> 4)) & 0x0F0F0F0F0F0F0F0FUL; //put count of each 8 bits into those 8 bits 
            //    return (int)((x * 0x0101010101010101UL) >> 56);
            //}
        }
    }
}