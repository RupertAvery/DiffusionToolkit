#define NOLOOPS
#define Manual4

using System;
using System.Runtime.CompilerServices;

#if NETCOREAPP3_1
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
#endif

namespace SparseBitsets
{
    /*
     * An optimized sparse bitset stores adjacent key-value pairs as only an array (run) of the values,
     * along with information about the start and end of the run.
     *
     * For example, if we have the following key value pairs in our SparseBitset
     *
     *  { Key: 100, Value: 1 }, { Key: 101, Value: 2 }, { Key: 102, Value: 3 }, { Key: 104, Value: 4 }
     *
     * This could be translated into:
     *
     *  { Start: 100, End: 103:, Values: [1, 2, 3, 4] }
     *
     * This can reduce the size of a SparseBitset up to 50%.
     * Since most of the questions have much fewer choices than respondents, it's more than likely most of the
     * key-value pairs in the sparse bitset are adjacent, so it should benefit 95% of the group memberships.
     *
     * Additionally, storing them as (sorted) runs allows us to access the values sequentially. While the runs
     * may occasionally be staggered, i.e. the start and end values don't align, it nevertheless allows us to
     * compare the values element-by-element in sequence, making AND and OR operations up to 20x faster than
     * using the Dictionary lookup approach.
     *
     */


    public static class SparseBitsetOptimzedOperators
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsOverlapping(Run a, Run b)
        {
            return (a.Start <= b.End && b.Start <= a.End);
        }

