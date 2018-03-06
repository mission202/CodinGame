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
        var game = new Game(debug: true);

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
}

public class Game
{
    private readonly GameState _gs;

    private Queue<string> _toPick;
    private Dictionary<string, List<StrategicMove>> _strategies = new Dictionary<string, List<StrategicMove>>();

    public Game(GameState gs = null, bool debug = false)
    {
        _gs = gs ?? new GameState();
        _gs.Init();

        if (debug)
        {
            D.WL("== YE OLDE BOTTE SHOPPE ==:");
            _gs.Items.ForEach(x => D.WL($"- {x.ToString()}"));
        }

        var fr = new Fundraising();
        var htl = new HoldTheLine();
        var dk = new DenyKills();
        var mdk = new MurderRanged();

        var hulk = new List<StrategicMove>
        {
            new RunToTheBush(healthWhenToRun: 40),
            new HealViaPotions(onPercent: 50),
            fr,
        };

        var ironMan = new List<StrategicMove>
        {
            new RunToTheBush(healthWhenToRun: 30),
            new HealViaPotions(onPercent: 50),
            new CashAndCarry(),
            new FireballSkill(),
            htl,
            new BurningSkill(),
            //new BlinkSkill(),
            dk,
        };

        _strategies.Add(Heroes.Hulk.Name, hulk);
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
}

public class DefaultAction : StrategicMove
{
    public override string Move(Hero hero, GameState state)
    {
        return Actions.Wait.Debug($"{hero.Attribs.HeroType}: Nothing to Do...");
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
            .Where(x => x.Distance(hero) <= 300)
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
        state.Common.EnemyHeroes.ForEach(x => D.WL($"{x.Attribs.HeroType} @ {x.Distance(hero)} > {x.MovementSpeed}"));

        var heroInRange = state.Common.EnemyHeroes
            .Where(x => x.Distance(hero) - x.MovementSpeed >= 700) // Only Fire at Range for DMG
            .Where(x => x.Distance(hero) - x.MovementSpeed <= 900)
            .FirstOrDefault();

        if (heroInRange == null) return false;

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
        var threat = state.Common.Enemies
            .Where(x => x.Distance(hero) <= x.AttackRange)
            .Count();

        if (threat < 2 || hero.Attribs.Mana >= 40) return false;

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
    private bool _selfish;

    public AoEHealSkill(bool selfish = false) : base("DOCTOR_STRANGE", "AOEHEAL", 50, 7)
    {
        _selfish = selfish;
    }

    protected override bool ShouldUse(Hero hero, GameState state)
    {
        var target = _selfish
            ? hero
            : state.Common.MyHeroes.SingleOrDefault(x => x.UnitId != hero.UnitId);

        if (target == null) return false;

        if (target.HealthPercent <= 75)
        {
            Target = new Coordinate(target.X, target.Y);
            return true;
        }

        return false;

        // TODO: Scan for area in range with MULTIPLE teammates needing healing.
        // Range 250
        // Radius 100
    }
}

public class ShieldSkill : UnitTargetedSkillMove
{
    private bool _selfish;

    public ShieldSkill(bool selfish = false) : base("DOCTOR_STRANGE", "SHIELD", 40, 6)
    {
        _selfish = selfish;
    }

