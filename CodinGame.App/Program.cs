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

    public static int Find(IEnumerable<string> input)
    {
        // Thanks http://fooplot.com for the visualisation!

        if (input.Count() < 2)
            return 0;

        var coordinates = input
            .Select(s => new Coordinate(s));

        var groupedByX = coordinates
            .GroupBy(c => c.X)
            .OrderBy(g => g.Key)
            .ToList();

        Console.Error.WriteLine($"X-Axis Groups: {groupedByX.Count}");

        var minX = coordinates.Min(h => h.X);
        var maxX = coordinates.Max(h => h.X);
        var minY = coordinates.Min(h => h.Y);
        var maxY = coordinates.Max(h => h.Y);
        var median = coordinates.OrderBy(c => c.Y).GetMedian().Y;
        Console.Error.WriteLine($"Median: {median}");

        var result = 0;

        for (int x = minX; x < (maxX + 1); x++)
        {
            var houses = groupedByX.SingleOrDefault(g => g.Key == x);

            if (houses != null)
            {
                // Calculate Distance to each how up/down.
                var houseCable = houses.Select(h =>
                {
                    int toHouse = Math.Abs(h.Y - median);
                    Console.Error.WriteLine($"{toHouse} to house {h}");
                    return toHouse;
                })
                .Sum();

                result += houseCable;
            }

            if (x != maxX)
                result++; // +1 for the X

            Console.Error.WriteLine($"Cable Distance Now @ {result}.");
        }

        return result;
    }
}

public struct Coordinate
{
    public int X { get; set; }
    public int Y { get; set; }

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
        // https://github.com/mathnet/mathnet-numerics/blob/master/src/Numerics/Statistics/ArrayStatistics.cs#L414
        /*

            var k = data.Length/2;
            return data.Length.IsOdd()
                ? SelectInplace(data, k)
                : (SelectInplace(data, k - 1) + SelectInplace(data, k))/2.0;

         */

        var k = data.Count() / 2;
        return data.ElementAt(k);
    }
}