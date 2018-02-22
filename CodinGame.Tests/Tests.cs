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
        public void SimpleMoves()
        {
            var input = "5 5|#####|#@  #|#   #|#  $#|#####".Split('|');
            var expected = "SOUTH|SOUTH|EAST|EAST".Split('|');
            Assert.Equal(expected, Solution.Find(input));
        }

        [Fact]
        public void Obstacles()
        {
            var input = "8 8|########|# @    #|#     X#|# XXX  #|#   XX #|#   XX #|#     $#|########".Split('|');
            var expected = "SOUTH|EAST|EAST|EAST|SOUTH|EAST|SOUTH|SOUTH|SOUTH".Split('|');
            Assert.Equal(expected, Solution.Find(input));
        }
    }
}