    protected override bool ShouldUse(Hero hero, GameState state)
    {
        var target = _selfish
            ? hero
            : state.Common.MyHeroes.SingleOrDefault(x => x.UnitId != hero.UnitId);

        if (target == null) return false;

        if (target.HealthPercent <= 75)
        {
            TargetId = target.UnitId;
            return true;
        }

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

public class RunToTheBush : StrategicMove
{
    private readonly int _whenToRun;

    public RunToTheBush(int healthWhenToRun)
    {
        _whenToRun = healthWhenToRun;
    }

    public override string Move(Hero hero, GameState state)
    {
        if (hero.HealthPercent >= _whenToRun) return string.Empty.Debug($"{hero.Attribs.HeroType}: Don't Need to Hide! ({hero.HealthPercent} > {_whenToRun}) {hero.Health}/{hero.MaxHealth}");

        var bush = state.Bushes
            .Where(x => state.Common.EnemyTower.Distance(x) > 500)
            .OrderBy(x => hero.Distance(x))
            .FirstOrDefault();

        if (bush.X == hero.X && bush.Y == hero.Y) return string.Empty.Debug($"{hero.Attribs.HeroType}: They Can't See Me!");

        return Actions.Move(bush).Debug($"{hero.Attribs.HeroType}: Hiding in Bush!");
    }
}

public class Fundraising : StrategicMove
{
    private int _target = -1;
    public override string Move(Hero hero, GameState state)
    {
        var target = state.Common.Groots.SingleOrDefault(x => x.UnitId == _target)
            ?? state.Common.Groots
                .Where(x => state.Common.EnemyTower.Distance(x) > 500)
                .OrderBy(x => x.Distance(hero))
                .FirstOrDefault();

        if (target == null)
        {
            _target = -1;
            return string.Empty;
        }

        _target = target.UnitId;

        D.WL($"Groot {_target} D:{target.Distance(hero)}, in Range? {(target.Distance(hero) < hero.AttackRange)}");
        var action = (target.Distance(hero) < hero.AttackRange)
            ? Actions.Attack(target)
            : Actions.MoveAttack(target);

        return action.Debug($"Killing Groot {target.UnitId}");
    }
}

public class HealViaPotions : StrategicMove
{
    private int _onPercent;

    public HealViaPotions(int onPercent = 75)
    {
        _onPercent = onPercent;
    }

    public override string Move(Hero hero, GameState state)
    {
        D.WL($"{hero.Attribs.HeroType}: {hero.HealthPercent} - >= {_onPercent}? {hero.HealthPercent >= _onPercent}");

        if (hero.HealthPercent >= _onPercent)
        {
            D.WL($"{hero.Attribs.HeroType}: Don't need health potion.");
            return string.Empty;
        }

        var wtb = state.Items
            .Where(x => x.IsInstant)
            .Where(x => x.Health > 0)
            .Affordable(state.PlayerGold)
            .OrderBy(x => x.Health / x.Cost)
            .FirstOrDefault();

        if (wtb == null)
        {
            D.WL($"{hero.Attribs.HeroType}: Unable to afford heal potions.");
            return string.Empty;
        }

        D.WL($"{hero.Attribs.HeroType}: Healing {wtb.Health}hp via {wtb.Name} for {wtb.Cost}g");
        return $"BUY {wtb.Name}";
    }
}

public class CashAndCarry : StrategicMove
{
    private List<Item> _carrying = new List<Item>();

    public override string Move(Hero hero, GameState state)
    {
        D.WL($"== SHOPPING! ==");
        if (!_carrying.Any())
            D.WL($"{hero.Attribs.HeroType}: Currently not carrying anything.");
        else
        {
            D.WL($"{hero.Attribs.HeroType} carrying:");
            _carrying.ToList().ForEach(x => D.WL($"* {x.ToString()}"));
        }

        var interesting = state.Items
            .Where(x => !x.Name.Contains("Bronze"))
            .Affordable(state.PlayerGold)
            .Where(x => x.BoostsDamage)
            .OrderBy(x => x.Damage / x.Cost)
            .ToList();

        // If we're out of slots, we need to make sure it's an improvement.
        if (_carrying.Count >= Consts.MAX_ITEMS)
        {
            var worst = _carrying.OrderByDescending(x => x.Damage).First();
            D.WL($"{hero.Attribs.HeroType}: Out of Item Slots - Ensuring Better Than {worst.Name} @ {worst.Damage}");

            interesting = interesting
                .Where(x => x.Damage > worst.Damage)
                .ToList();
        }

        if (!interesting.Any())
        {
            D.WL($"{hero.Attribs.HeroType} doesn't want to buy anything");
            return string.Empty;
        }

        D.WL($"{hero.Attribs.HeroType}: Affordable & Interesting:");
        interesting.ForEach(x => D.WL($"- {x.ToString()}"));


        var wtb = interesting.First();
        D.WL($"{hero.Attribs.HeroType}: Buying {wtb.Name} @ {wtb.Damage} for {wtb.Cost}g");

        _carrying.Add(wtb);
        return $"BUY {wtb.Name}";
    }
}

public class DenyKills : StrategicMove
{
    public override string Move(Hero hero, GameState state)
    {
        var enemyHeroes = state.Common.EnemyHeroes;

        var canDeny = state.Common.Mine
            .Where(x => (x.Health / x.MaxHealth) * 100 < 40) /* In case I'm mega-buff :) */
            .Where(x => x.Health <= hero.AttackDamage)
            .Where(x => enemyHeroes.Any(h => h.Distance(x) < h.AttackRange && h.AttackDamage > x.Health))
            .Where(x => x.Distance(hero) < hero.AttackRange)
            .OrderBy(x => x.GoldValue)
            .FirstOrDefault();

        if (canDeny == null) return string.Empty;
        return Actions.Attack(canDeny).Debug($"{hero.Attribs.HeroType} Attemping Deny of {canDeny.UnitId}");
    }
}

public class HoldTheLine : StrategicMove
{
    public override string Move(Hero hero, GameState state)
    {
        // Units Have Movement Speed of 150

        // Get the Front Line - Support their firing line!
        var myUnits = state.Common.MyUnits;

        if (myUnits.Count <= 2)
        {
            var tower = state.Common.MyTower;
            return Actions.Move(tower.X, tower.Y).Debug($"Line Folded, {hero.Attribs.HeroType} RTB");
        }

        if (state.Common.ForwardOfFrontLine(hero.X))
        {
            // Need to Get Back!
            var xPos = state.Common.ShiftX(state.Common.MyFrontLine, -50);
            var topMost = state.Common.MyUnits.Min(x => x.Y);
            return Actions.Move(xPos, topMost).Debug($"{hero.Attribs.HeroType}: Returning to Rear of Front Line @ {xPos},{topMost}!");
        }

        // Find Enemy Closest to Front Line AND in Firing Range!
        var closestWeakest = state.Common.Enemies
            .Where(x => state.Common.ForwardOfFrontLine(x.X))
            .OrderBy(x => x.Health / x.MaxHealth)
            .OrderBy(x => x.Distance(hero))
            .FirstOrDefault();

        if (closestWeakest == null)
        {
            var xPos = state.Common.ShiftX(state.Common.MyFrontLine, 100);
            var topMost = state.Common.MyUnits.Min(x => x.Y);
            return Actions.Move(xPos, topMost).Debug($"{hero.Attribs.HeroType}: Moving With The Front {xPos},{topMost}!");
        }

        if (hero.Distance(closestWeakest) < hero.AttackRange)
        {
            return Actions.Attack(closestWeakest).Debug($"{hero.Attribs.HeroType}: Front Line In Range {closestWeakest.UnitId}");
        }

        // Move Attack
        var heroX = state.Common.ShiftX(closestWeakest.X, -(hero.AttackRange - 30));
        return Actions.MoveAttack(heroX, closestWeakest.Y, closestWeakest.UnitId).Debug($"{hero.Attribs.HeroType}: Moving to Attack {closestWeakest.UnitId} on Front Line");
    }
}

public class MurderRanged : StrategicMove
{
    private int _target = -1;

    public override string Move(Hero hero, GameState state)
    {
        if (_target != -1)
        {
            var targeted = state.Entities.SingleOrDefault(x => x.UnitId == _target);
            if (targeted == null)
                _target = -1;
            else
                return Actions.MoveAttack(targeted).Debug($"Murdering Pre-Target Ranged {targeted.UnitId}");
        }

        var rangedThreat = state.Common.Enemies
            .Where(x => x.IsRanged)
            .Where(x => x.Distance(hero) < 300)
            .OrderBy(x => x.Health)
            .OrderBy(x => x.Distance(hero))
            .FirstOrDefault();

        if (rangedThreat == null)
            return string.Empty;

        _target = rangedThreat.UnitId;
        return Actions.MoveAttack(rangedThreat).Debug($"Murdering Ranged {rangedThreat.UnitId}");
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
    public bool IsRanged => AttackRange >= 150;
    public int Health { get; private set; }
    public int MaxHealth { get; private set; }
    public int HealthPercent => (int)(((float)Health / MaxHealth) * 100);
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
    public bool BoostsDamage => Name.Contains("Blade");
    public bool BoostsSpeed => Name.Contains("Boots");
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

    public override string ToString() => $"{Name} {Cost}g DMG:{Damage} H:{Health} MH:{MaxHealth} M:{Mana} MM:{MaxMana} MR:{ManaRegeneration} MV:{MoveSpeed} Potion? {IsPotion} Instant? {IsInstant}";
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
    public List<Entity> Groots { get; private set; }

    public Entity MyTower { get; private set; }
    public Entity EnemyTower { get; private set; }

    public List<Hero> MyHeroes { get; private set; }
    public List<Hero> EnemyHeroes { get; private set; }

    public List<Entity> MyUnits { get; private set; }
    public List<Entity> EnemyUnits { get; private set; }
    public int MyFrontLine { get; set; }
    public int EnemyFrontLine { get; set; }

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
        Groots = entities
            .Where(x => x.UnitType == Units.GROOT)
            .ToList();

        if (!Mine.Any() || !Enemies.Any()) return;

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

        MyUnits = Mine
            .Where(x => x.UnitType == Units.UNIT)
            .ToList();
        if (MyUnits.Any())
            MyFrontLine = state.MyTeam == 0
                ? MyUnits.Max(x => x.X)
                : MyUnits.Min(x => x.X);

        EnemyUnits = Enemies
            .Where(x => x.UnitType == Units.UNIT)
            .ToList();
        if (EnemyUnits.Any())
            EnemyFrontLine = state.MyTeam == 0
                ? EnemyUnits.Min(x => x.X)
                : EnemyUnits.Max(x => x.X);
    }

    public int ShiftX(int x, int delta)
    {
        // Negative Delta = "Move Back", Positive = "Move Forward"
        return (_state.MyTeam == 0)
            ? x += delta
            : x -= delta;
    }

    public bool ForwardOfFrontLine(int x)
    {
        return (_state.MyTeam == 0)
            ? x > MyFrontLine
            : x < MyFrontLine;
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
    public static string Move(Coordinate coordinate) => $"MOVE {coordinate.X} {coordinate.Y}";
    public static string Move(Entity entity) => $"MOVE {entity.X} {entity.Y}";
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