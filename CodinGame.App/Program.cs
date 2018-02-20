using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

public static class D
{
    public static Action<string> Print = message => Console.Error.WriteLine(message);
    public static Action<string> Write = message => Console.WriteLine(message);
    public static Func<string> Read = () => Console.ReadLine();
    public static Func<int> ReadInt = () => int.Parse(Read());
}

public class Solution
{
    static void Main(string[] args)
    {
        int R = D.ReadInt();
        int L = D.ReadInt();

        D.Write(ConwaySequence.Find(R, L));
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
            D.Print($"Encoding Line {currentLine}: {string.Join(" ", line)}");

            var encoded = new List<int>();
            var count = 0;

            for (int i = 0; i < line.Length; i++)
            {
                count++;
                var current = line[i];

                if (i + 1 == line.Length || line[i + 1] != current)
                {
                    // Encode and close this 'block'
                    D.Print($"Encoding: {count} * {current}");
                    encoded.AddRange(current.Encode(count));
                    count = 0;
                }
            }

            lines.Add(encoded.ToArray());
            D.Print("Current State");
            D.Print(lines.Pretty());
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

    public static string SpacedOut(this IEnumerable<int[]> lines)
    {
        return string.Join(" ", lines.Select(l => l.SpacedOut()));
    }

    public static string Pretty(this IEnumerable<int[]> lines)
    {
        return string.Join(Environment.NewLine, lines.Select(l => l.SpacedOut()));
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