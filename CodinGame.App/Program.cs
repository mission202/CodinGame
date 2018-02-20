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

        Console.WriteLine(Find(lines));
    }

    public static int Find(IEnumerable<string> input)
    {
        // I missed this KEY information the first time around :)
        // "We can assume this network contains no cyclic relation."
        // Changes ALL THE THINGS!

        var people = new Graph(input);
        var result = people.CountLayersFromLeaves();

        return result;
    }
}

public class Graph
{
    private readonly Dictionary<int, HashSet<int>> _graph = new Dictionary<int, HashSet<int>>();

    public Graph(IEnumerable<string> input)
    {
        foreach (var line in input)
        {
            string[] inputs = line.Split(' ');
            int xi = int.Parse(inputs[0]); // the ID of a person which is adjacent to yi
            int yi = int.Parse(inputs[1]); // the ID of a person which is adjacent to xi

            AddUndirected(xi, yi);
        }
    }

    public void Remove(int node)
    {
        // Remove Reference from Adjacents
        foreach (var adjacent in _graph[node])
            _graph[adjacent].Remove(node);

        _graph.Remove(node);
    }

    public void AddUndirected(int a, int b)
    {
        if (_graph.ContainsKey(a))
            _graph[a].Add(b);
        else
            _graph.Add(a, new HashSet<int> { b });

        if (_graph.ContainsKey(b))
            _graph[b].Add(a);
        else
            _graph.Add(b, new HashSet<int> { a });
    }

    public int CountLayersFromLeaves()
    {
        var counter = 0;

        while (_graph.Keys.Count > 1)
        {
            counter++;

            foreach (var leaf in LeafNodes)
                Remove(leaf);
        }

        return counter;
    }

    private int[] LeafNodes => _graph.Where(x => x.Value.Count == 1).Select(x => x.Key).ToArray();
}