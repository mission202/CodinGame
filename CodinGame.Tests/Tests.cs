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
        public void PicksIronman()
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
            Assert.Equal(Actions.Heroes.Ironman.Name, action.IgnoreMessage());
        }

        [Fact]
        public void RetreatIfHealthLessThan10Percent()
        {
            // Closing in Range Now, Move to Attack Enemy Front Line
            var action = @"
                0
                0
                0
                578
                548
                1
                17
                1 0 TOWER 100 540 400 1500 1500 0 100 0 0 0 0 0 0 0 0 0 - 1 0
                2 1 TOWER 1820 540 400 1500 1500 0 100 0 0 0 0 0 0 0 0 0 - 1 0
                3 0 HERO 1301 526 95 70 1450 0 80 200 0 300 0 0 0 90 90 1 HULK 1 0
                4 1 HERO 1512 529 95 835 1450 0 80 200 0 300 0 0 0 90 90 1 HULK 1 0
                8 0 UNIT 1210 491 300 250 250 0 35 150 0 50 0 0 0 0 0 0 - 1 0
                13 0 UNIT 1417 526 90 160 400 0 25 150 0 30 0 0 0 0 0 0 - 1 0
                14 0 UNIT 1417 526 90 400 400 0 25 150 0 30 0 0 0 0 0 0 - 1 0
                15 0 UNIT 1417 526 90 400 400 0 25 150 0 30 0 0 0 0 0 0 - 1 0
                16 0 UNIT 1206 543 300 250 250 0 35 150 0 50 0 0 0 0 0 0 - 1 0
                21 0 UNIT 310 490 90 400 400 0 25 150 0 30 0 0 0 0 0 0 - 1 0
                22 0 UNIT 310 540 90 400 400 0 25 150 0 30 0 0 0 0 0 0 - 1 0
                23 0 UNIT 310 590 90 400 400 0 25 150 0 30 0 0 0 0 0 0 - 1 0
                24 0 UNIT 260 590 300 250 250 0 35 150 0 50 0 0 0 0 0 0 - 1 0
                25 1 UNIT 1611 510 90 400 400 0 25 150 0 30 0 0 0 0 0 0 - 1 0
                26 1 UNIT 1610 531 90 400 400 0 25 150 0 30 0 0 0 0 0 0 - 1 0
                27 1 UNIT 1615 553 90 400 400 0 25 150 0 30 0 0 0 0 0 0 - 1 0
                28 1 UNIT 1662 570 300 250 250 0 35 150 0 50 0 0 0 0 0 0 - 1 0"
            .ToGame().Move();

            // Front Line = 160,540 + 95 Attack Range for Hulk
            Assert.Equal(Actions.Move(100, 540), action.IgnoreMessage());
        }

        // TODO: Tests More Generic vs. Specific Co-Ords etc?

        // TODO: Buy Available Items


        [Fact(Skip = "Fixed")]
        public void DontFuckUp()
        {
            // Closing in Range Now, Move to Attack Enemy Front Line
            var action = @"
                0
                0
                0
                488
                488
                1
                11
                1 0 TOWER 100 540 400 1500 1500 0 100 0 0 0 0 0 0 0 0 0 - 1 0
                2 1 TOWER 1820 540 400 1500 1500 0 100 0 0 0 0 0 0 0 0 0 - 1 0
                3 0 HERO 1005 540 95 1120 1450 0 80 200 0 300 0 0 0 90 90 1 HULK 1 0
                4 1 HERO 1010 541 95 1450 1450 0 80 200 0 300 0 0 0 90 90 1 HULK 1 0
                5 0 UNIT 915 490 90 305 400 0 25 150 0 30 0 0 0 0 0 0 - 1 0
                6 0 UNIT 915 540 90 215 400 0 25 150 0 30 0 0 0 0 0 0 - 1 0
                7 0 UNIT 915 590 90 375 400 0 25 150 0 30 0 0 0 0 0 0 - 1 0
                8 0 UNIT 785 490 300 250 250 0 35 150 0 50 0 0 0 0 0 0 - 1 0
                9 1 UNIT 1005 490 90 125 400 0 25 150 0 30 0 0 0 0 0 0 - 1 0
                11 1 UNIT 1005 590 90 300 400 0 25 150 0 30 0 0 0 0 0 0 - 1 0
                12 1 UNIT 1135 490 300 250 250 0 35 150 0 50 0 0 0 0 0 0 - 1 0"
            .ToGame().Move();

            // Front Line = 160,540 + 95 Attack Range for Hulk
            Assert.Equal(Actions.MoveAttack(1010, 540, 10), action.IgnoreMessage());
        }

        // TODO: Get Valkyrie Moving to Front Line!

        // TODO: Move to Furthest Forward Unit

        // TODO: If Within 300 of Furthest Forward, Attack Nearest
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
                .Split(new[] { Environment.NewLine }, StringSplitOptions.None)
                .Select(s => s.Trim())
                .ToList());
            _inputSource = _input.Dequeue;
        }
    }
}
