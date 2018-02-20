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

        var lines = new List<char[]> { new[] { start.ToString()[0] } };

        for (int currentLine = 1; currentLine < targetLine; currentLine++)
        {
            var line = lines.Last();
            D.Print($"Encoding Line {currentLine}: {string.Join(" ", line)}");

            var encoded = new List<char>();
            var count = 0;

            for (int charIdx = 0; charIdx < line.Length; charIdx++)
            {
                count++;
                var current = line[charIdx];

                if (charIdx + 1 == line.Length || line[charIdx + 1] != current)
                {
                    // Encode and close this 'block'
                    D.Print($"Encoding: {(char)(48 + count)} * {current}");
                    encoded.AddRange(current.EncodeChar(count));
                    count = 0;
                }
            }

            lines.Add(encoded.ToArray());
            D.Print("Current State");
            D.Print(lines.Pretty());
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

    public static string Pretty(this IEnumerable<char[]> lines)
    {
        return string.Join(Environment.NewLine, lines.Select(l => l.SpacedOut()));
    }

    public static char[] EncodeChar(this char @char, int count)
    {
        return new char[]
        {
           (char)(48 + count),
           @char
        };
    }
}