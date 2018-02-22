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

        Console.WriteLine(Find(input));
    }

    public static IEnumerable<string> Find(IEnumerable<string> input)
    {
        // Draw Grid
        var grid = input.ToGrid();

        Console.Error.WriteLine(grid.Draw());

        var bender = new Bender(grid);

        // Priorities S, E, N, W (Can be Reversed)

        var result = new List<string>();
        while (bender.Navigating)
        {
            Console.Error.WriteLine($"{bender}");
            result.Add(bender.Next);
        }

        return result.ToArray();

        return new string[] { "SOUTH", "SOUTH", "EAST", "EAST" };
    }
}

public class Bender
{
    private readonly Coordinate _goal;
    private readonly Grid<char> _map;

    private Coordinate _position;
    private string _currentDirection;

    public Bender(Grid<char> map)
    {
        _goal = map.Find('$');
        _map = map;
        _currentDirection = Directions.SOUTH;
        _position = map.Find('@');
    }

    public bool Navigating
    {
        get { return _position != _goal; }
    }

    public bool CanMove(Coordinate coordinate)
    {
        if (new[] { '#', 'X' }.Contains(_map[coordinate.X, coordinate.Y]))
            return false;

        return true;
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

            var priorities = new string[] { _currentDirection, Directions.SOUTH, Directions.EAST, Directions.NORTH, Directions.WEST };
            for (int i = 0; i < priorities.Length; i++)
            {
                // Clear? GO!
                var coord = _position.Move(priorities[i]);
                Console.Error.WriteLine($"Checking {coord} going {priorities[i]}");
                if (CanMove(coord))
                {
                    Console.Error.WriteLine($"Moving to {coord}");
                    _position = coord;
                    _currentDirection = priorities[i];
                    return priorities[i];
                }
            }

            // Still here? We're stuck!
            return "LOOP";

            // TODO: Detect Actual Loop
        }
    }

    public override string ToString()
    {
        return $"Bender at {_position}, heading {_currentDirection} trying to find {_goal}";
    }
}

public class Priorities
{
    private char[]  _priorities = new char[] { 'S', 'E', 'N', 'W' };

    public char Next(char current)
    {
        var idx = (Array.IndexOf(_priorities, current) + 1) % _priorities.Length;
        return _priorities[idx];
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

    public Coordinate Find(T value)
    {
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                // Hack due to lack of '==' support for generics.
                if (_data[x, y].ToString() == value.ToString())
                    return new Coordinate(x, y);
            }
        }

        throw new Exception($"Unable to find '{value.ToString()}'.");
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
        return a.X != b.X && a.Y != b.Y;
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