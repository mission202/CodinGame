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
        int N = int.Parse(Console.ReadLine());
        var input = Enumerable.Range(0, N).Select(i => Console.ReadLine()).ToList();
        Console.WriteLine(Find(input));
    }

    public static long Find(IEnumerable<string> input)
    {
        if (input.Count() < 2)
            return 0;

        var coordinates = input
            .Select(s => new Coordinate(s))
            .ToList();

        var median = coordinates.OrderBy(c => c.Y).GetMedian().Y;

        long result = 0;

        // Add main cable length ...
        result += (coordinates.Max(h => h.X) - coordinates.Min(h => h.X));

        // ... and cable to each house.
        var houseCabling = coordinates
            .Select(c => Math.Abs(c.Y - median))
            .Sum();

        result += houseCabling;

        return result;
    }
}

public struct Coordinate
{
    public long X { get; set; }
    public long Y { get; set; }

    public Coordinate(string input)
    {
        var values = input.Split(' ').Select(int.Parse).ToArray();
        X = values[0];
        Y = values[1];
    }

    public override string ToString()
    {
        return $"{X},{Y}";
    }
}

public static class Extensions
{
    public static T GetMedian<T>(this IEnumerable<T> data)
    {
        return data.ElementAt(data.Count() / 2);
    }
}