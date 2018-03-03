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
                D.WL("== EXCEPTION! ==");
                D.WL(ex.ToString());
            }
            finally
            {
                if (false)
                {
                    D.WL("Game State:");
                    D.WL(game.Serialise());
                }
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
    private readonly Dictionary<string, string> _stateBag = new Dictionary<string, string>();

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

    public void SetState(string key, string value)
    {
        if (_stateBag.ContainsKey(key))
        {
            _stateBag[key] = value;
        }
        else
        {
            _stateBag.Add(key, value);
        }
    }

    public string GetState(string key)
    {
        return _stateBag.ContainsKey(key)
            ? _stateBag[key]
            : string.Empty;
    }
}

public class Game
{
    private readonly GameState _gs;

    private Queue<string> _toPick;
    private Dictionary<string, List<StrategicMove>> _strategies = new Dictionary<string, List<StrategicMove>>();

    public Game(GameState gs = null)
    {
        _gs = gs ?? new GameState();
        _gs.Init();

        var hulk = new List<StrategicMove>
        {
            new StayAlive(),
            new Tanking()
        };

        var ironMan = new List<StrategicMove>
        {
            new StayAlive(scaredyCat: true)
        };

        _strategies.Add("HULK", hulk);
        _strategies.Add("IRONMAN", ironMan);

        _toPick = new Queue<string>(_strategies.Keys);

        // TODO: Store IDs to Common Entities (e.g. Heroes, Towers etc.)
    }

    public string[] Moves()
    {
        _gs.Turn();

        // TODO: Heroes should be in GS.
        if (_gs.IsHeroPickRound)
            return new[] { _toPick.Dequeue() };

        var response = new List<string>();

        foreach (var controlledHero in _strategies.Keys)
        {
            // TODO: Move Shopping to StrategicMove
            var shopping = ItemWorthBuying();
            if (shopping != null)
            {
                response.Add(_gs.Buy(shopping));
                continue;
            }

            var friendly = _gs.Entities.Where(x => x.Team == _gs.MyTeam).ToList();
            var hero = friendly.OfType<Hero>().Where(x => x.Attribs.HeroType == controlledHero).SingleOrDefault();
            if (hero == null)
            {
                D.WL($"RIP {controlledHero} :'(");
                continue;
            }

            string move = "";
            foreach (var strategy in _strategies[controlledHero])
            {
                move = strategy.Move(hero, _gs);
                if (!string.IsNullOrWhiteSpace(move))
                {
                    response.Add(move);
                    break;
                }
            }

            if (string.IsNullOrWhiteSpace(move))
                response.Add(new DefaultAction().Move(hero, _gs));
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
            .Where(x => !x.IsInstant)
            .Affordable(_gs.PlayerGold)
            .OrderByDescending(x => x.Damage)
            .FirstOrDefault();
    }
}

public class DefaultAction : StrategicMove
{
    public override string Move(Hero hero, GameState state)
    {
        var attackRange = hero.AttackRange;
        var friendly = state.Entities.Where(x => x.Team == state.MyTeam).ToList();

        var friendlyUnits = friendly
                .Where(x => x.UnitType == Units.UNIT)
                .ToList();

        if (!friendlyUnits.Any())
            return new Retreat().Move(hero, state);

        var enemy = state.Entities
                .Where(x => x.Team != state.MyTeam)
                .Where(x => x.UnitType != Units.GROOT)
                .ToList();

        const int scanRange = 2;
        var enemyFront = state.MyTeam == 0 ? enemy.Min(x => x.X) : enemy.Max(x => x.X);
        var rabble = (int)friendlyUnits.Average(x => x.X);

        var drawingClose = state.MyTeam == 0
            ? enemyFront < (rabble + (attackRange * scanRange))
            : enemyFront > (rabble - (attackRange * scanRange));

        if (drawingClose)
        {
            var target = enemy
                .Where(x => x.X == enemyFront)
                .OrderByDescending(x => x.AttackDamage).First();
            return Actions.AttackNearest(target.UnitType).WithMessage($"Target : {target.UnitType}");
        }

        // Stick Close to Troops
        var verticals = friendlyUnits.Select(x => x.Y).OrderBy(x => x).ToArray();
        var middle = verticals.First() + ((verticals.Last() - verticals.First()) / 2);
        var xPos = state.MyTeam == 0 ? rabble + (attackRange / 2) : rabble + (attackRange / 2);
        return Actions.Move(xPos, middle).WithMessage("HOLD THE LINE");
    }
}

public class StayAlive : StrategicMove
{
    private readonly bool _isPussy;

