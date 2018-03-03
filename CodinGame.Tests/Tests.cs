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
        public void PicksHulk()
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
            Assert.Equal(Actions.Heroes.Hulk, action.IgnoreMessage());
        }

        [Fact]
        public void MoveToSupportTheRearLine()
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

            Assert.Equal(Actions.Move(205, 540), action.IgnoreMessage());
        }

        [Fact]
        public void MoveToAttackEnemyUnits()
        {
            // Closing in Range Now, Move to Attack Enemy Front Line
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
                3 0 HERO 855 540 95 1450 1450 0 80 200 0 300 0 0 0 90 90 1 HULK 1 0
                4 1 HERO 1517 547 95 1450 1450 0 80 200 0 300 0 0 0 90 90 1 HULK 1 0
                5 0 UNIT 910 490 90 365 400 0 25 150 0 30 0 0 0 0 0 0 - 1 0
                6 0 UNIT 910 540 90 400 400 0 25 150 0 30 0 0 0 0 0 0 - 1 0
                7 0 UNIT 910 590 90 400 400 0 25 150 0 30 0 0 0 0 0 0 - 1 0
                8 0 UNIT 785 490 300 250 250 0 35 150 0 50 0 0 0 0 0 0 - 1 0
                9 1 UNIT 1010 490 90 365 400 0 25 150 0 30 0 0 0 0 0 0 - 1 0
                10 1 UNIT 1010 540 90 400 400 0 25 150 0 30 0 0 0 0 0 0 - 1 0
                11 1 UNIT 1010 590 90 400 400 0 25 150 0 30 0 0 0 0 0 0 - 1 0
                12 1 UNIT 1135 490 300 250 250 0 35 150 0 50 0 0 0 0 0 0 - 1 0"
            .ToGame().Move();

            // Front Line = 160,540 + 95 Attack Range for Hulk
            Assert.Equal(Actions.MoveAttack(1010, 540, 10), action.IgnoreMessage());
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

        [Fact]
        public void DeterminesCorrectAttackDirection()
        {
            // Closing in Range Now, Move to Attack Enemy Front Line
            var action = @"
                1
                0
                0
                276
                276
                1
                12
                1 0 TOWER 100 540 400 1500 1500 0 100 0 0 0 0 0 0 0 0 0 - 1 0
                2 1 TOWER 1820 540 400 1500 1500 0 100 0 0 0 0 0 0 0 0 0 - 1 0
                3 0 HERO 200 590 270 820 820 0 60 200 0 300 0 0 0 200 200 2 IRONMAN 1 0
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

            Assert.Equal(Actions.Move(1905, 540), action.IgnoreMessage());
        }

        // TODO: Attack in Right Direction! (Check MyTeam to Determine X Conditionals)



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
