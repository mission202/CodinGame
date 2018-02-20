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
        [InlineData(4, "0 0|1 1|2 2")]
        [InlineData(4, "1 2|0 0|2 2")]
        [InlineData(5, "1 2|0 0|2 2|1 3")]
        [InlineData(0, "1 1")]
        [InlineData(18, "-5 -3|-9 2|3 -4")]
        public void Examples(int expected, string input)
        {
            Assert.Equal(expected, Solution.Find(input.Split('|')));
        }
    }
}
