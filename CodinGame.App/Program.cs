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
        var people = new Graph(input);

        foreach (var line in input)
        {
            string[] inputs = line.Split(' ');
            int xi = int.Parse(inputs[0]); // the ID of a person which is adjacent to yi
            int yi = int.Parse(inputs[1]); // the ID of a person which is adjacent to xi

            people.AddUndirected(xi, yi);
        }

        // TODO: Can we make this not search EVERYTHING?
        var central = people.GetCentralNodes();
        Console.Error.WriteLine($"Central Nodes: {central.CommaSeparated()}");
        var search = people.Only(central);

        var bestCount = int.MaxValue;

        foreach (var node in central)
        {
            var result = people.CountLayers(node);

            Console.Error.WriteLine($"Node: {node} took {result} hours.");

            if (result < bestCount)
            {
                bestCount = result;
                Console.Error.WriteLine($"-- Best Time Updated to {result} hours.");
            }
        };

        return bestCount;
    }
}

public class Graph
{
    private readonly Dictionary<int, HashSet<int>> _graph = new Dictionary<int, HashSet<int>>();

    public Graph(Graph seed)
    {
        _graph = new Dictionary<int, HashSet<int>>();
        seed._graph
            .ToList()
            .ForEach(kvp => _graph.Add(kvp.Key, new HashSet<int>(kvp.Value)));
    }

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

    public Graph Remove(int node)
    {
        _graph.Remove(node);
        _graph
            .Where(x => x.Value.Contains(node))
            .ToList()
            .ForEach(x => x.Value.Remove(node));
        return this;
    }

    public int[] GetCentralNodes()
    {
        var clone = new Graph(this);
        var nodes = new HashSet<int>(clone._graph.Keys);

        var counter = 0;

        Console.Error.WriteLine($"Getting Central Node(s) for Graph: {clone._graph.Keys.CommaSeparated()}");

        while (nodes.Count > 2)
        {
            // Get Outer-Edge Nodes
            var leaves = clone._graph
                .GroupBy(x => x.Value.Count)
                .OrderBy(x => x.Key);

            if (leaves.Count() == 1) {
                return nodes.ToArray();
            }

            // Remove from Graph
            foreach (var leaf in leaves.First())
            {
                Console.Error.WriteLine($"Removing Leaf Node {leaf.Key}");

                clone.Remove(leaf.Key);
                nodes.Remove(leaf.Key);
            }

            if (nodes.Count < 3) break;
        }

        return nodes.ToArray();
    }

    public Dictionary<int, HashSet<int>> AddUndirected(int a, int b)
    {
        if (_graph.ContainsKey(a))
        {
            _graph[a].Add(b);
        }
        else
        {
            _graph.Add(a, new HashSet<int> { b });
        }

        if (_graph.ContainsKey(b))
        {
            _graph[b].Add(a);
        }
        else
        {
            _graph.Add(b, new HashSet<int> { a });
        }

        return _graph;
    }

    public int CountLayers(int startAt)
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

            var connected = _graph[n.Item1];
            var unvisited = connected.Where(k => !visited.Contains(k)).ToList();

            foreach (var adjacent in unvisited)
            {
                visited.Add(adjacent);
                toSearch.Enqueue(Tuple.Create(adjacent, n.Item2 + 1));
            }
        }

        return result;
    }

    public IEnumerable<KeyValuePair<int, HashSet<int>>> Only(IEnumerable<int> ids)
    {
        return _graph.Where(x => ids.Contains(x.Key)).ToList();
    }
}

public static class Extensions
{
    public static string CommaSeparated(this IEnumerable<int> items)
    {
        return string.Join(", ", items);
    }

    public static int[] GetBFSPath(this Dictionary<int, HashSet<int>> graph, int startAt)
    {
        //Console.Error.WriteLine($"Getting BFS Path for {startAt}");
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

    public static int[] GetCentralNodes(this Dictionary<int, HashSet<int>> graph)
    {
        var nodes = new HashSet<int>(graph.Keys);
        var central = nodes.Count % 2 == 0 ? 2 : 1;

        Console.Error.WriteLine($"Getting Central {(central == 1 ? "Node" : "Nodes")} for Graph of {graph.Keys.Count} items");


        while (nodes.Count > central)
        {
            // Get Leaf Nodes
            var leaves = graph.Where(x => nodes.Contains(x.Key) && x.Value.Count == 1).ToList();

            // Remove from Graph
            foreach (var leaf in leaves)
            {
                graph.Remove(leaf.Key);

                // Remove from Adjacents
                var connected = graph.Where(x => x.Value.Contains(leaf.Key)).ToList();
                connected.ForEach(x => x.Value.Remove(leaf.Key));

                nodes.Remove(leaf.Key);
            }
        }

        return nodes.ToArray();
    }
}