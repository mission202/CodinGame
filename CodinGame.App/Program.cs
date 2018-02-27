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
        var game = new Game(Console.ReadLine);
        while (true)
        {
            game.ProcessTurn();
            Console.Error.WriteLine("Game State:");
            Console.Error.WriteLine(game.Serialise);
            Console.WriteLine(game.GetNextAction());
        }
    }
}
public class Game
{

    public const int MAX_SAMPLES = 3;
    public const int MAX_MOLECULES = 10;

    private readonly Func<string> _inputSource;
    private MoleculeCollection[] _projects;
    private Player _player;
    private Player _opponent;
    private MoleculeCollection _available;
    private Dictionary<int, SampleDataFile> _samples = new Dictionary<int, SampleDataFile>();
    private readonly AI _ai = new AI();

    private string _lastUpdate = string.Empty;

    public Game(Func<string> inputSource)
    {
        _inputSource = inputSource;
        LoadProjects();
    }

    public Game(string state)
    {
        var inputAndAi = state.Split(new[] { "//" }, StringSplitOptions.None);
        _ai = new AI(inputAndAi[1]);
        var input = new Queue<string>(inputAndAi[0].Split('|'));
        _inputSource = input.Dequeue;
        LoadProjects();
        ProcessTurn();

    }

    public void ProcessTurn()
    {
        var player = _inputSource();
        var opponent = _inputSource();
        var molecules = _inputSource();
        var sampleCount = _inputSource();

        _player = new Player(player);
        _opponent = new Player(opponent);
        _available = new MoleculeCollection(molecules.Split(' ').Select(int.Parse).ToArray());
        var samples = Enumerable.Range(0, int.Parse(sampleCount)).Select(i => _inputSource()).ToList();
        samples.ForEach(s => UpdateSample(s));

        _lastUpdate = $"{player}|{opponent}|{molecules}|{sampleCount}|{string.Join("|", samples)}";
    }

    public string GetNextAction()
    {
        return _ai.GetNextAction(_projects, _samples, _player, _available);
    }

    private void LoadProjects()
    {
        int projectCount = int.Parse(_inputSource());
        _projects = new MoleculeCollection[projectCount];
        for (int i = 0; i < projectCount; i++)
        {
            var project = new MoleculeCollection(_inputSource().Split(' ').Select(int.Parse).ToArray());
            Console.Error.WriteLine($"Project: {project}");
            _projects[i] = project;
        }
    }

    private void UpdateSample(string input)
    {
        var data = new SampleDataFile(input);
        if (_samples.ContainsKey(data.Id))
            _samples[data.Id] = data;
        else
            _samples.Add(data.Id, data);
    }

