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
        string[] inputs = Console.ReadLine().Split(' ');
        int L = int.Parse(inputs[0]);
        int C = int.Parse(inputs[1]);
        for (int i = 0; i < L; i++)
        {
            string row = Console.ReadLine();
        }

        Console.WriteLine("answer");
    }

    public static IEnumerable<string> Find(IEnumerable<string> input)
    {
        return new string[] { "SOUTH", "SOUTH", "EAST", "EAST" };
    }
}