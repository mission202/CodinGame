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
        var lines = new List<char[]> { new[] { start.ToString()[0] } };

        for (int currentLine = 0; currentLine < targetLine; currentLine++)
        {
            var line = lines.Last();
            D.Print($"On Line {currentLine}, Last Line: {string.Join(" ", line)}");

            var current = line[0];
            var count = 0;

            for (int charIdx = 0; charIdx < line.Length; charIdx++)
            {
                // Same as Last? Add to Count
                var next = line[charIdx];
                if (next == current)
                {
                    count++;
                    D.Print($"Current Char {next} same as last, incremented count to {count}.");
                }
                else
                {
                    current = next;
                    count = 1;
                }

                // Encode
                var chars = Enumerable.Range(0, count).Select(x => current).ToArray();
                lines.Add(chars);
                D.Print($"Added '{chars.SpacedOut()}' to lines. Count now at {lines.Count()}.");
            }
        }

        return lines.SpacedOut(); // o_O
    }
}

public static class Extensions
{
    public static string SpacedOut(this IEnumerable<char> chars)
    {
        return string.Join(" ", chars);
    }

    public static string SpacedOut(this IEnumerable<char[]> lines)
    {
        return string.Join(" ", lines.Select(l => l.SpacedOut()));
    }
}