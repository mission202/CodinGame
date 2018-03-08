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
            Assert.Contains("HULK", actions.IgnoreMessage());
        }

        // TODO: Tests More Generic vs. Specific Co-Ords etc?

        // TODO: Add Skill Support - Thankfully Last Rule Change!

        // Multiple Heroes - Going to BORK Tests!
        /* == Hero AI ==
         * Golddigga: Attack to get the most gold available:
         * 300 Kill Hero
         * 150 Denied Hero
         * 100 Groot
         *  50 Ranged Unit
         *  30 Melee Unit
         *
         *  Good strategy might be to use HULK and BASH Groots until we've enough cash?
         *
         *  TODO Consider Invisiblity/Ghost Strategy
         *  - The turn after a visible hero enters a bush they become invisible if no enemy hero shares the same hiding spot.
         *  - Invisible heroes are not targetable by attacks nor by single target skills. Their information will be available in the input on the next round, but not the ones following after, if they stay invisible.
         *  - Invisibility is lost when the hero attacks an enemy unit, hero or tower or when an enemy hero enters the same bush area. Invisibility is not lost if the hero attacks a neutral unit.
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
