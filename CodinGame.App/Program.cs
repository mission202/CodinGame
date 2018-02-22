using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

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

    public static IEnumerable<string> Find(IEnumerable<string> input)
    {
        var grid = input.ToGrid();
        Console.Error.WriteLine(grid.Draw());
        var bender = new Bender(grid);
        return bender.GetPath();
    }
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

        var teleporters = _map.All('T').ToArray();

        if (teleporters.Count() == 2)
        {
            Console.Error.WriteLine($"Teleporters on Map @ {string.Join(" - ", teleporters)}");
            _teleporters.Add(teleporters[0], teleporters[1]);
            _teleporters.Add(teleporters[1], teleporters[0]);
        }
    }

    private bool Navigating => !_looping && _position != _goal;

    public bool CanMove(Coordinate coordinate)
    {
        var value = _map[coordinate.X, coordinate.Y];

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
            // Move if Directed by Square
            if (new[] { 'N', 'S', 'E', 'W' }.Contains(_map[_position.X, _position.Y]))
            {
                _currentDirection = Directions.FromChar(_map[_position.X, _position.Y]);
                Console.Error.WriteLine($"Current direction changed to '{_currentDirection}' due to tile '{_map[_position.X, _position.Y]}'");
            }

            if (_map[_position.X, _position.Y] == 'B')
                _breakerMode = !_breakerMode;

            // Check Current Direction then cycle priorities
            var toCheck = _priorities.Next(_currentDirection);

            foreach (var direction in toCheck)
            {
                // Clear? GO!
                var coord = _position.Move(direction);
                if (CanMove(coord))
                {
                    _position = (_map[coord.X, coord.Y] == 'T')
                        ? _teleporters[coord]
                        : _position = coord;

                    if (_map[coord.X, coord.Y] == 'I')
                        _priorities.Reverse();

                    if (_breakerMode && _map[coord.X, coord.Y] == 'X')
                        _map[coord.X, coord.Y] = ' ';

                    _currentDirection = direction;
                    break;
                }
            }

            _history.Add(_position);

            // Terrible, hacky loop detection

            var check = string.Join("", _history.Skip(_history.Count() - 10));
            var allHistory = string.Join("", _history);
            var erased = allHistory.Replace(check, string.Empty);
            Console.Error.WriteLine($"allHistory LEN: {allHistory.Length}. Erased LEN: {erased.Length}. Repeating? {(erased.Length < (allHistory.Length - check.Length))}");

            var repeating = erased.Length < (allHistory.Length - check.Length);
            if (repeating)
            {
                _looping = true;
                _currentDirection = "LOOP";
            };

            return _currentDirection;
        }
    }

    public override string ToString()
    {
        return $"Bender at {_position}, heading {_currentDirection} trying to find {_goal}";
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

    public void Reverse()
    {
        _priorities = _priorities.Reverse().ToArray();
    }
}

public class Grid<T>
{
    public int Width { get; private set; }
    public int Height { get; private set; }

    private T[,] _data;
    private int _currentRow = 0;

    public Grid(int w, int h)
    {
        Width = w;
        Height = h;

        _data = new T[w, h];
    }

    public Coordinate Single(T value)
    {
        return All(value).Single();
    }

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

    public string Draw()
    {
        var result = new StringBuilder();

        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                result.Append(_data[x, y]).ToString();
            }
            result.AppendLine();
        }

        return result.ToString(); ;
    }

    public Grid<T> AddRow(params T[] row)
    {
        for (int x = 0; x < row.Length; x++)
            _data[x, _currentRow] = row[x];
        _currentRow++;
        return this;
    }

    public T this[int x, int y]
    {
        get
        {
            return _data[x, y];
        }
        set
        {
            _data[x, y] = value;
        }
    }
}

public struct Coordinate
{
    public int X { get; set; }
    public int Y { get; set; }

    public Coordinate(int x, int y)
    {
        X = x;
        Y = y;
    }

    public static bool operator ==(Coordinate a, Coordinate b)
    {
        return a.X == b.X && a.Y == b.Y;
    }

    public static bool operator !=(Coordinate a, Coordinate b)
    {
        return a.X != b.X || a.Y != b.Y;
    }

    public override string ToString()
    {
        return $"({X},{Y})";
    }
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
                return new Coordinate(coord.X, ++coord.Y);
            case Directions.NORTH:
                return new Coordinate(coord.X, --coord.Y);
            case Directions.EAST:
                return new Coordinate(++coord.X, coord.Y);
            case Directions.WEST:
                return new Coordinate(--coord.X, coord.Y);
            default:
                throw new Exception("Unexepected direction.");
        }
    }
}

public static class Directions
{
    public const string SOUTH = "SOUTH";
    public const string EAST = "EAST";
    public const string NORTH = "NORTH";
    public const string WEST = "WEST";

    public static string FromChar(char @char)
    {
        return new[] { NORTH, SOUTH, EAST, WEST }.First(x => x[0] == @char);
    }
}