        /// <summary>
        /// Returns the logical And of two optimized bitsets
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static SparseBitset And(SparseBitset a, SparseBitset b)
        {
            var result = new SparseBitset();

            // Our purpose here is to merge Bitsets only where they overlap:
            // Here we have two bitsets with 1 run each.
            // Each letter here represents a Value element in a Run. A dash is an unused key.
            // 
            // The figure below represents two staggered runs (they don't align) A and B, with C containing
            // the ANDed Value elements.
            //              
            // -------------AAAAAAAAAAAAAAAAA
            // ----------BBBBBBBBB-----------
            //
            //--------------CCCCCC-----------

            var ptrA = 0;
            var ptrB = 0;
            var ptrC = 0;
            var currentA = 0;
            var currentB = 0;

            // If either of the runs are empty, return an empty bitset.
            if (a.Runs.Count == 0)
            {
                return result;
            }

            if (b.Runs.Count == 0)
            {
                return result;
            }

            // We start with the first Run. They may or may not overlap.

            var currentRunA = a.Runs[currentA];
            var currentRunB = b.Runs[currentB];
            Run currentRunC = null;

            while (currentA < a.Runs.Count && currentB < b.Runs.Count)
            {
                // after the end of any Run, we write the current Run to the output and start anew
                if (currentRunC != null)
                {
                    result.Runs.Add(currentRunC);
                    currentRunC = null;
                    ptrC = 0;
                }

                /// Check if the runs overlap
                if (IsOverlapping(currentRunA, currentRunB))
                {
                    // If there is no current run, create one
                    if (currentRunC == null)
                    {
                        currentRunC = new Run();
                        currentRunC.Start = Math.Max(currentRunA.Start, currentRunB.Start);
                        currentRunC.End = Math.Min(currentRunA.End, currentRunB.End);
                    }
                    else
                    {
                        currentRunC.End = Math.Min(currentRunC.End, Math.Min(currentRunA.End, currentRunB.End));
                    }

                    // Check if we need to initialize, or expand the Values array
                    if (currentRunC.Values == null || currentRunC.Values.Length < currentRunC.End - currentRunC.Start + 1)
                    {
                        var newArray = new uint[currentRunC.End - currentRunC.Start + 1];
                        if (currentRunC.Values != null)
                        {
                            // Copy the old values to the new array
                            Array.Copy(currentRunC.Values, newArray, currentRunC.Values.Length);
                        }
                        currentRunC.Values = newArray;
                    }

#if NOLOOPS
                    var diffA = (int)((ptrB + currentRunB.Start) - (ptrA + currentRunA.Start));

                    if (diffA > 0)
                    {
                        ptrA += diffA;
                    }
#else
                    while (ptrA + currentRunA.Start < ptrB + currentRunB.Start)
                    {
                        ptrA++;
                    }
#endif

#if NOLOOPS
                    var diffB = (int)((ptrA + currentRunA.Start) - (ptrB + currentRunB.Start));
                    if (diffB > 0)
                    {
                        ptrB += diffB;
                    }
#else
                    while (ptrB + currentRunB.Start < ptrA + currentRunA.Start)
                    {
                        ptrB++;
                    }
#endif


#if Manual4
                    // Now the Run pointers are aligned, start ANDing the elements until we reach the end of either Run
                    while (ptrA < currentRunA.Values.Length - 4 && ptrB < currentRunB.Values.Length - 4)
                    {
                        currentRunC.Values[ptrC] = currentRunA.Values[ptrA] & currentRunB.Values[ptrB];
                        currentRunC.Values[ptrC + 1] = currentRunA.Values[ptrA + 1] & currentRunB.Values[ptrB + 1];
                        currentRunC.Values[ptrC + 2] = currentRunA.Values[ptrA + 2] & currentRunB.Values[ptrB + 2];
                        currentRunC.Values[ptrC + 3] = currentRunA.Values[ptrA + 3] & currentRunB.Values[ptrB + 3];
                        ptrA += 4;
                        ptrB += 4;
                        ptrC += 4;
                    }

                    while (ptrA < currentRunA.Values.Length && ptrB < currentRunB.Values.Length)
                    {
                        currentRunC.Values[ptrC] = currentRunA.Values[ptrA] & currentRunB.Values[ptrB];
                        ptrA++;
                        ptrB++;
                        ptrC++;
                    }
#endif

                    //###################
                    // Use spans for performance (kind of like type-safe pointers)
                    //Span<ulong> spanA = currentRunA.Values.AsSpan(start: ptrA);
                    //Span<ulong> spanB = currentRunB.Values.AsSpan(start: ptrB);
                    //Span<ulong> spanC = currentRunC.Values.AsSpan(start: ptrC);
#if NETCOREAPP3_1 && Vector
                    var j = 0;

                    var simdLength = Vector<ulong>.Count;

                    // Work in sets of 4
                    var l = Math.Min(currentRunA.Values.Length - ptrA - simdLength, currentRunB.Values.Length - ptrB - simdLength);
                    if (l >= simdLength)
                    {
                        while (j < l)
                        {
                            Vector<ulong> vecA = new Vector<ulong>(currentRunA.Values, ptrA);
                            Vector<ulong> vecB = new Vector<ulong>(currentRunB.Values, ptrB);
                            var r = Vector.BitwiseAnd(vecA, vecB);
                            r.CopyTo(currentRunC.Values, ptrC);
                            ptrA += simdLength;
                            ptrB += simdLength;
                            ptrC += simdLength;
                            j += simdLength;
                        }
                    }

                    Span<ulong> spanA = currentRunA.Values.AsSpan(start: ptrA);
                    Span<ulong> spanB = currentRunB.Values.AsSpan(start: ptrB);
                    Span<ulong> spanC = currentRunC.Values.AsSpan(start: ptrC);

                    spanA = currentRunA.Values.AsSpan(start: ptrA);
                    spanB = currentRunB.Values.AsSpan(start: ptrB);
                    spanC = currentRunC.Values.AsSpan(start: ptrC);
                    j = 0;
                    l = Math.Min(currentRunA.Values.Length - ptrA, currentRunB.Values.Length - ptrB);
                    if (l > 0)
                    {
                        while (j < l)
                        {
                            spanC[j] = spanA[j] & spanB[j];
                            j++;
                        }

                        ptrA += l;
                        ptrB += l;
                        ptrC += l;
                    }

#endif



                    //###################
                    // Use spans for performance (kind of like type-safe pointers)
                    //Span<ulong> spanA = currentRunA.Values.AsSpan(start: ptrA);
                    //Span<ulong> spanB = currentRunB.Values.AsSpan(start: ptrB);
                    //Span<ulong> spanC = currentRunC.Values.AsSpan(start: ptrC);
                    //var j = 0;


                    //var l = Math.Min(currentRunA.Values.Length - ptrA - 4, currentRunB.Values.Length - ptrB - 4);
                    //if (l > 4)
                    //{
                    //    while (j < l)
                    //    {
                    //        // Hopefully the compiler will pipeline this and execute the instructions in parallel
                    //        spanC[j] = spanA[j] & spanB[j];
                    //        spanC[j + 1] = spanA[j + 1] & spanB[j + 1];
                    //        spanC[j + 2] = spanA[j + 2] & spanB[j + 2];
                    //        spanC[j + 3] = spanA[j + 3] & spanB[j + 3];
                    //        j += 4;
                    //    }
                    //    ptrA += l;
                    //    ptrB += l;
                    //    ptrC += l;
                    //}

                    // Process the items that didn't complete a batch of 4 
                    //spanA = currentRunA.Values.AsSpan(start: ptrA);
                    //spanB = currentRunB.Values.AsSpan(start: ptrB);
                    //spanC = currentRunC.Values.AsSpan(start: ptrC);
                    //j = 0;
                    //l = Math.Min(currentRunA.Values.Length - ptrA, currentRunB.Values.Length - ptrB);
                    //if (l > 0)
                    //{
                    //    while (j < l)
                    //    {
                    //        spanC[j] = spanA[j] & spanB[j];
                    //        j++;
                    //    }

                    //    ptrA += l;
                    //    ptrB += l;
                    //    ptrC += l;
                    //}
                    //###################

#if Manual2
                    var j = 0;
                    var l = Math.Min(currentRunA.Values.Length - ptrA, currentRunB.Values.Length - ptrB);
                    while (j < l)
                    {
                        currentRunC.Values[ptrC + j] = currentRunA.Values[ptrA + j] & currentRunB.Values[ptrB + j];
                        j++;
                    }

                    ptrA += l;
                    ptrB += l;
                    ptrC += l;
#endif                    

#if Manual1
                    while (ptrA < currentRunA.Values.Length && ptrB < currentRunB.Values.Length)
                    {
                        currentRunC.Values[ptrC] = currentRunA.Values[ptrA] & currentRunB.Values[ptrB];
                        ptrA++;
                        ptrB++;
                        ptrC++;
                    }
#endif                    

                    // Check if we reached the end of the A run
                    if (ptrA == currentRunA.Values.Length)
                    {
                        // advance to the next A run
                        currentA++;
                        if (currentA < a.Runs.Count)
                        {
                            currentRunA = a.Runs[currentA];
                        }
                        ptrA = 0;
                    }

                    // Check if we reached the end of the B run
                    if (ptrB == currentRunB.Values.Length)
                    {
                        // advance to the next B run
                        currentB++;
                        if (currentB < b.Runs.Count)
                        {
                            currentRunB = b.Runs[currentB];
                        }
                        ptrB = 0;
                    }
                }
                else if (currentRunA.Start > currentRunB.End)
                {
                    if (currentRunC != null)
                    {
                        result.Runs.Add(currentRunC);
                        currentRunC = null;
                        ptrC = 0;
                    }
                    //result.Runs.Add(currentRunB);
                    // catchup B
                    currentB++;
                    if (currentB < b.Runs.Count)
                    {
                        currentRunB = b.Runs[currentB];
                    }
                    ptrB = 0;
                }
                else if (currentRunB.Start > currentRunA.End)
                {
                    if (currentRunC != null)
                    {
                        result.Runs.Add(currentRunC);
                        currentRunC = null;
                        ptrC = 0;
                    }                        // catchup A
                    //result.Runs.Add(currentRunA);
                    currentA++;
                    if (currentA < a.Runs.Count)
                    {
                        currentRunA = a.Runs[currentA];
                    }
                    ptrA = 0;
                }
            }

            if (currentRunC != null)
            {
                result.Runs.Add(currentRunC);
            }

            return result;
        }