    public string Serialise
    {
        get
        {
            var sb = new StringBuilder();
            sb.AppendLine(_projects.Count().ToString());
            foreach (var p in _projects)
                sb.AppendLine(p.ToString());
            return $"{string.Join("|", sb.ToString().Split(new[] {'\n'}, StringSplitOptions.RemoveEmptyEntries))}//{_lastUpdate}//{_ai.Serialise}";
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

    public AI()
    {
        // Default
    }

    public AI(string state)
    {
        var split = state.Split('|');
        Console.Error.WriteLine(string.Join(",", split));

        _diagnosed = string.IsNullOrWhiteSpace(split[0])
            ? new HashSet<int>()
            : new HashSet<int>(split[0].Split(',').Select(int.Parse));

        _researched = string.IsNullOrWhiteSpace(split[1])
            ? new HashSet<int>()
            : new HashSet<int>(split[1].Split(',').Select(int.Parse));

        _queue = string.IsNullOrWhiteSpace(split[2])
            ? new Queue<string>()
            : new Queue<string>(split[2].Split(','));
    }

    public string GetNextAction(MoleculeCollection[] projects, Dictionary<int, SampleDataFile> samples, Player player, MoleculeCollection available)
    {
        // Clear Queue (if any)
        if (_queue.Count > 0)
            return _queue.Dequeue();

        if (player.Target == Modules.START_POS)
            return Travel(player.Target, Modules.SAMPLES);

        // No Sample Data Files? Go to SAMPLES
        var heldByPlayer = samples.Where(x => x.Value.CarriedByPlayer && IsUnresearched(x)).ToList();

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
                .Where(x => IsUndiagnosed(x)).ToList();

            if (toDiagnose.Any())
            {
                toDiagnose.ForEach(sample =>
                {
                    _diagnosed.Add(sample.Value.Id);
                    _queue.Enqueue(Actions.Connect(sample.Value.Id));
                });

                return _queue.Dequeue();
            }

            return Travel(player.Target, Modules.MOLECULES);
        }

        if (player.Target == Modules.MOLECULES)
        {
            Console.Error.WriteLine("== SHOPPING! ==");
            foreach (var held in heldByPlayer)
                Console.Error.WriteLine($"// {held.Value.Id}: {held.Value.Cost} ({held.Value.Health}) Exp: {held.Value.Gain}");
            Console.Error.WriteLine($"// Storage: {player.Storage}");
            Console.Error.WriteLine($"// Expertise: {player.Expertise}");
            Console.Error.WriteLine($"// Available: {available}");

            var list = ShoppingList.Create(new ShoppingList.Parameters
            {
                Player = player,
                Available = available,
                Diagnosed = _diagnosed,
                Samples = samples.Values.ToList()
            });

            var shoppingFor = list.First();

            // Connect and Types for Best Samples (up to max of 10);
            if ((player.Storage.A + player.Expertise.A) < shoppingFor.Cost.A) return "CONNECT A";
            if ((player.Storage.B + player.Expertise.B) < shoppingFor.Cost.B) return "CONNECT B";
            if ((player.Storage.C + player.Expertise.C) < shoppingFor.Cost.C) return "CONNECT C";
            if ((player.Storage.D + player.Expertise.D) < shoppingFor.Cost.D) return "CONNECT D";
            if ((player.Storage.E + player.Expertise.E) < shoppingFor.Cost.E) return "CONNECT E";

            // Got Molecules? Go to LABORATORY
            return Travel(player.Target, Modules.LABORATORY);
        }

        if (player.Target == Modules.LABORATORY)
        {
            Console.Error.WriteLine("== RESEARCHING! ==");
            foreach (var held in heldByPlayer)
                Console.Error.WriteLine($"// {held.Value.Id}: {held.Value.Cost} ({held.Value.Health})");
            Console.Error.WriteLine($"// Storage: {player.Storage}");

            var researching = heldByPlayer
                .Where(x => IsUnresearched(x))
                .Where(x => player.Storage.Covers(x.Value.Cost, player))
                .OrderByDescending(x => x.Value.Health)
                .ToList();

            if (researching.Count == 0)
            {
                return (heldByPlayer.Count == 0)
                    ? Travel(player.Target, Modules.SAMPLES)
                    : Travel(player.Target, Modules.MOLECULES);
            }

            _researched.Add(researching.First().Value.Id);
            return $"CONNECT {researching.First().Value.Id}";
        }

        // Shouldn't really get here.
        return "GOTO HELL";
    }

    private bool IsDiagnosed(KeyValuePair<int, SampleDataFile> sample) => _diagnosed.Contains(sample.Value.Id);
    private bool IsUndiagnosed(KeyValuePair<int, SampleDataFile> sample) => !_diagnosed.Contains(sample.Value.Id);
    private bool IsResearched(KeyValuePair<int, SampleDataFile> sample) => _researched.Contains(sample.Value.Id);
    private bool IsUnresearched(KeyValuePair<int, SampleDataFile> sample) => !_researched.Contains(sample.Value.Id);

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

    public string Serialise
    {
        get
        {
            return $"{string.Join(",", _diagnosed)}|{string.Join(",", _researched)}|{string.Join(",", _queue)}";
        }
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

    public MoleculeCollection(int a, int b, int c, int d, int e)
    {
        A = a;
        B = b;
        C = c;
        D = d;
        E = e;
    }

    public MoleculeCollection(int[] values)
    {
        A = values[0];
        B = values[1];
        C = values[2];
        D = values[3];
        E = values[4];
    }

    public override string ToString() => $"{new String('A', A)}{new String('B', B)}{new String('C', C)}{new String('D', D)}{new String('E', E)}";

    public bool Covers(MoleculeCollection cost, Player player)
    {
        Console.Error.WriteLine($"Checking {cost} against {this}");
        return (
            (A + player.Expertise.A) >= (cost.A - player.Storage.A) &&
            (B + player.Expertise.B) >= (cost.B - player.Storage.B) &&
            (C + player.Expertise.C) >= (cost.C - player.Storage.C) &&
            (D + player.Expertise.D) >= (cost.D - player.Storage.D) &&
            (E + player.Expertise.E) >= (cost.E - player.Storage.E)
        );
    }
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

public class ShoppingList
{
    public class Parameters
    {
        public Player Player { get; set; }
        public MoleculeCollection Available { get; set; } = new MoleculeCollection(0, 0, 0, 0, 0);
        public List<SampleDataFile> Samples { get; set; } = new List<SampleDataFile>();
        public HashSet<int> Diagnosed { get; set; } = new HashSet<int>();
    }

    public static IEnumerable<SampleDataFile> Create(Parameters p)
    {
        var heldByPlayer = p.Samples
            .Where(x => x.CarriedByPlayer)
            .Where(x => p.Diagnosed.Contains(x.Id))
            .Where(x => p.Available.Covers(x.Cost, p.Player))
            .OrderByDescending(x => x.Health);

        return heldByPlayer;
    }
}

public static class Actions
{
    public const string Wait = "WAIT";
    public static string Connect(int idOrRank) => $"CONNECT {idOrRank}";
    public static string Connect(char molecule) => $"CONNECT {molecule}";
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