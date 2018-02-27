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
            //Console.Error.WriteLine($"Project: {project}");
        }

        var game = new Game();

        // game loop
        while (true)
        {
            game.Setup(Console.ReadLine);

            Console.Error.WriteLine("Game State:");
            Console.Error.WriteLine(game.Replay);

            Console.WriteLine(game.GetNextAction());
        }
    }

    public const int MAX_SAMPLES = 3;
    public const int MAX_MOLECULES = 10;

    public Player Player { get; private set; }

    private Player _opponent;
    private MoleculeCollection _available;
    private Dictionary<int, SampleDataFile> _samples;
    private readonly AI _ai;

    private readonly StringBuilder _history = new StringBuilder();

    public Game()
    {
        _samples = new Dictionary<int, SampleDataFile>();
        _ai = new AI();
    }

    public Game(string state) : this()
    {
        var lines = state.Split('/');
        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line)) break;

            var split = line.Split('|');
            SetPlayer(split[0]);
            SetOpponent(split[1]);
            SetAvailableMolecules(split[2]);

            if (int.Parse(split[3]) > 0)
                split[4].Split(',').ToList().ForEach(s => UpdateSample(s));
        }
    }

    public void Setup(Func<string> inputSource)
    {
        var player = inputSource();
        var opponent = inputSource();
        var molecules = inputSource();
        var sampleCount = inputSource();

        SetPlayer(player);
        SetOpponent(opponent);
        SetAvailableMolecules(molecules);
        var samples = Enumerable.Range(0, int.Parse(sampleCount)).Select(i => inputSource()).ToList();
        samples.ForEach(s => UpdateSample(s));

        string turn = $"{player}|{opponent}|{molecules}|{sampleCount}|{string.Join(",", samples)}";
        Console.Error.WriteLine("This Turn");
        Console.Error.WriteLine(turn);
        _history.AppendLine(turn);
    }

    public Game SetPlayer(string input)
    {
        Player = new Player(input);
        return this;
    }

    public Game SetOpponent(string input)
    {
        _opponent = new Player(input);
        return this;
    }

    public Game SetAvailableMolecules(string values)
    {
        _available = new MoleculeCollection(values.Split(' ').Select(int.Parse).ToArray());
        return this;
    }

    public string GetNextAction()
    {
        return _ai.GetNextAction(_samples, Player);
    }

    public void UpdateSample(string input)
    {
        var data = new SampleDataFile(input);
        if (_samples.ContainsKey(data.Id))
            _samples[data.Id] = data;
        else
            _samples.Add(data.Id, data);
    }

    public string Replay
    {
        get
        {
            var split = _history.ToString().Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            return string.Join("/", split);
        }
    }
}

public class AI
{

    public const int MAX_SAMPLES = 3;
    public const int MAX_MOLECULES = 10;

    private readonly HashSet<int> _diagnosed = new HashSet<int>();
    private readonly HashSet<int> _researched = new HashSet<int>();
    private readonly Queue<string> _queue = new Queue<string>();

    private readonly Dictionary<string, int> _distances = new Dictionary<string, int>
    {
        { $"{Modules.START_POS}>{Modules.START_POS}", 0 },
        { $"{Modules.START_POS}>{Modules.SAMPLES}", 2 },
        { $"{Modules.START_POS}>{Modules.DIAGNOSIS}", 2 },
        { $"{Modules.START_POS}>{Modules.MOLECULES}", 2 },
        { $"{Modules.START_POS}>{Modules.LABORATORY}", 2 },
        { $"{Modules.SAMPLES}>{Modules.SAMPLES}", 0 },
        { $"{Modules.SAMPLES}>{Modules.DIAGNOSIS}", 3 },
        { $"{Modules.SAMPLES}>{Modules.MOLECULES}", 3 },
        { $"{Modules.SAMPLES}>{Modules.LABORATORY}", 3 },
        { $"{Modules.DIAGNOSIS}>{Modules.DIAGNOSIS}", 0 },
        { $"{Modules.DIAGNOSIS}>{Modules.MOLECULES}", 3 },
        { $"{Modules.DIAGNOSIS}>{Modules.LABORATORY}", 4 },
        { $"{Modules.MOLECULES}>{Modules.MOLECULES}", 0 },
        { $"{Modules.MOLECULES}>{Modules.LABORATORY}", 3 },
        { $"{Modules.LABORATORY}>{Modules.LABORATORY}", 0 }
    };

