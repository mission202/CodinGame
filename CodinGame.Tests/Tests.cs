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
        [InlineData("0467123456", 10)]
        public void Examples(string numbers, int expected)
        {
            var split = numbers.Split('|');
            Assert.Equal(expected, Solution.Find(split));
        }
    }
}
