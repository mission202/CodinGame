using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

/**
 * Made with love by AntiSquid, Illedan and Wildum.
 * You can help children learn to code while you participate by donating to CoderDojo.
 **/
class Player
{
    static void Main(string[] args)
    {
        var game = new Game();
        while (true)
        {
            // If roundType has a negative value then you need to output a Hero name, such as "DEADPOOL" or "VALKYRIE".
            // Else you need to output roundType number of any valid action, such as "WAIT" or "ATTACK unitId"
            Console.WriteLine(game.Move());

            Console.Error.WriteLine("Game State:");
            Console.Error.WriteLine(game.Serialise());
        }
    }
}

/* IDEAS
 * - Hold the Line - Support Units @ Front
 *  - Prioritise attacking to nearest Hero, Melee and Ranged
 * - Use Aggro - Stay near (<300 distance) to allied units.
 * - Turtle - Stay near the tower?
 * - Fundraising - Slaughter weak units? (30 melee, 50 ranged, 300 hero)
 * - Ranged - Avoid Units and attack at range.
 */

public class GameState
{
    public int MyTeam { get; private set; }
    public int BushAndSpawnPointCount { get; private set; }

    public int RoundType { get; private set; }
    public bool IsHeroPickRound => RoundType < 0;

    public int PlayerGold { get; private set; }
    public int EnemyGold { get; private set; }

    protected Func<string> _inputSource = Console.ReadLine;

    private readonly Queue<string> _init = new Queue<string>();
    private readonly Queue<string> _turn = new Queue<string>();

    public void Init()
    {
        Func<string> read = () =>
        {
            var s = _inputSource();
            _init.Enqueue(s);
            return s;
        };

        string input;
        string[] inputs;
        MyTeam = int.Parse(read());

        BushAndSpawnPointCount = int.Parse(read()); // usefrul from wood1, represents the number of bushes and the number of places where neutral units can spawn
        for (int i = 0; i < BushAndSpawnPointCount; i++)
        {
            input = read();
            Console.Error.WriteLine(input);
            inputs = input.Split(' ');

            // TODO: Store Entities
            string entityType = inputs[0]; // BUSH, from wood1 it can also be SPAWN
            int x = int.Parse(inputs[1]);
            int y = int.Parse(inputs[2]);
            int radius = int.Parse(inputs[3]);
        }
        input = read();

        int itemCount = int.Parse(input); // useful from wood2
        //TODO: Store Items
        for (int i = 0; i < itemCount; i++)
        {
            input = _inputSource();
            Console.Error.WriteLine(input);
            inputs = input.Split(' ');
            string itemName = inputs[0]; // contains keywords such as BRONZE, SILVER and BLADE, BOOTS connected by "_" to help you sort easier
            int itemCost = int.Parse(inputs[1]); // BRONZE items have lowest cost, the most expensive items are LEGENDARY
            int damage = int.Parse(inputs[2]); // keyword BLADE is present if the most important item stat is damage
            int health = int.Parse(inputs[3]);
            int maxHealth = int.Parse(inputs[4]);
            int mana = int.Parse(inputs[5]);
            int maxMana = int.Parse(inputs[6]);
            int moveSpeed = int.Parse(inputs[7]); // keyword BOOTS is present if the most important item stat is moveSpeed
            int manaRegeneration = int.Parse(inputs[8]);
            int isPotion = int.Parse(inputs[9]); // 0 if it's not instantly consumed
        }
    }

    public void Turn()
    {
        _turn.Clear();

        Func<string> read = () =>
        {
            var s = _inputSource();
            _turn.Enqueue(s);
            return s;
        };

        string input;
        string[] inputs;

        input = read();
        PlayerGold = int.Parse(input);

        input = read();
        EnemyGold = int.Parse(input);

        input = read();
        RoundType = int.Parse(input); // a positive value will show the number of heroes that await a command

        input = read();
        int entityCount = int.Parse(input);
        for (int i = 0; i < entityCount; i++)
        {
            input = read();
            inputs = input.Split(' ');
            int unitId = int.Parse(inputs[0]);
            int team = int.Parse(inputs[1]);
            string unitType = inputs[2]; // UNIT, HERO, TOWER, can also be GROOT from wood1
            int x = int.Parse(inputs[3]);
            int y = int.Parse(inputs[4]);
            int attackRange = int.Parse(inputs[5]);
            int health = int.Parse(inputs[6]);
            int maxHealth = int.Parse(inputs[7]);
            int shield = int.Parse(inputs[8]); // useful in bronze
            int attackDamage = int.Parse(inputs[9]);
            int movementSpeed = int.Parse(inputs[10]);
            int stunDuration = int.Parse(inputs[11]); // useful in bronze
            int goldValue = int.Parse(inputs[12]);
            int countDown1 = int.Parse(inputs[13]); // all countDown and mana variables are useful starting in bronze
            int countDown2 = int.Parse(inputs[14]);
            int countDown3 = int.Parse(inputs[15]);
            int mana = int.Parse(inputs[16]);
            int maxMana = int.Parse(inputs[17]);
            int manaRegeneration = int.Parse(inputs[18]);
            string heroType = inputs[19]; // DEADPOOL, VALKYRIE, DOCTOR_STRANGE, HULK, IRONMAN
            int isVisible = int.Parse(inputs[20]); // 0 if it isn't
            int itemsOwned = int.Parse(inputs[21]); // useful from wood1
        }
    }

    public string Serialise()
    {
        var allInputs = new List<string>();
        allInputs.AddRange(_init);
        allInputs.AddRange(_turn);
        return $"{string.Join(Environment.NewLine, allInputs)}";
    }
}

public class Game
{
    private readonly GameState _gs;

    public Game(GameState gs = null)
    {
        _gs = gs ?? new GameState();
        _gs.Init();
    }

    // TODO: Get Next Move

    public string Move()
    {
        _gs.Turn();

        if (_gs.IsHeroPickRound)
            return Actions.Heroes.Valkyrie;

        return Actions.Wait.WithMessage("Picking Random Player");
    }

    public string Serialise()
    {
        return $"{_gs.Serialise()}";
    }
}

public static class Actions
{
    public const string Wait = "WAIT";

    public static class Heroes
    {
        public const string Deadpool = "DEADPOOL";
        public const string DoctorStrange = "DOCTOR_STRANGE";
        public const string Hulk = "HULK";
        public const string IronMan = "IRONMAN";
        public const string Valkyrie = "VALKYRIE";
    }

    public static string WithMessage(this string action, string message) => $"{action};{message}";
}