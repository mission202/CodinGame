using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

/**
 * Auto-generated code below aims at helping you parse
 * the standard input according to the problem statement.
 **/
public class Solution
{
    static void Main(string[] args)
    {
        var input = Enumerable.Range(0, int.Parse(Console.ReadLine()))
            .Select(i => Console.ReadLine())
            .ToArray();
        Console.WriteLine(Find(input));
    }

    public static int Find(IEnumerable<string> numbers)
    {
        return 10;
    }
}