    public string GetNextAction(Dictionary<int, SampleDataFile> samples, Player player)
    {
        // Clear Queue (if any)
        if (_queue.Count > 0)
            return _queue.Dequeue();

        if (player.Target == Modules.START_POS)
            return Travel(player.Target, Modules.SAMPLES);

        // No Sample Data Files? Go to SAMPLES
        var heldByPlayer = samples.Where(x => x.Value.CarriedByPlayer && !_researched.Contains(x.Value.Id)).ToList();

        //if (heldByPlayer.Count() == 0)
        //{
        //    if (player.Target != Modules.SAMPLES)
        //        return Travel(player.Target, Modules.SAMPLES);

        //    // Connect and Get Best Sample / "Best" TBC
        //    var rank = 2; // 1 = Cheaper/Less Health, 3 = Expensive, More Health
        //    return $"CONNECT {rank}";
        //}

        if (player.Target == Modules.SAMPLES)
        {
            // Queue Up Collection of Samples
            // TODO: Make Selection of Rank Smarter (e.g. Availability of Modules/Expertise)
            var need = MAX_SAMPLES - heldByPlayer.Count;

            if (need == 0)
                return Travel(player.Target, Modules.DIAGNOSIS);

            for (int i = 0; i < need; i++)
                _queue.Enqueue(Actions.Connect(1));

            return _queue.Dequeue();
        }

        if (player.Target == Modules.DIAGNOSIS)
        {
            var toDiagnose = heldByPlayer
                .Where(x => IsUndiagnosed(x.Value)).ToList();

            if (toDiagnose.Any())
            {
                toDiagnose.ForEach(sample => _queue.Enqueue(Actions.Connect(sample.Value.Id)));
                return _queue.Dequeue();
            }

            return Travel(player.Target, Modules.MOLECULES);
        }

        // Got Files? Need to Get Molecule Requirements from DIAGNOSIS.
        if (heldByPlayer.Count() > 0)
        {
            var first = heldByPlayer.OrderByDescending(x => x.Value.Health).First().Value;
            Console.Error.WriteLine($"Holding {first.Id} - {first.Health}pts");

            // @ Laboratory? We've done grunt work - research away!
            if (player.Target == Modules.LABORATORY)
            {
                _researched.Add(first.Id);
                return $"CONNECT {first.Id}";
            }

            if (!IsDiagnosed(first))
            {
                if (player.Target != Modules.DIAGNOSIS)
                    return Travel(player.Target, Modules.DIAGNOSIS);

                _diagnosed.Add(first.Id);
                return $"CONNECT {first.Id}";
            }
            else
            {
                if (player.Target != Modules.MOLECULES)
                    return Travel(player.Target, Modules.MOLECULES);

                // Connect and Types for Best Samples (up to max of 10);
                if (player.Storage.A < first.Cost.A) return "CONNECT A";
                if (player.Storage.B < first.Cost.B) return "CONNECT B";
                if (player.Storage.C < first.Cost.C) return "CONNECT C";
                if (player.Storage.D < first.Cost.D) return "CONNECT D";
                if (player.Storage.E < first.Cost.E) return "CONNECT E";

                // Got Molecules? Go to LABORATORY
                return Travel(player.Target, Modules.LABORATORY);
            }
        }

        // Shouldn't really get here.
        return "GOTO HELL";
    }

    private bool IsDiagnosed(SampleDataFile sample) => _diagnosed.Contains(sample.Id);
    private bool IsUndiagnosed(SampleDataFile sample) => !_diagnosed.Contains(sample.Id);

    private string Travel(string current, string destination)
    {
        // current <> destination distance (didn't want to dupe values in Dictionary).
        var distance = _distances.ContainsKey($"{current}>{destination}")
            ? _distances[$"{current}>{destination}"]
            : _distances[$"{destination}>{current}"];

        // Take 1 off Distance for Turn Used by Instruction
        for (int i = 0; i < distance - 1; i++)
            _queue.Enqueue(Actions.Wait);
        return Actions.Goto(destination);
    }
}

public class Player
{
    public string Target { get; private set; }
    public int ETA { get; private set; }
    public MoleculeCollection Storage { get; private set; }
    public MoleculeCollection Expertise { get; private set; }

    public Player(string input)
    {
        var inputs = input.Split(' ');
        Target = inputs[0];

        var ints = inputs.Skip(1).Select(int.Parse).ToArray();
        int eta = ints[0];
        int score = ints[1];
        Storage = new MoleculeCollection(ints.Skip(2).Take(5).ToArray());
        Expertise = new MoleculeCollection(ints.Skip(7).Take(5).ToArray());
    }

    public Player(string target, int eta, int score, MoleculeCollection storage, MoleculeCollection expertise)
    {
        Target = target;
        ETA = eta;
        Storage = storage;
        Expertise = expertise;
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

public static class Actions
{
    public const string Wait = "WAIT";
    public static string Connect(int idOrRank) => $"CONNECT {idOrRank}";
    public static string Goto(string destination) => $"GOTO {destination}";
}

public static class Goto
{
    private const string PREFIX = "GOTO ";
    public const string Samples = PREFIX + Modules.SAMPLES;
    public const string Diagnosis = PREFIX + Modules.DIAGNOSIS;
    public const string Molecules = PREFIX + Modules.MOLECULES;
    public const string Laboratory = PREFIX + Modules.LABORATORY;
}

public static class Modules
{
    public const string START_POS = "START_POS";
    public const string SAMPLES = "SAMPLES";
    public const string DIAGNOSIS = "DIAGNOSIS";
    public const string MOLECULES = "MOLECULES";
    public const string LABORATORY = "LABORATORY";
}

public static class States
{
    public const string UNDIAGNOSED = "undiagnosed";
    public const string DIAGNOSED = "diagnosed";
}