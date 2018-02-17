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

        Console.Error.WriteLine(string.Join(Environment.NewLine, people.OrderByDescending(x => x.Value.Count).Select(kvp => $"{kvp.Key}: {string.Join(", ", kvp.Value)}")));

        // TODO: Can we make this not search EVERYTHING?
        // Need to get a weighting from leaf > root nodes
        var p1 = people.GetBFSPath(people.Keys.First());
        var p2 = people.GetBFSPath(p1.Last());
        Console.Error.WriteLine($"Path 1: {string.Join(" -> ", p1)} ({p1.Count()} Hops)");
        Console.Error.WriteLine($"Path 2: {string.Join(" -> ", p2)} ({p2.Count()} Hops)");
        var central = p2[p2.Length / 2];
        Console.Error.WriteLine($"Central Node: {central}");
        var search = people.Where(x => x.Key == central);

        var bestCount = int.MaxValue;

        foreach (var node in search)
        {
            var result = people.CountLayers(node.Key);

            Console.Error.WriteLine($"Node: {node.Key} took {result} hours with {node.Value.Count} adjacents.");

            if (result < bestCount)
            {
                bestCount = result;
                Console.Error.WriteLine($"-- Best Time Updated to {result} hours.");
            }
        };

        return bestCount;
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
        //Console.Error.WriteLine($"Counting layers in Graph of {graph.Keys.Count} items from Node {startAt}");

        var visited = new HashSet<int>();
        var toSearch = new Queue<Tuple<int, int>>();
        var result = 0;

        toSearch.Enqueue(Tuple.Create(startAt, 0));

        while (toSearch.Count > 0)
        {
            var n = toSearch.Dequeue();
            if (n.Item2 > result) result = n.Item2;
            visited.Add(n.Item1);

            var connected = graph[n.Item1];
            var unvisited = connected.Where(k => !visited.Contains(k)).ToList();

            foreach (var adjacent in unvisited)
            {
                visited.Add(adjacent);
                toSearch.Enqueue(Tuple.Create(adjacent, n.Item2 + 1));
            }
        }

        return result;
    }

    public static int[] GetBFSPath(this Dictionary<int, HashSet<int>> graph, int startAt)
    {
        Console.Error.WriteLine($"Getting BFS Path for {startAt}");
        var visited = new HashSet<int>();
        var toSearch = new Queue<Tuple<int, int>>();

        toSearch.Enqueue(Tuple.Create(startAt, 0));

        while (toSearch.Count > 0)
        {
            var n = toSearch.Dequeue();
            visited.Add(n.Item1);

            var connected = graph[n.Item1];
            var unvisited = connected.Where(k => !visited.Contains(k)).ToList();

            foreach (var adjacent in unvisited)
            {
                visited.Add(adjacent);
                toSearch.Enqueue(Tuple.Create(adjacent, n.Item2 + 1));
            }
        }

        return visited.ToArray();
    }
}