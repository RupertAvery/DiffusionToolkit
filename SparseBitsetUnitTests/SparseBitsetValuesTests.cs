using NUnit.Framework;
using SparseBitsets;
using System.Collections.Generic;
using System.Linq;

namespace SparseBitsetUnitTests
{
    [TestFixture]
    public class SparseBitsetValuesTests
    {
        private SparseBitset _overallBitset;

        [SetUp]
        public void Setup()
        {
            var bitValues = Enumerable.Range(0, 1000);
            _overallBitset = new SparseBitset();

            foreach (var bitValue in bitValues)
            {
                _overallBitset.Add((uint)bitValue);
            }

            _overallBitset.Pack();
        }

        [Test]
        public void OverallBitsetGetPopCount_ShouldBe1000_True()
        {
            Assert.AreEqual(1000, _overallBitset.GetPopCount());
        }

        [TestCase(ExpectedResult = 128)]
        public long Crossing64BitBoundaries()
        {
            return Enumerable.Range(1152 * 64 + 5, 128).Select(x => (uint)x).ToOptimizedBitset().GetPopCount();
        }



        [TestCase(new uint[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, ExpectedResult = 10)]
        [TestCase(new uint[] { 1, 2, 3, 4, 5 }, ExpectedResult = 5)]
        [TestCase(new uint[] { 1, 2 }, ExpectedResult = 2)]
        public long OverallAndGetPopCount_ShouldBe_True(IEnumerable<uint> valuesToCompare)
        {
            var breakdown = _overallBitset.And(valuesToCompare.ToOptimizedBitset());
            return breakdown.GetPopCount();
        }


        [TestCase(new uint[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, ExpectedResult = 990)]
        [TestCase(new uint[] { 1, 2, 3, 4, 5 }, ExpectedResult = 995)]
        [TestCase(new uint[] { 1, 2 }, ExpectedResult = 998)]
        public long OverallAndNotGetPopCount_ShouldBe_True(IEnumerable<uint> valuesToCompare)
        {
            var breakdown = _overallBitset.AndNot(valuesToCompare.ToOptimizedBitset(), _overallBitset);
            return breakdown.GetPopCount();
        }


        [TestCase(new uint[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, ExpectedResult = 990)]
        [TestCase(new uint[] { 1, 2, 3, 4, 5 }, ExpectedResult = 995)]
        [TestCase(new uint[] { 1, 2 }, ExpectedResult = 998)]
        public long OverallNotGetPopCount_ShouldBe_True(IEnumerable<uint> valuesToCompare)
        {
            var breakdown = valuesToCompare.ToOptimizedBitset().Not(_overallBitset);
            return breakdown.GetPopCount();
        }

        [TestCase(new uint[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, ExpectedResult = 1000)]
        [TestCase(new uint[] { 1, 2, 3, 4, 5 }, ExpectedResult = 1000)]
        [TestCase(new uint[] { 1, 2 }, ExpectedResult = 1000)]
        public long OverallOrGetPopCount_ShouldBe_True(IEnumerable<uint> valuesToCompare)
        {
            var breakdown = _overallBitset.Or(valuesToCompare.ToOptimizedBitset());
            return breakdown.GetPopCount();
        }

        [TestCase(new uint[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, new uint[] { 1, 2, 3, 4, 5, 6, 7 }, ExpectedResult = new uint[] { 1, 2, 3, 4, 5, 6, 7 })]
        [TestCase(new uint[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, new uint[] { 5, 6, 7, 8, 9, 10 }, ExpectedResult = new uint[] { 5, 6, 7, 8, 9, 10 })]
        [TestCase(new uint[] { 5, 6, 7, 8, 9, 10, 11, 12 }, new uint[] { 1, 2, 3, 4, 5, 6, 7, 8 }, ExpectedResult = new uint[] { 5, 6, 7, 8 })]
        [TestCase(new uint[] { 1, 2, 3, 4, 5, 6, 7, 8 }, new uint[] { 5, 6, 7, 8, 9, 10, 11, 12 }, ExpectedResult = new uint[] { 5, 6, 7, 8 })]
        [TestCase(new uint[] { 5, 6, 7, 8, 9, 10 }, new uint[] { 11, 12, 13, 14, 15, 16, 17, 18, 19, 20 }, ExpectedResult = new uint[] { })]
        [TestCase(new uint[] { 1, 2, 3, 4, 5, 6, 7, 8 }, new uint[] { 1, 2, 3, 4, 5, 6, 7, 8 }, ExpectedResult = new uint[] { 1, 2, 3, 4, 5, 6, 7, 8 })]
        [TestCase(new uint[] { 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, }, new uint[] { 1, 2, 3, 4, 5, 6, 7, 8, 15, 16, 17, 18 }, ExpectedResult = new uint[] { 5, 6, 7, 8, 15, 16, 17, 18 })]
        [TestCase(new uint[] { 60, 61, 62, 63, 64, 65, 66, 67, 68, 69, 70, 71 }, new uint[] { 54, 55, 56, 57, 58, 59, 60, 61, 62, 63, 64, 65, 66, 67 }, ExpectedResult = new uint[] { 60, 61, 62, 63, 64, 65, 66, 67 })]
        [TestCase(new uint[] { }, new uint[] { 1, 2, 3, 4, 5, 6, 7, 8 }, ExpectedResult = new uint[] { })]
        [TestCase(new uint[] { 1, 2, 3, 4, 5, 6, 7, 8 }, new uint[] { }, ExpectedResult = new uint[] { })]
        public long[] AndGetPopCount_ShouldBe_True(IEnumerable<uint> leftPopulation, IEnumerable<uint> rightPopulation)
        {
            var leftBitset = leftPopulation.ToOptimizedBitset();
            var rightBitset = rightPopulation.ToOptimizedBitset();

            return leftBitset.And(rightBitset).GetValues().ToArray();
        }

        [TestCase(new uint[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, new uint[] { 1, 2, 3, 4, 5 }, ExpectedResult = new uint[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 })]
        [TestCase(new uint[] { 4, 5, 6, 7, 8 }, new uint[] { 6, 7, 8, 9, 10 }, ExpectedResult = new uint[] { 4, 5, 6, 7, 8, 9, 10 }, Description = "Overlap with lower Left")]
        [TestCase(new uint[] { 6, 7, 8, 9, 10 }, new uint[] { 4, 5, 6, 7, 8 }, ExpectedResult = new uint[] { 4, 5, 6, 7, 8, 9, 10 }, Description = "Overlap with lower Right")]
        [TestCase(new uint[] { 6, 7, 8, 9, 10 }, new uint[] { 1, 2, 3, 4, 5 }, ExpectedResult = new uint[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, Description = "No Overlap")]
        [TestCase(new uint[] { 1, 2, 3, 4, 5 }, new uint[] { 1, 2, 3, 4, 5 }, ExpectedResult = new uint[] { 1, 2, 3, 4, 5 }, Description = "Full overlap")]
        public long[] OrGetPopCount_ShouldBe_True(IEnumerable<uint> leftPopulation, IEnumerable<uint> rightPopulation)
        {
            var leftBitset = leftPopulation.ToOptimizedBitset();
            var rightBitset = rightPopulation.ToOptimizedBitset();

            return leftBitset.Or(rightBitset).GetValues().ToArray();
        }

        [TestCase(new uint[] { 6, 7, 8, 9, 10, 11 }, ExpectedResult = new uint[] { 6, 7, 8, 9, 10, 11 })]
        [TestCase(new uint[] { 62, 63, 64, 65, 66, 67, 68, 69, 70, 71 }, ExpectedResult = new uint[] { 62, 63, 64, 65, 66, 67, 68, 69, 70, 71 }, Description = "Crosses 64-bit boundary")]
        public long[] GetValues_ShouldBe_True(IEnumerable<uint> leftPopulation)
        {
            var leftBitset = leftPopulation.ToOptimizedBitset();
            return leftBitset.GetValues().ToArray();
        }

        [TestCase(new uint[] { 6, 7, 8, 9, 10, 11 }, new uint[] { 6, 7, 8, 9, 10, 11 }, ExpectedResult = new uint[] { })]
        [TestCase(new uint[] { 6, 7, 8, 9, 10 }, new uint[] { 6, 7, 8, 9, 10, 11 }, ExpectedResult = new uint[] { 11 })]
        [TestCase(new uint[] { 1 }, new uint[] { 1, 2, 3, 4, 6, 7, 8, 9, 10 }, ExpectedResult = new uint[] { 2, 3, 4, 6, 7, 8, 9, 10 })]
        [TestCase(new uint[] { }, new uint[] { 1, 2, 3, 4, 6, 7, 8, 9, 10 }, ExpectedResult = new uint[] { 1, 2, 3, 4, 6, 7, 8, 9, 10 })]
        [TestCase(new uint[] { 1, 2, 3, 4, 6, 7, 8, 9, 10 }, new uint[] { 1, 2, 3, 4, 6, 7, 8, 9, 10 }, ExpectedResult = new uint[] { })]
        public long[] Not_ShouldBe_True(IEnumerable<uint> leftPopulation, IEnumerable<uint> fullPopulation)
        {
            var leftBitset = leftPopulation.ToOptimizedBitset();
            var fullBitset = fullPopulation.ToOptimizedBitset();

            return leftBitset.Not(fullBitset).GetValues().ToArray();
        }

        [TestCase(new uint[] { 6, 7, 8, 9, 10, 11 }, new uint[] { 1, 2, 3, 4, 5, 11 }, ExpectedResult = 5)]
        [TestCase(new uint[] { 6, 7, 8, 9, 10 }, new uint[] { 6, 7, 8, 9, 10 }, ExpectedResult = 0)]
        public long AndNotGetPopCount_ShouldBe_True(IEnumerable<uint> leftPopulation, IEnumerable<uint> rightPopulation)
        {
            var leftBitset = leftPopulation.ToOptimizedBitset();
            var rightBitset = rightPopulation.ToOptimizedBitset();
            var fullBitset = leftBitset.Or(rightBitset);

            return leftBitset.AndNot(rightBitset, fullBitset).GetPopCount();
        }

        [TestCase(new uint[] { 1, 2, 3, 4, 5 }, new uint[] { 2, 5 }, new uint[] { 6, 7, 8 }, ExpectedResult = 5)]
        [TestCase(new uint[] { 1, 2, 3, 4, 5 }, new uint[] { 10, 11, 12 }, new uint[] { 6, 7, 8 }, ExpectedResult = 3)]
        public long AndOrGetPopCount_ShouldBe_True(IEnumerable<uint> andPopulation1, IEnumerable<uint> andPopulation2, IEnumerable<uint> orPopulation)
        {
            var bitSet = andPopulation1.ToOptimizedBitset()
                .And(andPopulation2.ToOptimizedBitset())
                .Or(orPopulation.ToOptimizedBitset());

            return bitSet.GetPopCount();
        }

        [TestCase(new uint[] { 1, 2, 3, 4, 5 }, new uint[] { 2, 5 }, new uint[] { 6, 7, 8 }, new uint[] { 2, 5 }, ExpectedResult = 3)]
        public long MixedOperationGetPopCount_ShouldBe_True(IEnumerable<uint> andPopulation1, IEnumerable<uint> andPopulation2, IEnumerable<uint> orPopulation, IEnumerable<uint> notPopulation)
        {
            var fullBitset = andPopulation1.ToOptimizedBitset().Or(andPopulation2.ToOptimizedBitset()).Or(orPopulation.ToOptimizedBitset())
                .Or(notPopulation.ToOptimizedBitset());

            var bitSet = andPopulation1.ToOptimizedBitset()
                .And(andPopulation2.ToOptimizedBitset())
                .Or(orPopulation.ToOptimizedBitset())
                .AndNot(notPopulation.ToOptimizedBitset(), fullBitset);

            return bitSet.GetPopCount();
        }

        [Test]
        public void RemoveMembers_ShouldBe_True()
        {
            var bitValues = Enumerable.Range(1, 250);
            var bitset = new SparseBitset();

            foreach (var bitValue in bitValues)
            {
                bitset.Add((uint)bitValue);
            }
            bitset.Pack();

            bitset.Unpack();
            bitset.Remove(260);
            bitset.Remove(100);
            bitset.Pack();

            Assert.AreEqual(248, bitset.GetPopCount());
        }

        [Test]
        public void Pack_Unpack_Runs_ShouldBe_True()
        {
            var bitset = new SparseBitset();

            bitset.Add(25);
            bitset.Add(64);
            bitset.Add(192);
            bitset.Pack();

            bitset.Unpack();

            Assert.AreEqual(3, bitset.GetPopCount());
        }
    }
}