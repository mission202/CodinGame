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
        [Fact]
        public void Diagram()
        {
            Assert.Equal(2, Solution.Find("1 2|2 3|3 4|3 7|4 5|4 6|7 8".Split('|')));
        }

        [Fact]
        public void Test1()
        {
            Assert.Equal(2, Solution.Find("0 1|1 2|2 3|2 4".Split('|')));
        }

        [Fact]
        public void Test2()
        {
            Assert.Equal(2, Solution.Find("0 1|1 2|1 4|2 3|4 5|4 6".Split('|')));
        }

        [Fact]
        public void Test3()
        {
            Assert.Equal(3, Solution.Find("0 1|0 8|0 15|1 2|1 5|2 3|2 4|5 6|5 7|8 9|8 12|9 10|9 11|12 13|12 14|15 16|15 19|16 17|16 18|19 20|19 21".Split('|')));
        }
    }
}
