using System;
using System.Linq;
using System.Collections.Generic;

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
        return new Trie()
            .For(numbers)
            .NodeCount;
    }
}
public class Trie
{
    public int NodeCount => _root.NodeCount;

    private readonly TrieNode _root = new TrieNode();

    public Trie For(IEnumerable<string> numbers)
    {
        numbers.ToList().ForEach(AddNumber);
        return this;
    }

    public void AddNumber(string number)
    {
        TrieNode temp = _root;
        var chars = number.ToCharArray();
        for (int i = 0; i < chars.Length; i++)
            temp = temp.Add(chars[i]);
    }
}

public class TrieNode
{
    public int NodeCount => _nodes.Keys.Count + _nodes.Values.Sum(n => n.NodeCount);

    private Dictionary<char, TrieNode> _nodes = new Dictionary<char, TrieNode>();

    public TrieNode Add(char @char)
    {
        if (_nodes.ContainsKey(@char))
            return _nodes[@char];

        var node = new TrieNode();
        _nodes.Add(@char, node);
        return node;
    }
}