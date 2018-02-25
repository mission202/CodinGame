using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

public class Game
{
    public const int MAX_SAMPLES = 3;
    public const int MAX_MOLECULES = 10;

    static void Main(string[] args)
    {
        string[] inputs;
        int projectCount = int.Parse(Console.ReadLine());
        for (int i = 0; i < projectCount; i++)
        {
            inputs = Console.ReadLine().Split(' ');
            var project = new MoleculeCollection(inputs.Select(int.Parse).ToArray());
            //Console.Error.WriteLine($"Project: {project}");
        }

        Player player;
        Player opponent;
        List<SampleDataFile> samples;

        // game loop
        while (true)
        {
            player = new Player(Console.ReadLine());
            opponent = new Player(Console.ReadLine());

            inputs = Console.ReadLine().Split(' ');
            var available = new MoleculeCollection(inputs.Select(int.Parse).ToArray());
            //Console.Error.WriteLine($"Available: {available}");

            samples = new List<SampleDataFile>();
            int sampleCount = int.Parse(Console.ReadLine());
            for (int i = 0; i < sampleCount; i++)
            {
                var input = Console.ReadLine();
                Console.Error.WriteLine($"S {i} - {input}");
                var sample = new SampleDataFile(input);
                Console.Error.WriteLine($"Parsed {sample.Cost}");

                samples.Add(sample);
                //Console.Error.WriteLine($"Sample {sample.Id}: {sample.Cost}");
            }

            var ai = new AI(samples, player);
            Console.WriteLine(ai.GetNextAction());
        }
    }
}

public class AI
{
    public List<SampleDataFile> Samples { get; }
    public Player Player { get; }

    public const int MAX_SAMPLES = 3;
    public const int MAX_MOLECULES = 10;

    public AI(List<SampleDataFile> samples, Player player)
    {
        Samples = samples;
        Player = player;
    }

    public string GetNextAction()
    {
        // No Sample Data Files? Go to DIAGNOSIS
        var heldByPlayer = Samples.Where(x => x.CarriedByPlayer).ToList();

        // We've done grunt work - research away!
        if (Player.Target == Modules.LABORATORY && heldByPlayer.Count > 0)
        {
            var toResearch = heldByPlayer.OrderByDescending(x => x.Health).First();
            return $"CONNECT {toResearch.Id}";
        }

        if (heldByPlayer.Count() == 0)
        {
            if (Player.Target != Modules.DIAGNOSIS) return $"GOTO {Modules.DIAGNOSIS}";

            // Connect and Get Best Sample
            var bestSample = Samples.Where(x => x.InCloud).OrderByDescending(x => x.Health).First();
            return $"CONNECT {bestSample.Id}";
        }

        // Got Files, Need Molecules? Go to MOLECULES
        if (heldByPlayer.Count() > 0)
        {
            if (Player.Target != Modules.MOLECULES)
                return $"GOTO {Modules.MOLECULES}";

            // Connect and Types for Best Samples (up to max of 10);
            var shoppingFor = heldByPlayer.OrderByDescending(x => x.Health).First();

            if (Player.Storage.A < shoppingFor.Cost.A) return "CONNECT A";
            if (Player.Storage.B < shoppingFor.Cost.B) return "CONNECT B";
            if (Player.Storage.C < shoppingFor.Cost.C) return "CONNECT C";
            if (Player.Storage.D < shoppingFor.Cost.D) return "CONNECT D";
            if (Player.Storage.E < shoppingFor.Cost.E) return "CONNECT E";

            Console.Error.WriteLine($"Shopping: {shoppingFor.Id} {shoppingFor.Cost} | Storage: {Player.Storage}");

            // Got Molecules? Go to LABORATORY
            return $"GOTO {Modules.LABORATORY}";
        }

        // Shouldn't really get here.
        return "GOTO HELL";
    }
}

public class Player
{
    public string Target { get; private set; }
    public SampleDataFile[] SampleDataFiles { get; private set; }
    public MoleculeCollection Storage { get; private set; }
    public MoleculeCollection Expertise { get; private set; }

    public Player(string input)
    {
        var inputs = input.Split(' ');
        Target = inputs[0];

        var ints = inputs.Skip(1).Select(int.Parse).ToArray();
        int eta = ints[1];
        int score = ints[2];

        Storage = new MoleculeCollection(ints.Skip(2).Take(5).ToArray());
        //Console.Error.WriteLine($"Storage: {Storage}");
        Expertise = new MoleculeCollection(ints.Skip(7).Take(5).ToArray());
        //Console.Error.WriteLine($"Expertise: {Expertise}");
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
    public bool CarriedByPlayer => CarriedBy == 0;
    public bool CarriedByOpponent => CarriedBy == 1;
    public bool InCloud => CarriedBy == -1;

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
        Cost = new MoleculeCollection(inputs.Skip(5).Take(5).Select(int.Parse).ToArray());
    }
}

public static class Modules
{
    public const string DIAGNOSIS = "DIAGNOSIS";
    public const string MOLECULES = "MOLECULES";
    public const string LABORATORY = "LABORATORY";
}