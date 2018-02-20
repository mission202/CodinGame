using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace CodinGame.Tests
{
    public class Tests
    {
        [Theory]
        [InlineData(1, 11, "1 1 1 3 1 2 2 1 1 3 3 1 1 2 1 3 2 1 1 3 2 1 2 2 2 1")]
        [InlineData(2, 1, "2")]
        public void TestCases(int r, int l, string expected)
        {
            Assert.Equal(expected, ConwaySequence.Find(r, l));
        }
    }
}
