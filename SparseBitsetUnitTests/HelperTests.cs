using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace SparseBitsetUnitTests
{
    [TestFixture]
    public class HelperTests
    {

        [TestCase()]
        public void Value()
        {
            var value = BitsetHelpers.ToValue("-------- -------- -------- --------");
            Assert.AreEqual(0, value);
        }


        [TestCase()]
        public void MaxValue()
        {
            var value = BitsetHelpers.ToValue("******** ******** ******** ********");
            Assert.AreEqual(uint.MaxValue, value);
        }



        [TestCase()]
        public void ArbitraryValue()
        {
            var value = BitsetHelpers.ToValue("-------- -------- *****--* --------");
            var expected = 0b00000000_00000000_11111001_00000000;
            Assert.AreEqual(expected, value);
        }

        [TestCase()]
        public void SingleRun()
        {
            var runs = BitsetHelpers.ToRuns(0, "------------****------------", new Dictionary<char, uint>()).ToList();

            Assert.AreEqual(1, runs.Count);
            var run = runs[0];
            Assert.AreEqual(12, run.Start);
            Assert.AreEqual(15, run.End);
            CollectionAssert.AreEqual(new uint[] { uint.MaxValue, uint.MaxValue, uint.MaxValue, uint.MaxValue }, run.Values);
        }


        [TestCase()]
        public void MultipleRuns()
        {
            var runs = BitsetHelpers.ToRuns(0, "--***-----*****-****", new Dictionary<char, uint>()).ToList();

            Assert.AreEqual(3, runs.Count);
            var run = runs[0];
            Assert.AreEqual(2, run.Start);
            Assert.AreEqual(4, run.End);
            CollectionAssert.AreEqual(new uint[] { uint.MaxValue, uint.MaxValue, uint.MaxValue }, run.Values);
            run = runs[1];
            Assert.AreEqual(10, run.Start);
            Assert.AreEqual(14, run.End);
            CollectionAssert.AreEqual(new uint[] { uint.MaxValue, uint.MaxValue, uint.MaxValue, uint.MaxValue, uint.MaxValue }, run.Values);
            run = runs[2];
            Assert.AreEqual(16, run.Start);
            Assert.AreEqual(19, run.End);
            CollectionAssert.AreEqual(new uint[] { uint.MaxValue, uint.MaxValue, uint.MaxValue, uint.MaxValue }, run.Values);
        }


    }
}