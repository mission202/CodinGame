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
        public void PicksHulkFirst()
        {
            var actions = @"
                0
                0
                0
                488
                488
                -1
                2
                1 0 TOWER 100 540 400 1500 1500 0 100 0 0 0 0 0 0 0 0 0 - 1 0
                2 1 TOWER 1820 540 400 1500 1500 0 100 0 0 0 0 0 0 0 0 0 - 1 0"
            .ToGame().Moves();
            Assert.Contains(Heroes.Hulk.Name, actions.IgnoreMessage());
        }

        // TODO: Tests More Generic vs. Specific Co-Ords etc?

        // TODO: Buy Available Items

        // Multiple Heroes - Going to BORK Tests!
        /* == Hero AI ==
         * Golddigga: Attack to get the most gold available:
         * 300 Kill Hero
         * 150 Denied Hero
         * 100 Groot
         *  50 Ranged Unit
         *  30 Melee Unit
         *
         */

    }

    internal static class Extensions
    {
        internal static string[] IgnoreMessage(this string[] values)
        {
            return values.Select(s => s.Split(';')[0]).ToArray();
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
                .Split(new[] { Environment.NewLine }, StringSplitOptions.None)
                .Select(s => s.Trim())
                .ToList());
            _inputSource = _input.Dequeue;
        }
    }
}
