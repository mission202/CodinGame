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
        [InlineData("0123456789|1123456789", 20)]
        [InlineData("0123456789|0123", 10)]
        [InlineData("0412578440|0412199803|0468892011|112|15", 28)]
        public void Examples(string numbers, int expected)
        {
            var split = numbers.Split('|');
            Assert.Equal(expected, Solution.Find(split));
        }
    }
}