    public StayAlive(bool scaredyCat = false)
    {
        _isPussy = scaredyCat;
    }

    public override string Move(Hero hero, GameState state)
    {
        var threat = state.Entities
            .Where(x => !x.IsNeutral)
            .Where(x => x.Team != state.MyTeam)
            .Where(x => x.Distance(hero) <= x.AttackRange)
            .Sum(x => x.AttackDamage);

        var fear = _isPussy
            ? threat * 2
            : threat * 1.1;

        var safe = hero.Health > fear;

        D.WL($"(StayAlive): {hero.Attribs.HeroType} Pussy? {_isPussy} Health: {hero.Health} - Threat: {threat} Safe? {safe}");

        if (safe) return string.Empty;

        if (hero.Attribs.ItemsOwned < Consts.MAX_ITEMS)
        {
            var need = hero.MaxHealth - hero.Health;
            var potion = state.Items
                .Affordable(state.PlayerGold)
                .Where(x => x.Health > 0 && x.Health < need)
                .OrderByDescending(x => x.Health)
                .FirstOrDefault();

            if (potion != null)
                return Actions.Buy(potion).WithMessage("Healed Via Potion!");
        }

        // TODO: Sell Item to make space?
        // TODO: Consider attacking at range?

        var tower = state.Entities.Where(x => x.Team == state.MyTeam).Where(x => x.UnitType == Units.TOWER).Single();
        return Actions.Move(tower.X, tower.Y).WithMessage("Out of Game");
    }
}

public class Tanking : StrategicMove
{
    public override string Move(Hero hero, GameState state)
    {
        var byDistance = state.Entities
            .Where(x => !x.IsNeutral)
            .Where(x => x.Team != state.MyTeam)
            .OrderBy(x => x.Distance(hero));

        var closest = byDistance.FirstOrDefault();

        // Charge!
        if (closest == null || closest.Distance(hero) > 500) return string.Empty;

        if (closest.Distance(hero) < hero.AttackRange)
        {
            var threatCount = byDistance.Where(x => x.Distance(hero) < x.AttackRange).Count();

            if (threatCount > 2)
            {
                var shieldState = int.TryParse(state.GetState("HULK_SHIELD"), out var shieldCD);
                if (shieldState)
                {
                    if (shieldCD > 1)
                    {
                        state.SetState("HULK_SHIELD", (--shieldCD).ToString());
                        return Actions.MoveAttack(closest);
                    }
                }

                state.SetState("HULK_SHIELD", "8");
                return $"EXPLOSIVESHIELD";
            }

            return Actions.MoveAttack(closest);
        }

        var inState = int.TryParse(state.GetState("HULK_CHARGE"), out var coolDown);
        if (inState)
        {
            if (coolDown > 1)
            {
                state.SetState("HULK_CHARGE", (--coolDown).ToString());
                return Actions.MoveAttack(closest).WithMessage($"HULK COOLING {coolDown}");
            }
        }

        state.SetState("HULK_CHARGE", "8");
        return $"CHARGE {closest.UnitId}".WithMessage($"HULK GET {closest.UnitType} {closest.UnitId}!");
    }
}

public class Retreat : StrategicMove
{
    public override string Move(Hero hero, GameState state)
    {
        var friendly = state.Entities.Where(x => x.Team == state.MyTeam).ToList();
        var tower = friendly.Where(x => x.UnitType == Units.TOWER).Single();
        return Actions.Move(tower.X, tower.Y).WithMessage("Retreat!");
    }
}

#region Domain Objects

public abstract class StrategicMove
{
    public abstract string Move(Hero hero, GameState state);
}

public class Entity
{
    public int UnitId { get; private set; }
    public int Team { get; private set; }
    public bool IsNeutral => Team == -1;
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

#endregion

public static class Extensions
{
    public static IEnumerable<Item> Affordable(this IEnumerable<Item> items, int gold)
    {
        return items.Where(x => x.Cost <= gold);
    }

    public static int Distance(this Entity a, Entity b)
    {
        return (int)Math.Sqrt(Math.Pow(a.X - b.X, 2) + Math.Pow(b.Y - a.Y, 2));
    }
}

#region Consts / Helpers
public static class D
{
    public static void WL(string message) => Console.Error.WriteLine(message);
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

public static class Consts
{
    public const int MAX_ITEMS = 4;
}

#endregion