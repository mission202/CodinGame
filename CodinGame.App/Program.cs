using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

/*
 * I AM ONLY SUBMITTING THIS TO CHECK SCORE, IT STILL NEEDS REFACTORING.
 * I AM NOT RESPONSIBLE FOR BLEEDING EYES/LOSS OF WILL TO LIVE UPON READING.
 *
 * FOLLOW UP: HOLY CRAP I GOT 100%!!!
 */

public static class Solution
{
    static void Main(string[] args)
    {
        var input = new List<string>();
        Func<string> readLine = () =>
        {
            var s = Console.ReadLine();
            input.Add(s);
            return s;
        };

        string[] dimensions = readLine().Split(' ');
        for (int i = 0; i < int.Parse(dimensions[0]); i++)
            readLine();

        Console.WriteLine(string.Join(Environment.NewLine, Find(input)));
    }

    public static IEnumerable<string> Find(IEnumerable<string> input) => new Bender(input.ToGrid()).GetPath();
}

public class Bender
{
    private readonly Coordinate _goal;
    private readonly Grid<char> _map;
    private readonly Priorities _priorities = new Priorities();
    private readonly Dictionary<Coordinate, Coordinate> _teleporters = new Dictionary<Coordinate, Coordinate>();

    private Coordinate _position;
    private string _currentDirection;
    private bool _breakerMode = false;
    private List<Coordinate> _history = new List<Coordinate>();
    private bool _looping = false;

    public Bender(Grid<char> map)
    {
        _goal = map.Single('$');
        _map = map;
        _currentDirection = Directions.SOUTH;
        _position = map.Single('@');
        _teleporters.AddFrom(map);
    }

    private bool Navigating => !_looping && _position != _goal;

    public bool CanMove(Coordinate coordinate)
    {
        var value = _map[coordinate];
        if (value == '#') return false;
        if (value == 'X') return _breakerMode;
        return true;
    }

    public string[] GetPath()
    {
        var result = new List<string>();
        while (Navigating)
        {
            var n = Next;
            if (n == "LOOP") return new[] { "LOOP" };
            result.Add(n);
        }
        return result.ToArray();
    }

    public string Next
    {
        get
        {
            if (new[] { 'N', 'S', 'E', 'W' }.Contains(_map[_position]))
                _currentDirection = Directions.FromChar(_map[_position]);

            if (_map[_position] == 'B') _breakerMode = !_breakerMode;

            var toCheck = _priorities.Next(_currentDirection);

            foreach (var direction in toCheck)
            {
                var coord = _position.Move(direction);
                if (!CanMove(coord)) continue;

                _currentDirection = direction;
                _position = (_map[coord] == 'T')
                    ? _teleporters[coord]
                    : _position = coord;

                if (_map[coord] == 'I') _priorities.Reverse();
                if (_breakerMode && _map[coord] == 'X') _map[coord] = ' ';
                break;
            }

            _history.Add(_position);

            if (_history.IsRepeating())
            {
                _looping = true;
                _currentDirection = "LOOP";
            }

            return _currentDirection;
        }
    }
}

public class Priorities
{
    private string[] _priorities = new string[] { Directions.SOUTH, Directions.EAST, Directions.NORTH, Directions.WEST };

    public int Length => _priorities.Length;

    public string[] Next(string current)
    {
        var result = new List<string>(_priorities);
        if (current != _priorities[0])
            result.Insert(0, current);
        return result.ToArray();
    }

    public void Reverse() => _priorities = _priorities.Reverse().ToArray();
}

public class Grid<T>
{
    public int Width => _data.GetLength(0);
    public int Height => _data.GetLength(1);

    private T[,] _data;
    private int _currentRow = 0;

    public Grid(int w, int h)
    {
        _data = new T[w, h];
    }

    public Coordinate Single(T value) => All(value).Single();

    public Coordinate[] All(T value)
    {
        var result = new List<Coordinate>();
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                // Hack due to lack of '==' support for generics.
                if (_data[x, y].ToString() == value.ToString())
                    result.Add(new Coordinate(x, y));
            }
        }
        return result.ToArray();
    }

    public Grid<T> AddRow(params T[] row)
    {
        for (int x = 0; x < row.Length; x++)
            _data[x, _currentRow] = row[x];
        _currentRow++;
        return this;
    }

    public T this[Coordinate coord]
    {
        get => _data[coord.X, coord.Y];
        set => _data[coord.X, coord.Y] = value;
    }
}

public struct Coordinate
{
    public int X { get; }
    public int Y { get; }

    public Coordinate(int x, int y)
    {
        X = x;
        Y = y;
    }

    public static bool operator ==(Coordinate a, Coordinate b) => a.X == b.X && a.Y == b.Y;
    public static bool operator !=(Coordinate a, Coordinate b) => a.X != b.X || a.Y != b.Y;
    public override string ToString() => $"({X},{Y})";
}

public static class Extensions
{
    public static Grid<char> ToGrid(this IEnumerable<string> input)
    {
        var inputs = input.ToArray();
        var dimensions = inputs[0].Split(' ');
        var grid = new Grid<char>(int.Parse(dimensions[1]), int.Parse(dimensions[0]));

        for (int y = 1; y <= grid.Height; y++)
        {
            grid.AddRow(inputs[y].ToCharArray());
        }

        return grid;
    }

    public static Coordinate Move(this Coordinate coord, string direction)
    {
        switch (direction)
        {
            case Directions.SOUTH:
                return new Coordinate(coord.X, coord.Y + 1);
            case Directions.NORTH:
                return new Coordinate(coord.X, coord.Y - 1);
            case Directions.EAST:
                return new Coordinate(coord.X + 1, coord.Y);
            case Directions.WEST:
                return new Coordinate(coord.X - 1, coord.Y);
            default:
                throw new Exception("Unexepected direction.");
        }
    }

    public static Dictionary<Coordinate, Coordinate> AddFrom(this Dictionary<Coordinate, Coordinate> teleporters, Grid<char> map)
    {
        var t = map.All('T').ToArray();

        if (t.Count() == 2)
        {
            teleporters.Add(t[0], t[1]);
            teleporters.Add(t[1], t[0]);
        }

        return teleporters;
    }

    public static bool IsRepeating(this List<Coordinate> history)
    {
        // Terrible, hacky loop detection
        var check = string.Join("", history.Skip(history.Count() - 30));
        var allHistory = string.Join("", history);
        var erased = allHistory.Replace(check, string.Empty);

        var repeating = erased.Length < (allHistory.Length - check.Length);
        return repeating;
    }
}

public static class Directions
{
    public const string SOUTH = "SOUTH";
    public const string EAST = "EAST";
    public const string NORTH = "NORTH";
    public const string WEST = "WEST";

    private static string[] array = new[] { NORTH, SOUTH, EAST, WEST };

    public static string FromChar(char @char) => array.First(x => x[0] == @char);
}