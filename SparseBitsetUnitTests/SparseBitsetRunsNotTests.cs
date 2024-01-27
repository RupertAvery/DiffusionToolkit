using NUnit.Framework;

namespace SparseBitsetUnitTests
{
    [TestFixture]
    public class SparseBitsetRunsNotTests
    {
        [TestCase()]
        public void NotOfEmptyIsOverall()
        {
            var ____left = BitsetHelpers.ToRuns(0, "----------------------------");
            var ____full = BitsetHelpers.ToRuns(0, "****************************");
            var __result = BitsetHelpers.ToRuns(0, "****************************");


            var leftBitset = ____left.ToOptimizedBitset();
            var fullBitset = ____full.ToOptimizedBitset();
            var actual = leftBitset.Not(fullBitset).GetValues();
            var expected = __result.ToOptimizedBitset().GetValues();

            CollectionAssert.AreEqual(actual, expected);
        }

        [TestCase()]
        public void NotOfEmptyIsOverallWithGap()
        {
            var ____left = BitsetHelpers.ToRuns(0, "----------------------------");
            var ____full = BitsetHelpers.ToRuns(0, "**************--************");
            var __result = BitsetHelpers.ToRuns(0, "**************--************");


            var leftBitset = ____left.ToOptimizedBitset();
            var fullBitset = ____full.ToOptimizedBitset();
            var actual = leftBitset.Not(fullBitset).GetValues();
            var expected = __result.ToOptimizedBitset().GetValues();

            CollectionAssert.AreEqual(actual, expected);
        }

        [TestCase()]
        public void NotOfOverallWithGapIsEmpty()
        {
            var ____left = BitsetHelpers.ToRuns(0, "****************************");
            var ____full = BitsetHelpers.ToRuns(0, "**************--************");
            var __result = BitsetHelpers.ToRuns(0, "----------------------------");


            var leftBitset = ____left.ToOptimizedBitset();
            var fullBitset = ____full.ToOptimizedBitset();
            var actual = leftBitset.Not(fullBitset).GetValues();
            var expected = __result.ToOptimizedBitset().GetValues();

            CollectionAssert.AreEqual(actual, expected);
        }

        [TestCase()]
        public void Not()
        {
            var ____left = BitsetHelpers.ToRuns(0, "------------****------------");
            var ____full = BitsetHelpers.ToRuns(0, "****************************");
            var __result = BitsetHelpers.ToRuns(0, "************----************");


            var leftBitset = ____left.ToOptimizedBitset();
            var fullBitset = ____full.ToOptimizedBitset();
            var actual = leftBitset.Not(fullBitset).GetValues();
            var expected = __result.ToOptimizedBitset().GetValues();

            CollectionAssert.AreEqual(actual, expected);
        }


        [TestCase()]
        public void NotWithOverallWithGap()
        {
            var ____left = BitsetHelpers.ToRuns(0, "------------****------------");
            var ____full = BitsetHelpers.ToRuns(0, "********-*******************");
            var __result = BitsetHelpers.ToRuns(0, "********-***----************");


            var leftBitset = ____left.ToOptimizedBitset();
            var fullBitset = ____full.ToOptimizedBitset();
            var actual = leftBitset.Not(fullBitset).GetValues();
            var expected = __result.ToOptimizedBitset().GetValues();

            CollectionAssert.AreEqual(actual, expected);
        }
        
        [TestCase()]
        public void NotWithGapRuns()
        {
            var ____left = BitsetHelpers.ToRuns(0, "------------****------------");
            var ____full = BitsetHelpers.ToRuns(0, "********-*******************");
            var __result = BitsetHelpers.ToRuns(0, "********-***----************");

            var leftBitset = ____left.ToOptimizedBitset();
            var fullBitset = ____full.ToOptimizedBitset();
            var actual = leftBitset.Not(fullBitset).GetValues();
            var expected = __result.ToOptimizedBitset().GetValues();

            CollectionAssert.AreEqual(actual, expected);
        }
    }
}