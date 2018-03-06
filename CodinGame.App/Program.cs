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

public class ScoreCriteria
{
    public int PlayerHeroHealth { get; private set; }
    public int EnemyHeroHealth { get; private set; }
    public int PlayerHealth { get; private set; }
    public int EnemyHealth { get; private set; }
    public int PlayerPower { get; private set; }
    public int EnemyPower { get; private set; }
    public int PlayerUnits { get; private set; }
    public int EnemyUnits { get; private set; }
    public int PlayerThreat { get; private set; }
    public int EnemyThreat { get; private set; }
    public int PlayerGold { get; private set; }
    public int EnemyGold { get; private set; }

    public ScoreCriteria(GameState state)
    {
        var playerUnits = state.Entities.Where(x => x.Team == state.MyTeam).ToList();
        var enemyUnits = state.Entities.Where(x => x.Team != state.MyTeam && !x.IsNeutral).ToList();

        PlayerHeroHealth = playerUnits.OfType<Hero>().Sum(x => x.Health);
        EnemyHeroHealth = enemyUnits.OfType<Hero>().Sum(x => x.Health);

        PlayerHealth = playerUnits.Sum(x => x.Health);
        EnemyHealth = enemyUnits.Sum(x => x.Health);

        PlayerPower = playerUnits.Sum(x => x.AttackDamage);
        EnemyPower = enemyUnits.Sum(x => x.AttackDamage);

        PlayerUnits = playerUnits.Count();
        EnemyUnits = enemyUnits.Count();

        PlayerThreat = playerUnits.Where(x => enemyUnits.Any(y => y.Distance(x) < x.AttackRange)).Sum(x => x.AttackDamage);
        EnemyThreat = enemyUnits.Where(x => playerUnits.Any(y => y.Distance(x) < x.AttackRange)).Sum(x => x.AttackDamage);

        PlayerGold = state.PlayerGold;
        EnemyGold = state.EnemyGold;
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.AppendLine("Score Criteria:");
        sb.AppendLine($"* Hero Health: {PlayerHeroHealth} vs. {EnemyHeroHealth}");
        sb.AppendLine($"* Health: {PlayerHealth} vs. {EnemyHealth}");
        sb.AppendLine($"* Power: {PlayerPower} vs. {EnemyPower}");
        sb.AppendLine($"* Units: {PlayerUnits} vs. {EnemyUnits}");
        sb.AppendLine($"* Threat: {PlayerThreat} vs. {EnemyThreat}");
        sb.AppendLine($"* Gold: {PlayerGold} vs. {EnemyGold}");
        return sb.ToString();
    }
}

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
    public List<Coordinate> Bushes { get; } = new List<Coordinate>();

    public CommonEntities Common { get; private set; }

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
            inputs = input.Split(' ');

            // TODO: Store Entities
            string entityType = inputs[0]; // BUSH, from wood1 it can also be SPAWN
            int x = int.Parse(inputs[1]);
            int y = int.Parse(inputs[2]);
            int radius = int.Parse(inputs[3]);

            if (entityType == Units.BUSH)
            {
                Bushes.Add(new Coordinate(x, y));
            }
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

        Common = new CommonEntities(this, Entities);
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

    private Queue<string> _toPick;
    private Dictionary<string, List<StrategicMove>> _strategies = new Dictionary<string, List<StrategicMove>>();

    public Game(GameState gs = null)
    {
        _gs = gs ?? new GameState();
        _gs.Init();

        var htl = new HoldTheLine();
        var dk = new DenyKills();

        var drStrange = new List<StrategicMove>
        {
            new StayAlive(),
            dk,
            new AoEHealSkill(),
            new ShieldSkill(),
            new PullSkill(),
            htl,
        };

        var ironMan = new List<StrategicMove>
        {
            new StayAlive(scaredyCat: true),
            dk,
            new BlinkSkill(),
            new FireballSkill(),
            new BurningSkill(),
            htl,
            new RangedFighter()
        };

        _strategies.Add(Heroes.DoctorStrange.Name, drStrange);
        _strategies.Add(Heroes.Ironman.Name, ironMan);

        _toPick = new Queue<string>(_strategies.Keys);
    }

    public string[] Moves()
    {
        _gs.Turn();

        var score = new ScoreCriteria(_gs);
        //Console.Error.WriteLine(score);

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
        if (_gs.ItemsSlotsAvailable <= 1) return null; // Keep 1 Slot for Potions

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

#region HULK Skills
public class ChargeSkill : UnitTargetedSkillMove
{
    public ChargeSkill() : base("HULK", "CHARGE", 20, 4) { }

    protected override bool ShouldUse(Hero hero, GameState state)
    {
        var heroInRange = state.Entities
            .OfType<Hero>()
            .Where(x => !x.IsNeutral)
            .Where(x => x.Team != state.MyTeam)
            .OrderByDescending(x => x.MovementSpeed) // Want to attack fastest unit due to movement speed debuff
            .Where(x => x.Distance(hero) <= 500)
            .FirstOrDefault();

        if (heroInRange == null) return false;

        TargetId = heroInRange.UnitId;
        return true;
    }
}

public class ExplosiveShieldSkill : SkillMove
{
    public ExplosiveShieldSkill() : base("HULK", "EXPLOSIVESHIELD", 30, 8) { }

    protected override bool ShouldUse(Hero hero, GameState state)
    {
        var threat = state.Entities
            .Where(x => !x.IsNeutral)
            .Where(x => x.Team != state.MyTeam)
            .Where(x => x.Distance(hero) <= x.AttackRange)
            .Sum(x => x.AttackDamage);

        return threat > hero.MaxHealth * 0.2;
    }
}

public class BashSkill : UnitTargetedSkillMove
{
    public BashSkill() : base("HULK", "BASH", 40, 8) { }

    protected override bool ShouldUse(Hero hero, GameState state)
    {
        var heroInRange = state.Entities
            .OfType<Hero>()
            .Where(x => !x.IsNeutral)
            .Where(x => x.Team != state.MyTeam)
            .Where(x => x.Distance(hero) <= 140)
            .FirstOrDefault();

        if (heroInRange == null) return false;

        D.WL($"Targeted {heroInRange.Attribs.HeroType} ({heroInRange.UnitId}) @ {heroInRange.Distance(hero)}");

        TargetId = heroInRange.UnitId;
        return true;
    }
}
#endregion

#region IRONMAN Skills
public class FireballSkill : CoordinateTargetedSkillMove
{
    public FireballSkill() : base("IRONMAN", "FIREBALL", 60, 6) { }

    protected override bool ShouldUse(Hero hero, GameState state)
    {
        var heroInRange = state.Common.EnemyHeroes
            .Where(x => x.Distance(hero) <= 500) // Only Fire at Range for DMG
            .Where(x => x.Distance(hero) <= 900)
            .FirstOrDefault();

        if (heroInRange == null) return false;

        // Splash Damage - 50px Radius
        var nearby = state.Common.Enemies
            .Where(x => x.Distance(heroInRange) <= 50)
            .Count();

        if (nearby < 2) return false;

        Target = new Coordinate(heroInRange.X, heroInRange.Y);
        return true;
    }
}

public class BurningSkill : CoordinateTargetedSkillMove
{
    public BurningSkill() : base("IRONMAN", "BURNING", 50, 5) { }

    protected override bool ShouldUse(Hero hero, GameState state)
    {
        var heroInRange = state.Common.EnemyHeroes
            .Where(x => x.Distance(hero) <= 250)
            .FirstOrDefault();

        if (heroInRange == null) return false;

        // Splash Damage - 100px Radius
        var nearby = state.Common.Enemies
            .Where(x => x.Distance(heroInRange) <= 100)
            .Count();

        if (nearby < 2) return false;

        Target = new Coordinate(heroInRange.X, heroInRange.Y);
        return true;
    }
}

public class BlinkSkill : CoordinateTargetedSkillMove
{
    public BlinkSkill() : base("IRONMAN", "BLINK", 16, 3) { }

    protected override bool ShouldUse(Hero hero, GameState state)
    {
        if (hero.Attribs.MaxMana - hero.Attribs.Mana < 20) return false;
        // TODO: Also Use for Retreat?

        var back200 = state.MyTeam == 0
            ? Math.Max(hero.X - 200, 0)
            : Math.Min(hero.X + 200, Consts.WIDTH);

        Target = new Coordinate(back200, hero.Y);
        return true;
    }
}
#endregion

#region DOCTOR_STRANGE Skills
public class AoEHealSkill : CoordinateTargetedSkillMove
{
    public AoEHealSkill() : base("DOCTOR_STRANGE", "AOEHEAL", 50, 7) { }

    protected override bool ShouldUse(Hero hero, GameState state)
    {
        // TODO: Scan for area in range with multiple teammates needing healing.
        // Range 250
        // Radius 100
        return false;
    }
}

public class ShieldSkill : UnitTargetedSkillMove
{
    public ShieldSkill() : base("DOCTOR_STRANGE", "SHIELD", 20, 6) { }

    protected override bool ShouldUse(Hero hero, GameState state)
    {
        // TODO: Scan for hero in range and shield them!
        // Probably want to prioritise based on threat to them.
        // Range 500
        return false;
    }
}

public class PullSkill : UnitTargetedSkillMove
{
    public PullSkill() : base("DOCTOR_STRANGE", "PULL", 40, 5) { }

    protected override bool ShouldUse(Hero hero, GameState state)
    {
        // TODO: Scan for heroes in range (300), pull them if it means they are going to get ganked by troops.
        return false;
    }
}
#endregion

public class DenyKills : StrategicMove
{
    // TODO: 'Paint' Targets - Only Engage if Current Damage Target
    private int _targetId = -1;

    public override string Move(Hero hero, GameState state)
    {
        if (_targetId != -1 && !state.Entities.Any(x => x.UnitId == _targetId)) _targetId = -1;

        var enemyHeroes = state.Entities
            .OfType<Hero>()
            .Where(x => x.Team != state.MyTeam)
            .ToList();

        var canDeny = state.Entities
            .Where(x => x.Team == state.MyTeam)
            .Where(x => (x.Health / x.MaxHealth) * 100 < 40)
            .Where(x => enemyHeroes.Any(h => h.Distance(x) < h.AttackRange && h.AttackDamage > x.Health))
            .Where(x => x.Distance(hero) < hero.AttackRange)
            .FirstOrDefault();

        if (canDeny == null) return string.Empty;

        _targetId = canDeny.UnitId;
        return Actions.Attack(canDeny).Debug($"{hero.Attribs.HeroType} Attemping Deny of {canDeny.UnitId}");
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
            : threat * 1.5;

        var safe = hero.Health > fear;

        //if (safe)
        //{
        var need = hero.MaxHealth - hero.Health;
        var pc = ((float)hero.Health / hero.MaxHealth) * 100;
        if (pc >= 80 || safe) return string.Empty;

        var bestAffordable = state.Items
            .Where(x => x.Health > 0 && x.Health < need)
            .Where(x => x.IsInstant)
            .OrderBy(x => x.Cost / x.Health)
            .Affordable(state.PlayerGold)
            .FirstOrDefault();

        if (bestAffordable != null)
            return Actions.Buy(bestAffordable).Debug($"Healed {hero.Attribs.HeroType} Via Potion!");
        else
        {
            var tower = state.Entities
                .Where(x => x.UnitType == Units.TOWER)
                .Where(x => x.Team == state.MyTeam)
                .Single();

            var grootNearestHome = state.Entities
                .Where(x => x.UnitType == Units.GROOT)
                .OrderBy(x => x.Distance(tower))
                .FirstOrDefault();

            if (grootNearestHome != null) return Actions.MoveAttack(grootNearestHome).Debug($"{hero.Attribs.HeroType} Attacking Groot {grootNearestHome.UnitId} for Med Fix");
        }

        D.WL($"{hero.Attribs.HeroType} Health {hero.Health}/{hero.MaxHealth} ({pc})");
        //}

        var bush = state.Bushes.OrderBy(x => hero.Distance(x)).FirstOrDefault();
        return Actions.Move(bush.X, bush.Y).Debug($"{hero.Attribs.HeroType} Evac to the Bush!");
    }
}

public class HoldTheLine : StrategicMove
{
    private int _targetId = -1;

    public override string Move(Hero hero, GameState state)
    {
        // Get the Front Line - Support their firing line!
        var myUnits = state.Entities
            .Where(x => x.Team == state.MyTeam)
            .Where(x => x.UnitType == Units.UNIT)
            .ToList();

        if (!myUnits.Any())
        {
            _targetId = -1;
            var tower = state.Entities
                .Where(x => x.Team == state.MyTeam)
                .Where(x => x.UnitType == Units.TOWER)
                .Single();

            return Actions.Move(tower.X, tower.Y).Debug($"Line Folded {hero.Attribs.HeroType} RTB");
        }

        var theFront = state.MyTeam == 0
            ? myUnits.Max(x => x.X) - hero.AttackRange
            : myUnits.Min(x => x.X) + hero.AttackRange;

        var centreFormation = (int)myUnits.Average(x => x.Y);

        if (_targetId != -1)
        {
            var unit = state.Entities.SingleOrDefault(x => x.UnitId == _targetId);

            if (unit == null)
                _targetId = -1;
            else
            {
                // Only Gank if In Range
                if (unit.Distance(hero) <= hero.AttackRange)
                    return Actions.Attack(unit).Debug($"Engaging Previous Target {_targetId} as In-Range");
            }
        }

        // Find Enemy Closest to Front Line AND in Firing Range!
        var closest = state.Entities
            .Where(x => !x.IsNeutral)
            .Where(x => x.Team != state.MyTeam)
            .Where(x => x.Distance(new Coordinate(theFront, hero.Y)) < hero.AttackRange)
            .OrderBy(x => (x.Health / x.MaxHealth) * 100)
            .FirstOrDefault();

        if (closest == null)
        {
            _targetId = -1;
            return Actions.Move(theFront, centreFormation).Debug($"{hero.Attribs.HeroType} To the Front!");
        }

        var heroX = state.MyTeam == 0
            ? closest.X - hero.AttackRange
            : closest.X + hero.AttackRange;

        _targetId = closest.UnitId;
        return Actions.Attack(closest).Debug($"Get {closest.UnitId} Off The Line!");
    }
}

public class RangedFighter : StrategicMove
{
    public override string Move(Hero hero, GameState state)
    {
        var byDistance = state.Entities
            .Where(x => !x.IsNeutral)
            .Where(x => x.Team != state.MyTeam)
            .OrderBy(x => x.Distance(hero));

        var closest = byDistance.FirstOrDefault();

        if (closest == null) return string.Empty;

        // Keep at Arms Length
        var moveX = state.MyTeam == 0
            ? closest.X - hero.AttackRange
            : closest.X + hero.AttackRange;

        return Actions.MoveAttack(moveX, closest.Y, closest.UnitId);
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

public abstract class UnitTargetedSkillMove : SkillMove
{
    protected int TargetId = -1;

    protected override string Command => $"{SkillName} {TargetId}";

    public UnitTargetedSkillMove(string forHero, string skillName, int manaCost, int cooldown) : base(forHero, skillName, manaCost, cooldown)
    {
    }
}

public abstract class CoordinateTargetedSkillMove : SkillMove
{
    protected Coordinate Target = new Coordinate(0, 0);

    protected override string Command => $"{SkillName} {Target.X} {Target.Y}";

    public CoordinateTargetedSkillMove(string forHero, string skillName, int manaCost, int cooldown) : base(forHero, skillName, manaCost, cooldown)
    {
    }
}

public abstract class SkillMove : StrategicMove
{
    protected abstract bool ShouldUse(Hero hero, GameState state);

    protected int CooldownRemaining = 0;
    protected readonly string ForHero;
    protected readonly string SkillName;
    protected readonly int ManaCost;
    protected readonly int Cooldown;

    protected virtual string Command => $"{SkillName}";

    public SkillMove(string forHero, string skillName, int manaCost, int cooldown)
    {
        ForHero = forHero;
        SkillName = skillName;
        ManaCost = manaCost;
        Cooldown = cooldown;
    }

    public override string Move(Hero hero, GameState state)
    {
        if (hero.Attribs.HeroType != ForHero) return string.Empty;
        if (CooldownRemaining > 0)
        {
            CooldownRemaining--;
            D.WL($"{SkillName} Cooldown to {CooldownRemaining}");
            return string.Empty;
        }

        if (ShouldUse(hero, state))
        {
            if (hero.Attribs.Mana >= ManaCost)
            {
                D.WL($"Activating {SkillName}");
                CooldownRemaining = Cooldown;
                return Command;
            }

            D.WL($"Unable to Activate {SkillName} - Mana {hero.Attribs.Mana}/{hero.Attribs.MaxMana}");
        }

        D.WL($"Don't Need to Activate {SkillName}");
        return string.Empty;
    }
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
    public const string BUSH = "BUSH";
    public const string SPAWN = "SPAWN";
}

public struct Coordinate
{
    public int X { get; set; }
    public int Y { get; set; }

    public Coordinate(int x, int y)
    {
        X = x;
        Y = y;
    }
}

public class CommonEntities
{
    private readonly GameState _state;
    private readonly List<Entity> _entities;

    public List<Entity> Mine { get; private set; }
    public List<Entity> Enemies { get; private set; }
    public List<Entity> Neutral { get; private set; }

    public Entity MyTower { get; private set; }
    public Entity EnemyTower { get; private set; }

    public List<Hero> MyHeroes { get; private set; }
    public List<Hero> EnemyHeroes { get; private set; }

    public CommonEntities(GameState state, List<Entity> entities)
    {
        _state = state;
        _entities = entities;

        Mine = entities
            .Where(x => x.Team == state.MyTeam)
            .ToList();
        Enemies = entities
            .Where(x => x.Team != state.MyTeam)
            .Where(x => !x.IsNeutral)
            .ToList();
        Neutral = entities
            .Where(x => x.IsNeutral)
            .ToList();

        MyTower = Mine
            .Where(x => x.UnitType == Units.TOWER)
            .FirstOrDefault();
        EnemyTower = Enemies
            .Where(x => x.UnitType == Units.TOWER)
            .FirstOrDefault();

        MyHeroes = Mine
            .OfType<Hero>()
            .ToList();
        EnemyHeroes = Enemies
            .OfType<Hero>()
            .ToList();
    }
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

    public static int Distance(this Entity a, Coordinate b)
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

    public static string Attack(Entity entity) => $"ATTACK {entity.UnitId}";
    public static string Move(int x, int y) => $"MOVE {x} {y}";
    public static string MoveAttack(Entity entity) => MoveAttack(entity.X, entity.Y, entity.UnitId);
    public static string MoveAttack(int x, int y, int id) => $"MOVE_ATTACK {x} {y} {id}";
    public static string AttackNearest(string unit) => $"ATTACK_NEAREST {unit}";
    public static string Buy(string item) => $"BUY {item}";
    public static string Buy(Item item) => Buy(item.Name);
    public static string Sell(string item) => $"SELL {item}";
    public static string WithMessage(this string action, string message) => $"{action};{message}";
    public static string Debug(this string action, string message)
    {
        D.WL(message);
        return $"{action}";
    }
}

public static class Heroes
{
    private static readonly Dictionary<string, HeroStats> _heroes = new Dictionary<string, HeroStats>
    {
        { "DEADPOOL", new HeroStats("DEADPOOL", 110) },
        { "DOCTOR_STRANGE", new HeroStats("DOCTOR_STRANGE", 245) },
        { "HULK", new HeroStats("HULK", 95) },
        { "IRONMAN", new HeroStats("IRONMAN", 270) },
        { "VALKYRIE", new HeroStats("VALKYRIE", 130) },
        // TODO: Valkyrie
    };

    public static HeroStats Deadpool => _heroes["DEADPOOL"];
    public static HeroStats DoctorStrange => _heroes["DOCTOR_STRANGE"];
    public static HeroStats Hulk => _heroes["HULK"];
    public static HeroStats Ironman => _heroes["IRONMAN"];
    public static HeroStats Valkyrie => _heroes["VALKYRIE"];
}

public static class Consts
{
    public const int MAX_ITEMS = 4;
    public const int WIDTH = 1920;
    public const int HEIGHT = 750;
}

#endregion