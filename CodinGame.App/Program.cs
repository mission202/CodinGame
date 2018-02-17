using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

public class Solution
{
    static void Main(string[] args)
    {
        int n = int.Parse(Console.ReadLine()); // the number of adjacency relations
        var lines = Enumerable.Range(0, n).Select(i => Console.ReadLine());

        // The minimal amount of steps required to completely propagate the advertisement
        Console.WriteLine(Find(lines));
    }

    public static int Find(IEnumerable<string> input)
    {
        var people = new Dictionary<int, HashSet<int>>();

        foreach (var line in input)
        {
            string[] inputs = line.Split(' ');
            int xi = int.Parse(inputs[0]); // the ID of a person which is adjacent to yi
            int yi = int.Parse(inputs[1]); // the ID of a person which is adjacent to xi

            people.AddUndirected(xi, yi);
        }

        Console.Error.WriteLine(string.Join(Environment.NewLine, people.Select(kvp => $"{kvp.Key}: {string.Join(", ", kvp.Value)}")));

        // TODO: Brute Force 'Fastest Route'

        // Start with Most Connected?
        var mostConnected = people
            .OrderByDescending(x => x.Value.Count)
            .First()
            .Key;

        // BFS to Coverage!
        return people.CountLayers(mostConnected);
    }
}

public static class Extensions
{
    public static Dictionary<int, HashSet<int>> AddUndirected(this Dictionary<int, HashSet<int>> dictionary, int a, int b)
    {
        if (dictionary.ContainsKey(a))
        {
            dictionary[a].Add(b);
        }
        else
        {
            dictionary.Add(a, new HashSet<int> { b });
        }

        if (dictionary.ContainsKey(b))
        {
            dictionary[b].Add(a);
        }
        else
        {
            dictionary.Add(b, new HashSet<int> { a });
        }

        return dictionary;
    }

    public static int CountLayers(this Dictionary<int, HashSet<int>> graph, int startAt)
    {
        var visited = new HashSet<int>();
        var toSearch = new Queue<Tuple<int, int>>();
        var result = 0;

        visited.Add(startAt);
        toSearch.Enqueue(Tuple.Create(startAt, 0));

        while (toSearch.Count > 0)
        {
            // Pop
            var n = toSearch.Dequeue();

            // Level Up (If Deeper)
            if (n.Item2 > result) result = n.Item2;

            // Visit First
            visited.Add(n.Item1);

            // Queue Unvisited Connected
            Console.Error.WriteLine($"Getting Connected Items for: {n.Item1}");
            var connected = graph[n.Item1];
            var nextUnvisited = connected.Where(k => !visited.Contains(k)).ToList();

            if (nextUnvisited.Any())
                toSearch.Enqueue(Tuple.Create(nextUnvisited.First(), n.Item2 + 1));
        }

        return result;
    }
}