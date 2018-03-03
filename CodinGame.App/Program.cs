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
            try
            {
                var moves = game.Moves();
                foreach (var move in moves)
                    Console.WriteLine(move);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("== EXCEPTION! ==");
                Console.Error.WriteLine(ex.ToString());
            }
            finally
            {
                Console.Error.WriteLine("Game State:");
                Console.Error.WriteLine(game.Serialise());
            }
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
    public List<Entity> Entities { get; } = new List<Entity>();
    public List<Item> Items { get; } = new List<Item>();

    private int _carrying = 0;
    public int ItemsSlotsAvailable => Consts.MAX_ITEMS - _carrying;

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
        for (int i = 0; i < itemCount; i++)
        {
            input = _inputSource();
            Items.Add(new Item(input));
            Console.Error.WriteLine(input);
        }
    }

    public void Turn()
    {
        _turn.Clear();
        Entities.Clear(); // TODO: Yuck, can't I just update?

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

            Entity entity;

            switch (unitType)
            {
                case Units.HERO:
                    entity = new Hero(unitId, team, x, y, attackRange, health, maxHealth, shield, attackDamage, movementSpeed, stunDuration, goldValue,
                        new Hero.Attributes(countDown1, countDown2, countDown3, mana, maxMana, manaRegeneration, heroType, isVisible, itemsOwned));
                    break;
                default:
                    entity = new Entity(unitId, team, unitType, x, y, attackRange, health, maxHealth, shield, attackDamage, movementSpeed, stunDuration, goldValue);
                    break;
            }

            Entities.Add(entity);
        }
    }

    public string Serialise()
    {
        var allInputs = new List<string>();
        allInputs.AddRange(_init);
        allInputs.AddRange(_turn);
        return $"{string.Join(Environment.NewLine, allInputs)}";
    }

    public string Buy(Item item)
    {
        _carrying++;
        PlayerGold -= item.Cost;
        return Actions.Buy(item.Name).WithMessage("Retail Therapy");
    }
}

public class Game
{
    private readonly GameState _gs;

    private HeroStats[] _myHeroes;
    private Queue<string> _toPick;

    public Game(GameState gs = null)
    {
        _gs = gs ?? new GameState();
        _gs.Init();

        _myHeroes = new[] { Heroes.Hulk, Heroes.Ironman };
        _toPick = new Queue<string>(_myHeroes.Select(x => x.Name));
    }

    public string[] Moves()
    {
        _gs.Turn();

        // TODO: Create Basic "AI" for 2x Heroes

        // TODO: Heroes should be in GS.
        if (_gs.IsHeroPickRound)
            return new[] { _toPick.Dequeue() };

        var response = new List<string>();

        foreach (var controlledHero in _myHeroes)
        {
            var shopping = ItemWorthBuying();
            if (shopping != null)
            {
                response.Add(_gs.Buy(shopping));
                continue;
            }

            var attackRange = controlledHero.Range;

            var friendly = _gs.Entities.Where(x => x.Team == _gs.MyTeam).ToList();
            var enemy = _gs.Entities.Where(x => x.Team != _gs.MyTeam).ToList();

            var hero = friendly.OfType<Hero>().Where(x => x.Attribs.HeroType == controlledHero.Name).Single();

            if (hero.Health < (hero.MaxHealth * 0.1))
            {
                response.Add(new HealingUp(hero, _gs).Move());
                continue;
            }

            var friendlyUnits = friendly
                .Where(x => x.UnitType == Units.UNIT)
                .ToList();

            if (!friendlyUnits.Any())
            {
                var tower = friendly.Where(x => x.UnitType == Units.TOWER).Single();
                response.Add(Actions.Move(tower.X, tower.Y).WithMessage("Retreat!"));
                continue;
            }

            const int scanRange = 2;
            var enemyFront = _gs.MyTeam == 0 ? enemy.Min(x => x.X) : enemy.Max(x => x.X);
            var rabble = (int)friendlyUnits.Average(x => x.X);

            var drawingClose = _gs.MyTeam == 0
                ? enemyFront < (rabble + (attackRange * scanRange))
                : enemyFront > (rabble - (attackRange * scanRange));

            if (drawingClose)
            {
                var target = enemy
                    .Where(x => x.X == enemyFront)
                    .OrderByDescending(x => x.GoldValue).First();
                response.Add(Actions.AttackNearest(target.UnitType).WithMessage($"Target : {target.UnitType}"));
                continue;
            }

            // Stick Close to Troops
            var verticals = friendlyUnits.Select(x => x.Y).OrderBy(x => x).ToArray();
            var middle = verticals.First() + ((verticals.Last() - verticals.First()) / 2);
            var xPos = _gs.MyTeam == 0 ? rabble + (attackRange / 2) : rabble + (attackRange / 2);
            response.Add(Actions.Move(xPos, middle).WithMessage("HOLD THE LINE"));
            continue;
        }

        return response.ToArray();
    }

