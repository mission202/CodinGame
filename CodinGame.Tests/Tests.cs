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
        public void Example()
        {
            var input = "10 10|##########|#        #|#  S   W #|#        #|#  $     #|#        #|#@       #|#        #|#E     N #|##########".Split('|');
            var expected = "SOUTH|SOUTH|EAST|EAST|EAST|EAST|EAST|EAST|NORTH|NORTH|NORTH|NORTH|NORTH|NORTH|WEST|WEST|WEST|WEST|SOUTH|SOUTH".Split('|');
            Assert.Equal(expected, Solution.Find(input));
        }

        [Fact]
        public void SmallExample()
        {
            var input = "5 6|######|#@E $#|# N  #|#X   #|######".Split('|');
            var expected = "SOUTH|EAST|NORTH|EAST|EAST".Split('|');
            Assert.Equal(expected, Solution.Find(input));
        }

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

        [Fact]
        public void BreakerMode()
        {
            var input = "10 10|##########|# @      #|# B      #|#XXX     #|# B      #|#    BXX$#|#XXXXXXXX#|#        #|#        #|##########".Split('|');
            var expected = "SOUTH|SOUTH|SOUTH|SOUTH|EAST|EAST|EAST|EAST|EAST|EAST".Split('|');
            Assert.Equal(expected, Solution.Find(input));
        }

        [Fact]
        public void InverterMode()
        {
            var input = "10 10|##########|#    I   #|#        #|#       $#|#       @#|#        #|#       I#|#        #|#        #|##########".Split('|');
            var expected = "SOUTH|SOUTH|SOUTH|SOUTH|WEST|WEST|WEST|WEST|WEST|WEST|WEST|NORTH|NORTH|NORTH|NORTH|NORTH|NORTH|NORTH|EAST|EAST|EAST|EAST|EAST|EAST|EAST|SOUTH|SOUTH".Split('|');
            Assert.Equal(expected, Solution.Find(input));
        }

        [Fact]
        public void Teleport()
        {
            var input = "10 10|##########|#    T   #|#        #|#        #|#        #|#@       #|#        #|#        #|#    T  $#|##########".Split('|');
            var expected = "SOUTH|SOUTH|SOUTH|EAST|EAST|EAST|EAST|EAST|EAST|EAST|SOUTH|SOUTH|SOUTH|SOUTH|SOUTH|SOUTH|SOUTH".Split('|');
            Assert.Equal(expected, Solution.Find(input));
        }

        [Fact]
        public void BrokenWall()
        {
            var input = "10 10|##########|#        #|#  @     #|#  B     #|#  S   W #|# XXX    #|#  B   N #|# XXXXXXX#|#       $#|##########".Split('|');
            var expected = "SOUTH|SOUTH|SOUTH|SOUTH|EAST|EAST|EAST|EAST|NORTH|NORTH|WEST|WEST|WEST|WEST|SOUTH|SOUTH|SOUTH|SOUTH|EAST|EAST|EAST|EAST|EAST".Split('|');
            Assert.Equal(expected, Solution.Find(input));
        }

        [Fact]
        public void Loop()
        {
            var input = "15 15|###############|#      IXXXXX #|#  @          #|#E S          #|#             #|#  I          #|#  B          #|#  B   S     W#|#  B   T      #|#             #|#         T   #|#         B   #|#N          W$#|#        XXXX #|###############".Split('|');
            Assert.Equal("LOOP", Solution.Find(input).Single());
        }
    }

    public class BenderTests
    {
        [Fact]
        public void MovesFirstPriority()
        {
            var grid = new Grid<char>(3, 3)
                .AddRow(' ', '$', ' ')
                .AddRow(' ', '@', ' ')
                .AddRow(' ', ' ', ' ');
            var bender = new Bender(grid);
            Assert.Equal(Directions.SOUTH, bender.Next);
        }

        [Fact]
        public void MovesSecondPriority()
        {
            var grid = new Grid<char>(3, 3)
                .AddRow(' ', '$', ' ')
                .AddRow(' ', '@', ' ')
                .AddRow(' ', 'X', ' ');
            var bender = new Bender(grid);
            Assert.Equal(Directions.EAST, bender.Next);
        }

        [Fact]
        public void MovesThirdPriority()
        {
            var grid = new Grid<char>(3, 3)
                .AddRow(' ', ' ', ' ')
                .AddRow('$', '@', 'X')
                .AddRow(' ', 'X', ' ');
            var bender = new Bender(grid);
            Assert.Equal(Directions.NORTH, bender.Next);
        }

        [Fact]
        public void MovesFourthPriority()
        {
            var grid = new Grid<char>(3, 3)
                .AddRow(' ', 'X', '$')
                .AddRow(' ', '@', 'X')
                .AddRow(' ', 'X', ' ');
            var bender = new Bender(grid);
            Assert.Equal(Directions.WEST, bender.Next);
        }
    }

    public class PriorityTests
    {
        [Fact]
        public void CanGetPrioritySequence()
        {
            Assert.Equal(
                new[] { Directions.EAST, Directions.SOUTH, Directions.EAST, Directions.NORTH, Directions.WEST },
                new Priorities().Next(Directions.EAST));
        }

        [Fact]
        public void DoesntDuplicateSouth()
        {
            Assert.Equal(
                new[] { Directions.SOUTH, Directions.EAST, Directions.NORTH, Directions.WEST },
                new Priorities().Next(Directions.SOUTH));
        }

        [Fact]
        public void CanReversePriorities()
        {
            var p = new Priorities();
            Console.Error.WriteLine($"{string.Join(", ", p.Next(Directions.SOUTH))}");
            Assert.Equal(Directions.EAST, p.Next(Directions.SOUTH).Skip(1).First()); // Default
            p.Reverse();
            Assert.Equal(Directions.WEST, p.Next(Directions.SOUTH).Skip(1).First());

        }
    }

    public class GridTests
    {
        [Fact]
        public void CreatesGrid()
        {
            var grid = new Grid<int>(2, 2)
                .AddRow(0, 1)
                .AddRow(2, 3);

            Assert.Equal(0, grid[0,0]);
            Assert.Equal(1, grid[1,0]);
            Assert.Equal(2, grid[0,1]);
            Assert.Equal(3, grid[1,1]);
        }

        [Fact]
        public void CanDraw()
        {
            var grid = new Grid<char>(2, 2)
                .AddRow('X', 'O')
                .AddRow('O', 'X');

            var expLines = "XO|OX".Split('|');
            var result = grid.Draw().Split(new [] { Environment.NewLine }, StringSplitOptions.None);

            for (int i = 0; i < expLines.Length; i++)
            {
                Assert.Equal(expLines[i], result[i]);
            }
        }

        [Fact]
        public void CanFindSingle()
        {
            var grid = new Grid<char>(2, 2)
                .AddRow('X', 'O')
                .AddRow('O', '@');

            Assert.Equal(new Coordinate(1, 1), grid.Single('@'));
        }

        [Fact]
        public void CanFindAll()
        {
            var grid = new Grid<char>(2, 2)
                .AddRow('X', 'T')
                .AddRow('T', '@');

            Assert.Contains(new Coordinate(1, 0), grid.All('T'));
            Assert.Contains(new Coordinate(0, 1), grid.All('T'));
        }
    }
}