        /// <summary>
        /// Returns the logical Not of an optimized bitsets, using a full bitset as a reference 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="full"></param>
        /// <returns></returns>
        public static SparseBitset Not(SparseBitset a, SparseBitset full)
        {
            var result = new SparseBitset();

            var ptrA = 0;
            var ptrB = 0;
            var ptrC = 0;
            var currentA = 0;
            var currentB = 0;

            if (a.Runs.Count == 0)
            {
                return full;
            }
            if (full.Runs.Count == 0)
            {
                return result;
            }

            var currentRunA = a.Runs[currentA];
            var currentRunB = full.Runs[currentB];
            Run currentRunC = null;

            // Loop while we still have data in both bitsets
            while (currentA < a.Runs.Count && currentB < full.Runs.Count)
            {
                // Check if we're in the middle of a Run in A
                if (ptrA > 0)
                {
                    // Nothing to do, since the full bitset doesn't have data here, the output bitset should likewise be empty
                    // So, just flush the current output Run to the result bitset
                    if (currentRunC != null)
                    {
                        result.Runs.Add(currentRunC);
                        currentRunC = null;
                        ptrC = 0;
                    }
                }
                // Check if we're in the middle of a Run in the full bitset
                else if (ptrB > 0)
                {
                    // Since this is a NOT, anywhere there is no data (0) in the source bitset means it should be 
                    // 1 in the output bitset, or equivalent to the full bitset (since the full bitset may have 0s where there are no respondents)
                    while (ptrB + currentRunB.Start < Math.Min(ptrA + currentRunA.Start, currentRunB.End + 1))
                    {
                        currentRunC.Values[ptrC] = currentRunB.Values[ptrB];
                        ptrB++;
                        ptrC++;
                    }

                    if (ptrB + currentRunB.Start > currentRunB.End)
                    {
                        if (currentRunC != null)
                        {
                            result.Runs.Add(currentRunC);
                            currentRunC = null;
                            ptrC = 0;
                        }
                        // Advance to the next run
                        currentB++;
                        if (currentB < full.Runs.Count)
                        {
                            currentRunB = full.Runs[currentB];
                        }
                        else
                        {
                            continue;
                        }
                        ptrB = 0;
                    }
                }
                else if (ptrA == 0 && ptrB == 0)
                {
                    if (currentRunC != null)
                    {
                        result.Runs.Add(currentRunC);
                        currentRunC = null;
                        ptrC = 0;
                    }
                }

                /// Check if the runs overlap
                if (IsOverlapping(currentRunA, currentRunB))
                {
                    // If there is no current run, create one
                    if (currentRunC == null)
                    {
                        currentRunC = new Run();
                        currentRunC.Start = Math.Min(currentRunA.Start, currentRunB.Start);
                    }

                    // Calculate the end of the run 
                    currentRunC.End = Math.Max(currentRunC.End, Math.Max(currentRunA.End, currentRunB.End));

                    // Check if we need to initialize, or expand the Values array
                    if (currentRunC.Values == null || currentRunC.Values.Length < currentRunC.End - currentRunC.Start + 1)
                    {
                        var newArray = new uint[currentRunC.End - currentRunC.Start + 1];
                        if (currentRunC.Values != null)
                        {
                            // Copy the old values to the new array
                            Array.Copy(currentRunC.Values, newArray, currentRunC.Values.Length);
                        }
                        currentRunC.Values = newArray;
                    }

                    while (ptrA + currentRunA.Start < ptrB + currentRunB.Start)
                    {
                        ptrA++;
                    }

                    while (ptrB + currentRunB.Start < ptrA + currentRunA.Start)
                    {
                        currentRunC.Values[ptrC] = currentRunB.Values[ptrB];
                        ptrB++;
                        ptrC++;
                    }

                    while (ptrA < currentRunA.Values.Length && ptrB < currentRunB.Values.Length)
                    {
                        currentRunC.Values[ptrC] = ~currentRunA.Values[ptrA] & currentRunB.Values[ptrB];
                        ptrA++;
                        ptrB++;
                        ptrC++;
                    }

                    if (ptrA == currentRunA.Values.Length)
                    {
                        currentA++;
                        if (currentA < a.Runs.Count)
                        {
                            currentRunA = a.Runs[currentA];
                        }
                        ptrA = 0;
                    }

                    if (ptrB == currentRunB.Values.Length)
                    {
                        currentB++;
                        if (currentB < full.Runs.Count)
                        {
                            currentRunB = full.Runs[currentB];
                        }
                        ptrB = 0;
                    }

                }
                else if (currentRunA.Start > currentRunB.End)
                {
                    if (currentRunC != null)
                    {
                        result.Runs.Add(currentRunC);
                        currentRunC = null;
                        ptrC = 0;
                    }
                    // catchup B
                    result.Runs.Add(currentRunB);
                    currentB++;
                    if (currentB < full.Runs.Count)
                    {
                        currentRunB = full.Runs[currentB];
                    }
                    ptrB = 0;
                }
                else if (currentRunB.Start > currentRunA.End)
                {
                    if (currentRunC != null)
                    {
                        result.Runs.Add(currentRunC);
                        currentRunC = null;
                        ptrC = 0;
                    }
                    // catchup A
                    // Nothing to write to output, 
                    currentA++;
                    if (currentA < a.Runs.Count)
                    {
                        currentRunA = a.Runs[currentA];
                    }
                    ptrA = 0;
                }
            }

            if (ptrA > 0)
            {
                if (currentRunC != null)
                {
                    result.Runs.Add(currentRunC);
                    currentRunC = null;
                    ptrC = 0;
                }
            }

            if (ptrB > 0)
            {
                while (ptrB + currentRunB.Start < currentRunB.End + 1)
                {
                    currentRunC.Values[ptrC] = currentRunB.Values[ptrB];
                    ptrB++;
                    ptrC++;
                }

                // Check if we reached the end of our run
                if (ptrB + currentRunB.Start > currentRunB.End)
                {
                    if (currentRunC != null)
                    {
                        result.Runs.Add(currentRunC);
                        currentRunC = null;
                        ptrC = 0;
                    }
                    // Advance to the next run
                    currentB++;
                    if (currentB < full.Runs.Count)
                    {
                        currentRunB = full.Runs[currentB];
                    }
                    ptrB = 0;
                }
            }

            if (currentRunC != null)
            {
                result.Runs.Add(currentRunC);
                currentRunC = null;
                ptrC = 0;
            }

            //while (currentA < a.Runs.Count)
            //{
            //    currentA++;
            //}

            while (currentB < full.Runs.Count)
            {
                currentRunB = full.Runs[currentB];
                result.Runs.Add(currentRunB);
                currentB++;
            }

            return result;
        }