    public string Serialise()
    {
        return $"{_gs.Serialise()}";
    }

    private Item ItemWorthBuying()
    {
        if (_gs.ItemsSlotsAvailable <= 0) return null;

        return _gs
            .Items
            .Affordable(_gs.PlayerGold)
            .OrderByDescending(x => x.Damage)
            .FirstOrDefault();
    }
}

public static class Extensions
{
    public static IEnumerable<Item> Affordable(this IEnumerable<Item> items, int gold)
    {
        return items.Where(x => x.Cost <= gold);
    }
}

public abstract class AI
{
    protected readonly Hero Hero;
    protected readonly GameState State;

    public abstract string Move();

    protected AI(Hero hero, GameState state)
    {
        Hero = hero;
        State = state;
    }
}

public class HealingUp : AI
{
    public HealingUp(Hero hero, GameState state) : base(hero, state)
    {

    }

    public override string Move()
    {
        var need = Hero.MaxHealth - Hero.Health;
        if (Hero.Attribs.ItemsOwned < Consts.MAX_ITEMS)
        {
            var potion = State.Items
                .Affordable(State.PlayerGold)
                .Where(x => x.Health > 0 && x.Health < need)
                .OrderByDescending(x => x.Health)
                .FirstOrDefault();

            if (potion != null)
                return Actions.Buy(potion).WithMessage("YOU ARE HEALLLED!");
        }

        // TODO: Sell Item?

        var friendly = State.Entities.Where(x => x.Team == State.MyTeam).ToList();
        var tower = friendly.Where(x => x.UnitType == Units.TOWER).Single();
        return Actions.Move(tower.X, tower.Y).WithMessage("Licking Wounds");
    }
}

public class Entity
{
    public int UnitId { get; private set; }
    public int Team { get; private set; }
    public string UnitType { get; private set; }
    public int X { get; private set; }
    public int Y { get; private set; }
    public int AttackRange { get; private set; }
    public int Health { get; private set; }
    public int MaxHealth { get; private set; }
    public int Shield { get; private set; }
    public int AttackDamage { get; private set; }
    public int MovementSpeed { get; private set; }
    public int StunDuration { get; private set; }
    public int GoldValue { get; private set; }

    public Entity(int unitId, int team, string unitType, int x, int y, int attackRange, int health, int maxHealth, int shield, int attackDamage, int movementSpeed, int stunDuration, int goldValue)
    {
        UnitId = unitId;
        Team = team;
        UnitType = unitType;
        X = x;
        Y = y;
        AttackRange = attackRange;
        Health = health;
        MaxHealth = maxHealth;
        Shield = shield;
        AttackDamage = attackDamage;
        MovementSpeed = movementSpeed;
        StunDuration = stunDuration;
        GoldValue = goldValue;
    }
}

public class Hero : Entity
{
    public Attributes Attribs { get; private set; }

    public Hero(int unitId, int team, int x, int y, int attackRange, int health, int maxHealth, int shield, int attackDamage, int movementSpeed, int stunDuration, int goldValue, Attributes heroAttribs)
        : base(unitId, team, Units.HERO, x, y, attackRange, health, maxHealth, shield, attackRange, movementSpeed, stunDuration, goldValue)
    {
        Attribs = heroAttribs;
    }

