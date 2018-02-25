using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

public class Game
{
    static void Main(string[] args)
    {
        string[] inputs;
        int projectCount = int.Parse(Console.ReadLine());
        for (int i = 0; i < projectCount; i++)
        {
            inputs = Console.ReadLine().Split(' ');
            var project = new MoleculeCollection(inputs.Select(int.Parse).ToArray());
            Console.Error.WriteLine($"Project: {project}");
        }

        // game loop
        while (true)
        {
            for (int i = 0; i < 2; i++)
            {
                var input = Console.ReadLine();
                var player = new Player(input);
            }

            inputs = Console.ReadLine().Split(' ');

            var available = new MoleculeCollection(inputs.Select(int.Parse).ToArray());
            Console.Error.WriteLine($"Available: {available}");

            int sampleCount = int.Parse(Console.ReadLine());
            for (int i = 0; i < sampleCount; i++)
            {
                var input = Console.ReadLine();
                var sample = new SampleDataFile(input);
                Console.Error.WriteLine($"Sample {sample.Id}: {sample.Cost}");
            }

            Console.WriteLine("GOTO HELL");
        }
    }
}

public class Player
{
    public SampleDataFile[] SampleDataFiles { get; private set; }
    public char[] Molecules { get; private set; }

    public Player(string input)
    {
        var inputs = input.Split(' ');
        string target = inputs[0];

        var ints = inputs.Skip(1).Select(int.Parse).ToArray();
        int eta = ints[1];
        int score = ints[2];

        var storage = new MoleculeCollection(ints.Skip(2).Take(5).ToArray());
        Console.Error.WriteLine($"Storage: {storage}");
        var expertise = new MoleculeCollection(ints.Skip(7).Take(5).ToArray());
        Console.Error.WriteLine($"Expertise: {storage}");
    }
}

public class MoleculeCollection
{
    public int A { get; private set; }
    public int B { get; private set; }
    public int C { get; private set; }
    public int D { get; private set; }
    public int E { get; private set; }

    public MoleculeCollection(int[] values)
    {
        A = values[0];
        B = values[1];
        C = values[2];
        D = values[3];
        E = values[4];
    }

    public override string ToString() => $"A: {A}, B: {B}, C: {C}, D: {D}, E: {E}";
}

public class SampleDataFile
{
    public int Id { get; private set; }
    public int CarriedBy { get; private set; }
    public int Rank { get; private set; }
    public string Gain { get; private set; }
    public int Health { get; private set; }

    public MoleculeCollection Cost { get; private set; }

    public SampleDataFile(string input)
    {
        var inputs = input.Split();
        Id = int.Parse(inputs[0]);
        CarriedBy = int.Parse(inputs[1]);
        Rank = int.Parse(inputs[2]);
        Gain = inputs[3];
        Health = int.Parse(inputs[4]);
        Cost = new MoleculeCollection(inputs.Skip(4).Take(5).Select(int.Parse).ToArray());
    }
}

public static class Modules
{
    public const string DIAGNOSIS = "DIAGNOSIS";
    public const string MOLECULES = "MOLECULES";
    public const string LABORATORY = "LABORATORY";
}