        /// <summary>
        /// Returns the logical Or of two optimized bitsets
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static SparseBitset Or(SparseBitset a, SparseBitset b)
        {
            var result = new SparseBitset();

            var ptrA = 0;
            var ptrB = 0;
            var ptrC = 0;

            var currentA = 0;
            var currentB = 0;

            if (a.Runs.Count == 0)
            {
                return b;
            }
            if (b.Runs.Count == 0)
            {
                return a;
            }

            var currentRunA = a.Runs[currentA];
            var currentRunB = b.Runs[currentB];
            Run currentRunC = null;

            // Only process the first and overlapping runs. Exit early when we run out of runs in either bitset.
            while (currentA < a.Runs.Count && currentB < b.Runs.Count)
            {
                // Check if we're in the middle of a run. This happens when one of the staggered overlapping runs end, leaving the longer run hanging
                if (ptrA > 0)
                {
                    // Since we are ORing, continue copying elements until we reach the end of this run, or the start of the opposite run
                    while (ptrA + currentRunA.Start < Math.Min(ptrB + currentRunB.Start, currentRunA.End + 1))
                    {
                        currentRunC.Values[ptrC] = currentRunA.Values[ptrA];
                        ptrA++;
                        ptrC++;
                    }

                    // Check if we reached the end of our run
                    if (ptrA + currentRunA.Start > currentRunA.End)
                    {
                        if (currentRunC != null)
                        {
                            result.Runs.Add(currentRunC);
                            currentRunC = null;
                            ptrC = 0;
                        }
                        // Advance to the next run
                        currentA++;
                        if (currentA < a.Runs.Count)
                        {
                            currentRunA = a.Runs[currentA];
                        }
                        else
                        {
                            continue;
                        }
                        ptrA = 0;
                    }
                }

                // Check if we're in the middle of a run. This happens when one of the staggered overlapping runs end, leaving the longer run hanging
                if (ptrB > 0)
                {
                    // Since we are ORing, continue copying elements until we reach the end of this run, or the start of the opposite run
                    while (ptrB + currentRunB.Start < Math.Min(ptrA + currentRunA.Start, currentRunB.End + 1))
                    {
                        currentRunC.Values[ptrC] = currentRunB.Values[ptrB];
                        ptrB++;
                        ptrC++;
                    }

                    // Check if we reached the end of our run
                    if (ptrB + currentRunB.Start > currentRunB.End)
                    {
                        if (currentRunC != null)
                        {
                            result.Runs.Add(currentRunC);
                            currentRunC = null;
                            ptrC = 0;
                        }
                        // Advance to the next run
                        currentB++;
                        if (currentB < b.Runs.Count)
                        {
                            currentRunB = b.Runs[currentB];
                        }
                        else
                        {
                            continue;
                        }
                        ptrB = 0;
                    }
                }

                if (ptrA == 0 && ptrB == 0)
                {
                    if (currentRunC != null)
                    {
                        result.Runs.Add(currentRunC);
                        currentRunC = null;
                        ptrC = 0;
                    }
                }

                // Check if these runs overlap
                if (IsOverlapping(currentRunA, currentRunB))
                {
                    // Check if we need to create a new output run
                    if (currentRunC == null)
                    {
                        currentRunC = new Run();
                        currentRunC.Start = Math.Min(currentRunA.Start, currentRunB.Start);
                    }

                    // Extend the run as needed
                    currentRunC.End = Math.Max(currentRunC.End, Math.Max(currentRunA.End, currentRunB.End));

                    // Copy the current run array to the new one as needed
                    if (currentRunC.Values == null || currentRunC.Values.Length < currentRunC.End - currentRunC.Start + 1)
                    {
                        var newArray = new uint[currentRunC.End - currentRunC.Start + 1];
                        if (currentRunC.Values != null)
                        {
                            Array.Copy(currentRunC.Values, newArray, currentRunC.Values.Length);
                        }
                        currentRunC.Values = newArray;
                    }

                    // Copy elements from A to the current output run until we reach the overlapping element in B
                    while (ptrA + currentRunA.Start < ptrB + currentRunB.Start)
                    {
                        currentRunC.Values[ptrC] = currentRunA.Values[ptrA];
                        ptrA++;
                        ptrC++;
                    }

                    // Copy elements from B to the current output run until we reach the overlapping element in A
                    while (ptrB + currentRunB.Start < ptrA + currentRunA.Start)
                    {
                        currentRunC.Values[ptrC] = currentRunB.Values[ptrB];
                        ptrB++;
                        ptrC++;
                    }

                    // Now OR the elements in A and B and write to the output run while they overlap
                    while (ptrA < currentRunA.Values.Length && ptrB < currentRunB.Values.Length)
                    {
                        currentRunC.Values[ptrC] = currentRunA.Values[ptrA] | currentRunB.Values[ptrB];
                        ptrA++;
                        ptrB++;
                        ptrC++;
                    }

                    // Check if we've reached the end of current run A
                    if (ptrA == currentRunA.Values.Length)
                    {
                        // Advance to the next run
                        currentA++;
                        if (currentA < a.Runs.Count)
                        {
                            currentRunA = a.Runs[currentA];
                        }
                        // And reset the pointer
                        ptrA = 0;
                    }

                    // Check if we've reached the end of current run B
                    if (ptrB == currentRunB.Values.Length)
                    {
                        // Advance to the next run
                        currentB++;
                        if (currentB < b.Runs.Count)
                        {
                            currentRunB = b.Runs[currentB];
                        }
                        // And reset the pointer
                        ptrB = 0;
                    }

                }
                else if (currentRunA.Start > currentRunB.End)
                {
                    if (currentRunC != null)
                    {
                        result.Runs.Add(currentRunC);
                        currentRunC = null;
                        ptrC = 0;
                    }
                    result.Runs.Add(currentRunB);
                    // catchup B
                    currentB++;
                    if (currentB < b.Runs.Count)
                    {
                        currentRunB = b.Runs[currentB];
                    }
                }
                else if (currentRunB.Start > currentRunA.End)
                {
                    if (currentRunC != null)
                    {
                        result.Runs.Add(currentRunC);
                        currentRunC = null;
                        ptrC = 0;
                    }
                    // catchup A
                    result.Runs.Add(currentRunA);
                    currentA++;
                    if (currentA < a.Runs.Count)
                    {
                        currentRunA = a.Runs[currentA];
                    }
                }
            }

            // Check if we're in the middle of a run. This happens when one of the staggered overlapping runs end, leaving the longer run hanging
            if (ptrA > 0)
            {
                // Since we are ORing, continue copying elements until we reach the end of this run, or the start of the opposite run
                while (ptrA + currentRunA.Start < currentRunA.End + 1)
                {
                    currentRunC.Values[ptrC] = currentRunA.Values[ptrA];
                    ptrA++;
                    ptrC++;
                }

                // Check if we reached the end of our run
                if (ptrA + currentRunA.Start > currentRunA.End)
                {
                    if (currentRunC != null)
                    {
                        result.Runs.Add(currentRunC);
                        currentRunC = null;
                        ptrC = 0;
                    }
                    // Advance to the next run
                    currentA++;
                    if (currentA < a.Runs.Count)
                    {
                        currentRunA = a.Runs[currentA];
                    }
                    ptrA = 0;
                }
            }


            // Check if we're in the middle of a run. This happens when one of the staggered overlapping runs end, leaving the longer run hanging
            if (ptrB > 0)
            {
                // Since we are ORing, continue copying elements until we reach the end of this run, or the start of the opposite run
                while (ptrB + currentRunB.Start < currentRunB.End + 1)
                {
                    currentRunC.Values[ptrC] = currentRunB.Values[ptrB];
                    ptrB++;
                    ptrC++;
                }

                // Check if we reached the end of our run
                if (ptrB + currentRunB.Start > currentRunB.End)
                {
                    if (currentRunC != null)
                    {
                        result.Runs.Add(currentRunC);
                        currentRunC = null;
                        ptrC = 0;
                    }
                    // Advance to the next run
                    currentB++;
                    if (currentB < b.Runs.Count)
                    {
                        currentRunB = b.Runs[currentB];
                    }
                    ptrB = 0;
                }
            }

            if (currentRunC != null)
            {
                result.Runs.Add(currentRunC);
                currentRunC = null;
                ptrC = 0;
            }


            while (currentA < a.Runs.Count)
            {
                currentRunA = a.Runs[currentA];
                result.Runs.Add(currentRunA);
                currentA++;
            }

            while (currentB < b.Runs.Count)
            {
                currentRunB = b.Runs[currentB];
                result.Runs.Add(currentRunB);
                currentB++;
            }

            return result;
        }
    }
}