    public class Attributes
    {
        public int Countdown1 { get; private set; }
        public int Countdown2 { get; private set; }
        public int Countdown3 { get; private set; }
        public int Mana { get; private set; }
        public int MaxMana { get; private set; }
        public int ManaRegeneration { get; private set; }
        public string HeroType { get; private set; }
        public int IsVisible { get; private set; }
        public int ItemsOwned { get; private set; }

        public Attributes(int countDown1, int countDown2, int countDown3, int mana, int maxMana, int manaRegeneration, string heroType, int isVisible, int itemsOwned)
        {
            Countdown1 = countDown1;
            Countdown2 = countDown2;
            Countdown3 = countDown3;
            Mana = mana;
            MaxMana = maxMana;
            ManaRegeneration = manaRegeneration;
            HeroType = heroType;
            IsVisible = isVisible;
            ItemsOwned = itemsOwned;
        }
    }
}

public class Item
{
    public string Name { get; private set; } // BRONZE>LEGENDARY BLADE=DMG BOOTS=SPEED
    public int Cost { get; private set; }
    public int Damage { get; private set; }
    public int Health { get; private set; }
    public int MaxHealth { get; private set; }
    public int Mana { get; private set; }
    public int MaxMana { get; private set; }
    public int MoveSpeed { get; private set; }
    public int ManaRegeneration { get; private set; }
    public int IsPotion { get; private set; }
    public bool IsInstant => IsPotion != 0;

    public Item(string input)
    {
        var inputs = input.Split(' ');
        Name = inputs[0]; // contains keywords such as BRONZE, SILVER and BLADE, BOOTS connected by "_" to help you sort easier
        Cost = int.Parse(inputs[1]); // BRONZE items have lowest cost, the most expensive items are LEGENDARY
        Damage = int.Parse(inputs[2]); // keyword BLADE is present if the most important item stat is damage
        Health = int.Parse(inputs[3]);
        MaxHealth = int.Parse(inputs[4]);
        Mana = int.Parse(inputs[5]);
        MaxMana = int.Parse(inputs[6]);
        MoveSpeed = int.Parse(inputs[7]); // keyword BOOTS is present if the most important item stat is moveSpeed
        ManaRegeneration = int.Parse(inputs[8]);
        IsPotion = int.Parse(inputs[9]); // 0 if it's not instantly consumed
    }
}

public static class Actions
{
    public const string Wait = "WAIT";

    public static string Move(int x, int y) => $"MOVE {x} {y}";
    public static string MoveAttack(Entity entity) => MoveAttack(entity.X, entity.Y, entity.UnitId);
    public static string MoveAttack(int x, int y, int id) => $"MOVE_ATTACK {x} {y} {id}";
    public static string AttackNearest(string unit) => $"ATTACK_NEAREST {unit}";
    public static string Buy(string item) => $"BUY {item}";
    public static string Buy(Item item) => Buy(item.Name);
    public static string Sell(string item) => $"SELL {item}";
    public static string WithMessage(this string action, string message) => $"{action};{message}";
}

public static class Heroes
{
    public const string Deadpool = "DEADPOOL";
    public const string DoctorStrange = "DOCTOR_STRANGE";
    public const string Valkyrie = "VALKYRIE";

    private static readonly Dictionary<string, HeroStats> _heroes = new Dictionary<string, HeroStats>
    {
        // TODO: Deadpool
        // TODO: Dr Strange
        { "HULK", new HeroStats("HULK", 95) },
        { "IRONMAN", new HeroStats("IRONMAN", 270) },
        // TODO: Valkyrie
    };

    public static HeroStats Hulk => _heroes["HULK"];
    public static HeroStats Ironman => _heroes["IRONMAN"];
}

public class HeroStats
{
    public string Name { get; private set; }
    public int Range { get; private set; }

    public HeroStats(string name, int range)
    {
        Name = name;
        Range = range;
    }
}

public static class Units
{
    // UNIT, HERO, TOWER, can also be GROOT from wood1
    public const string UNIT = "UNIT";
    public const string HERO = "HERO";
    public const string TOWER = "TOWER";
    public const string GROOT = "GROOT";
}

public static class Consts
{
    public const int MAX_ITEMS = 4;
}