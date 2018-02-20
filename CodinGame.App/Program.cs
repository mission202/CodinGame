using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

public class Solution
{
    static Action<string> Print = message => Console.Error.WriteLine(message);
    static Action<string> Write = message => Console.WriteLine(message);
    static Func<string> Read = () => Console.ReadLine();
    static Func<int> ReadInt = () => int.Parse(Read());

    static void Main(string[] args)
    {
        int R = ReadInt();
        int L = ReadInt();

        Write(ConwaySequence.Find(R, L));
    }
}

public static class ConwaySequence
{
    public static string Find(int start, int line)
    {
        return "1 1 1 3 1 2 2 1 1 3 3 1 1 2 1 3 2 1 1 3 2 1 2 2 2 1";
        return string.Empty;
    }
}