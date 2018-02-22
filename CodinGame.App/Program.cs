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

        // Place Bender at the Start @
        var start = grid.Find('@');

        Console.Error.WriteLine($"Bender @ {start}");

        // Priorities S, E, N, W (Can be Reversed)

        var result = new List<string>();


        return new string[] { "SOUTH", "SOUTH", "EAST", "EAST" };
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
                if (_data[x,y].ToString() == value.ToString())
                    return new Coordinate(x, y);
            }
        }

        throw new Exception($"Unable to find '{value.ToString()}.");
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

        return result.ToString();;
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
}