using System;
using System.Linq;
using System.Collections.Generic;

public class Solution
{
    static Func<int> ReadInt = () => int.Parse(Console.ReadLine());

    static void Main(string[] args)
    {

        Console.WriteLine(ConwaySequence.Find(ReadInt(), ReadInt()));
    }
}

public static class ConwaySequence
{
    public static string Find(int start, int targetLine)
    {
        if (targetLine == 1)
            return start.ToString();

        var lines = new List<int[]> { new[] { start } };

        for (int currentLine = 1; currentLine < targetLine; currentLine++)
        {
            var line = lines.Last();
            var encoded = new List<int>();
            var count = 0;

            for (int i = 0; i < line.Length; i++)
            {
                count++;
                var current = line[i];

                if (i + 1 == line.Length || line[i + 1] != current)
                {
                    encoded.AddRange(current.Encode(count));
                    count = 0;
                }
            }

            lines.Add(encoded.ToArray());
        }

        return lines.Last().SpacedOut(); // o_O
    }
}

public static class Extensions
{
    public static string SpacedOut(this IEnumerable<int> chars)
    {
        return string.Join(" ", chars);
    }

    public static int[] Encode(this int @int, int count)
    {
        return new int[]
        {
            count,
            @int
        };
    }
}