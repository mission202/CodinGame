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
        public void PicksRandomHeroInPickPhase()
        {
            var action = @"
                0
                0
                0
                488
                488
                -1
                2
                1 0 TOWER 100 540 400 1500 1500 0 100 0 0 0 0 0 0 0 0 0 - 1 0
                2 1 TOWER 1820 540 400 1500 1500 0 100 0 0 0 0 0 0 0 0 0 - 1 0"
            .ToGame().Move();
            Assert.Equal(Actions.Heroes.Valkyrie, action.IgnoreMessage());
        }

        [Fact]
        public void GetMovingToTower()
        {
            var action = @"
                0
                0
                0
                488
                488
                1
                12
                1 0 TOWER 100 540 400 1500 1500 0 100 0 0 0 0 0 0 0 0 0 - 1 0
                2 1 TOWER 1820 540 400 1500 1500 0 100 0 0 0 0 0 0 0 0 0 - 1 0
                3 0 HERO 200 590 130 1400 1400 0 65 200 0 300 0 0 0 155 155 2 VALKYRIE 1 0
                4 1 HERO 1720 590 95 1450 1450 0 80 200 0 300 0 0 0 90 90 1 HULK 1 0
                5 0 UNIT 160 490 90 400 400 0 25 150 0 30 0 0 0 0 0 0 - 1 0
                6 0 UNIT 160 540 90 400 400 0 25 150 0 30 0 0 0 0 0 0 - 1 0
                7 0 UNIT 160 590 90 400 400 0 25 150 0 30 0 0 0 0 0 0 - 1 0
                8 0 UNIT 110 490 300 250 250 0 35 150 0 50 0 0 0 0 0 0 - 1 0
                9 1 UNIT 1760 490 90 400 400 0 25 150 0 30 0 0 0 0 0 0 - 1 0
                10 1 UNIT 1760 540 90 400 400 0 25 150 0 30 0 0 0 0 0 0 - 1 0
                11 1 UNIT 1760 590 90 400 400 0 25 150 0 30 0 0 0 0 0 0 - 1 0
                12 1 UNIT 1810 490 300 250 250 0 35 150 0 50 0 0 0 0 0 0 - 1 0"
            .ToGame().Move();
            Assert.Equal(Actions.Move(1820, 540), action.IgnoreMessage());
        }

        // TODO: Get Valkyrie Moving to Front Line!
    }

    internal static class Extensions
    {
        internal static string IgnoreMessage(this string value)
        {
            return value.Split(';')[0];
        }

        internal static Game ToGame(this string state)
        {
            return new Game(new StringState(state));
        }
    }

    internal class StringState : GameState
    {
        private Queue<string> _input;

        public StringState(string state)
        {
            _input = new Queue<string>(
                state.Trim()
                .Split(new [] {Environment.NewLine }, StringSplitOptions.None)
                .Select(s => s.Trim())
                .ToList());
            _inputSource = _input.Dequeue;
        }
    }
}
