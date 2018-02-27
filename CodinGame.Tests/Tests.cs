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
        public void TestsWork()
        {
            Assert.True(true);
        }

        [Fact]
        public void ParseSampleData()
        {
            var sample = new SampleDataFile("0 -1 1 0 1 0 2 1 0 0");
            Assert.Equal(0, sample.Id);
            Assert.Equal(-1, sample.CarriedBy);
            Assert.True(sample.InCloud);
            Assert.Equal(1, sample.Rank);
            Assert.Equal("0", sample.Gain);
            Assert.Equal(1, sample.Health);
            Assert.Equal(0, sample.Cost.A);
            Assert.Equal(2, sample.Cost.B);
            Assert.Equal(1, sample.Cost.C);
            Assert.Equal(0, sample.Cost.D);
            Assert.Equal(0, sample.Cost.E);
        }

        [Fact]
        public void CanDeserialise()
        {
            var state = "START_POS 0 0 0 0 0 0 0 0 0 0 0 0|START_POS 0 0 0 0 0 0 0 0 0 0 0 0|5 5 5 5 5|0|/";
            var game = new Game(state);
            Assert.Equal(Modules.START_POS, game.Player.Target);
        }
    }
}
