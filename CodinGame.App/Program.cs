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
        var game = new Game(outputShop: true);

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

public class IdeaResult
{
    public int MyHeroDeaths { get; private set; }
    public int MyHealth { get; private set; }
    public int MyDamage { get; private set; }
    public int EnemyHeroDeaths { get; private set; }
    public int EnemyUnitDeaths { get; private set; }
    public int EnemyHealth { get; private set; }
    public int MyGoldEarned { get; private set; }
    public int Items { get; private set; }

    public IdeaResult(
        int myHeroDeaths = 0,
        int myHealth = 0,
        int myDamage = 0,
        int enemyHeroDeaths = 0,
        int enemyUnitDeaths = 0,
        int enemyHealth = 0,
        int myGoldEarned = 0,
        int items = 0)
    {
        MyHeroDeaths = myHeroDeaths;
        MyHealth = myHealth;
        MyDamage = myDamage;
        EnemyHeroDeaths = enemyHeroDeaths;
        EnemyUnitDeaths = enemyUnitDeaths;
        EnemyHealth = enemyHealth;
        MyGoldEarned = myGoldEarned;
    }

    public static IdeaResult NoChange => new IdeaResult();
    public static IdeaResult HeroDeath(Hero hero) => new IdeaResult(myHeroDeaths: 1, myHealth: -hero.Health);
    public static IdeaResult GrootKill => new IdeaResult(myGoldEarned: 150);
    public static IdeaResult EnemyKill(Entity entity, int threat = 0) => new IdeaResult(myHealth: -threat, enemyHealth: -entity.Health, enemyUnitDeaths: 1, myGoldEarned: entity.GoldValue);
    public static IdeaResult Attack(Hero hero, Entity entity, int threat = 0) => new IdeaResult(enemyHealth: -hero.AttackDamage, myHealth: -threat);

    public static IdeaResult HitEnemy(Hero hero, Entity entity)
    {
        var score = HitEnemy(entity, hero.AttackDamage);
        var willKill = entity.Health <= hero.AttackDamage;
        if (willKill) score.MyGoldEarned += entity.GoldValue;
        return score;
    }

    public static IdeaResult HitEnemy(Entity entity, int damage)
    {
        var willKill = entity.Health <= damage;
        var isHero = entity is Hero;
        var ehd = isHero && willKill ? 1 : 0;
        var eud = !isHero && willKill ? 1 : 0;

        return new IdeaResult(
            enemyHeroDeaths: ehd,
            enemyUnitDeaths: eud,
            enemyHealth: -damage);
    }

    public static IdeaResult ForHeroPosition(Hero hero, int threat)
    {
        var willDie = hero.Health <= threat;
        return new IdeaResult(
            myHeroDeaths: willDie ? 1 : 0,
            myHealth: -threat,
            myDamage: willDie ? -hero.AttackDamage : 0);
    }

    public static IdeaResult ForItem(Item item)
    {
        return new IdeaResult
        {
            MyHealth = item.Health,
            MyDamage = item.Damage,
            Items = 1
        };
    }

    public override string ToString() => $"MyHeroDeaths:{MyHeroDeaths} MyHealth:{MyHealth} MyDamage:{MyDamage} EnemyHeroDeaths:{EnemyHeroDeaths} EnemyUnitDeaths:{EnemyUnitDeaths}  EnemyHealth:{EnemyHealth} MyGoldEarned:{MyGoldEarned}";
}

public class GameState
{
    public int TurnNumber { get; private set; }
    public int MyTeam { get; private set; }
    public int BushAndSpawnPointCount { get; private set; }

    public int RoundType { get; private set; }
    public bool IsHeroPickRound => RoundType < 0;

    public int PlayerGold { get; set; }
    public int EnemyGold { get; private set; }
    public List<Entity> Entities { get; } = new List<Entity>();
    public List<Item> Items { get; } = new List<Item>();
    public List<Coordinate> Bushes { get; } = new List<Coordinate>();
    public List<Coordinate> Spawns { get; } = new List<Coordinate>();

    public CommonEntities Common { get; private set; }

    private int _carrying = 0;
    public int ItemsSlotsAvailable => Consts.MAX_ITEMS - _carrying;

    protected Func<string> _inputSource = Console.ReadLine;

    private readonly Queue<string> _toPick = new Queue<string>();
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

            string entityType = inputs[0]; // BUSH, from wood1 it can also be SPAWN
            int x = int.Parse(inputs[1]);
            int y = int.Parse(inputs[2]);
            int radius = int.Parse(inputs[3]);

            if (entityType == Units.BUSH)
                Bushes.Add(new Coordinate(x, y));

            if (entityType == Units.SPAWN)
                Spawns.Add(new Coordinate(x, y));
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
        TurnNumber++;

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

    internal void SetHeroes(List<HeroBot> heroes)
    {
        heroes.ForEach(x => _toPick.Enqueue(x.Name));
    }

    internal string NextHero() => _toPick.Dequeue();
}

public class MoveIdea
{
    public string Reason { get; private set; }
    public string Command { get; set; }
    public IdeaResult Result { get; private set; }
    public Action OnExecuted { get; private set; }

    public MoveIdea(string command, string reason, IdeaResult expResult, Action onExecuted = null)
    {
        Command = command;
        Reason = reason;
        Result = expResult;
        OnExecuted = onExecuted ?? new Action(() => { });
    }
}

public class AI
{
    private readonly HeroBot[] _heroes;

    public AI(List<HeroBot> heroes)
    {
        _heroes = heroes.ToArray();
    }

    public string[] GetMoves(GameState state)
    {
        var response = new List<string>();

        foreach (var hero in _heroes)
        {
            var entity = state.Common.MyHeroes.SingleOrDefault(x => x.Attribs.HeroType == hero.Name);
            if (entity == null)
            {
                D.WL($"RIP {hero.Name} :'(");
                continue;
            }

            var ideas = hero.GetIdeas(state);

            // TODO: Pass Ideas to Role, With Scores. AI+Role Designation determines move(s) picked.
            if (ideas.Any())
            {
                D.WL($"Ideas from {hero.Name}:");
                foreach (var idea in ideas)
                {
                    D.WL($"  - {idea.Command} : {idea.Reason} ({idea.Result})");
                }

                var executing = ideas.First();
                response.Add(executing.Command);
                executing.OnExecuted();
                continue;
            }
            else
                D.WL($"{hero.Name} is out of ideas.");

            response.Add(Actions.Default(hero.Name));
        }

        return response.ToArray();
    }
}

public class Game
{
    private readonly bool _outputUnits;

    private readonly GameState _gs;
    private readonly AI _brain;

    public Game(GameState gs = null, bool outputShop = false, bool outputUnits = false)
    {
        _outputUnits = outputUnits;

        _gs = gs ?? new GameState();
        _gs.Init();

        if (outputShop)
        {
            D.WL("== YE OLDE BOTTE SHOPPE ==");
            _gs.Items.ForEach(x => D.WL($" - {x.ToString()}"));
        }

        var heroes = new List<HeroBot>()
        {
            new IronmanCarry(),
            new DrStrangeSupport(),
        };
        _gs.SetHeroes(heroes);
        _brain = new AI(heroes);
    }

    public string[] Moves()
    {
        _gs.Turn();

        if (_gs.IsHeroPickRound)
            return new[] { _gs.NextHero() };

        if (_outputUnits)
        {
            D.WL($"{_gs.Entities.Count()} Units:");
            _gs.Entities.ForEach(x => D.WL($" - {x.ToString()}"));
        }

        return _brain.GetMoves(_gs);
    }

    public string Serialise()
    {
        return $"{_gs.Serialise()}";
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

#region MoveIdea Makers
public class GetIdeasParameters
{
    public HeroBot HeroBot { get; private set; }
    public Hero Hero { get; private set; }
    public GameState State { get; private set; }
    public List<Entity> EnemiesInRange { get; private set; }
    public List<Entity> InEnemiesRange { get; private set; }
    public int Threat { get; private set; }

    public GetIdeasParameters(HeroBot bot, GameState state)
    {
        HeroBot = bot;
        Hero = state.Common.MyHeroes.Where(x => x.Attribs.HeroType == HeroBot.Name).Single();
        State = state;
        EnemiesInRange = state.Common.Enemies.Where(x => x.Distance(Hero) <= Hero.AttackRange).ToList();
        InEnemiesRange = state.Common.Enemies.Where(x => x.Distance(Hero) <= x.AttackRange).ToList();
        Threat = InEnemiesRange.Sum(x => x.AttackDamage);
    }
}

public abstract class MoveIdeaMaker
{
    protected abstract void AddIdeas(GetIdeasParameters p, List<MoveIdea> result);

    public List<MoveIdea> GetIdeas(GetIdeasParameters @params)
    {
        // Kinda ugly, but we end up creating lists everywhere just for "return ..."
        var result = new List<MoveIdea>();
        AddIdeas(@params, result);
        return result;
    }
}

public class AttackEnemiesInRange : MoveIdeaMaker
{
    protected override void AddIdeas(GetIdeasParameters p, List<MoveIdea> result)
    {
        if (p.EnemiesInRange.Any())
        {
            var byHealth = p.EnemiesInRange
                .OrderByDescending(x => x.UnitType) /* UNITs Before Heroes to Avoid Aggro */
                .ThenBy(x => x.Health)
                .Select(x => new { Entity = x, WillKill = x.Health <= p.Hero.AttackDamage })
                .ToList();

            var target = byHealth.FirstOrDefault(x => x.WillKill) ?? byHealth.FirstOrDefault();

            var score = target.WillKill
                ? IdeaResult.EnemyKill(target.Entity, p.Threat)
                : IdeaResult.Attack(p.Hero, target.Entity, p.Threat);

            result.Add(
                new MoveIdea(Actions.Attack(target.Entity),
                    $"Attack Enemy In Range {target.Entity.UnitId} {target.Entity.UnitType} (Kill? {target.WillKill})",
                    score));
        }
    }
}

public class StayBehindFrontLine : MoveIdeaMaker
{
    private readonly int _distanceFromFront;

    public StayBehindFrontLine(int distanceFromFront = 50)
    {
        _distanceFromFront = distanceFromFront;
    }

    protected override void AddIdeas(GetIdeasParameters p, List<MoveIdea> result)
    {
        var frontLine = p.State.Common.MyFrontLine;
        var shifted = p.State.Common.ShiftX(frontLine, -_distanceFromFront);
        var ideaResult = IdeaResult.ForHeroPosition(p.Hero, p.Threat);

        if (p.State.Common.ForwardOfFrontLine(p.Hero.X))
        {
            result.Add(
                new MoveIdea(Actions.Move(shifted, p.State.Common.MyTower.Y),
                    $"Move Back to Front Line {shifted},{p.State.Common.MyTower.Y}",
                    ideaResult));
        }
        else
        {
            result.Add(
                new MoveIdea(Actions.Move(shifted, p.State.Common.MyTower.Y),
                    $"Move Forward to Front Line {shifted},{p.State.Common.MyTower.Y}",
                    ideaResult));
        }
    }
}

public class EscapePullOrSpearflip : MoveIdeaMaker
{
    private readonly int _shiftX;
    private readonly int _shiftY;
    private readonly int _yMin;

    public EscapePullOrSpearflip(int shiftX = 200, int shiftY = 200, int yMin = 300)
    {
        _shiftX = shiftX;
        _shiftY = shiftY;
        _yMin = yMin;
    }

    protected override void AddIdeas(GetIdeasParameters p, List<MoveIdea> result)
    {
        var enemyFront = p.State.Common.EnemyFrontLine;
        var pulled = p.State.Common.ForwardOf(p.Hero.X, enemyFront);

        if (!pulled) return;

        // Spearflipped or Pulled into Enemy Lines - Get Out (Head Away from Lane)
        var coord = new Coordinate(
            p.State.Common.MyTower.X,
            //p.State.Common.ShiftX(p.Hero.X, -_shiftX),
            Math.Max(_yMin, p.Hero.Y - _shiftY));

        // Prefer BLINK if We Have It!
        var blink = p.HeroBot.Skills.OfType<BlinkSkill>().SingleOrDefault();
        var canBlink = blink != null && blink.CanUse(p.Hero, p.State);
        var escapeAction = (canBlink)
            ? blink.Move(p.Hero, p.State, coord)
            : Actions.Move(coord);
        Action onExecuted = canBlink ? new Action(() => blink.Used(p.State)) : new Action(() => { });

        result.Add(
            new MoveIdea(escapeAction,
                $"Pulled Into Enemy Lines - Escape!",
                IdeaResult.HeroDeath(p.Hero),
                onExecuted));
    }
}

public class ThrowFireball : MoveIdeaMaker
{
    private readonly int _minRange;
    private readonly int _maxRange;

    public ThrowFireball(int minRange = 300, int maxRange = 1000)
    {
        _minRange = minRange;
        _maxRange = maxRange;
    }

    protected override void AddIdeas(GetIdeasParameters p, List<MoveIdea> result)
    {
        var fireball = p.HeroBot.Skills.OfType<FireballSkill>().SingleOrDefault();
        if (fireball == null) return;

        if (fireball.CanUse(p.Hero, p.State))
        {
            /*  • range: 900
                • flytime: 0.9
                • impact radius: 50
                • Damage: current mana * 0.2 + 55 * distance traveled / 1000
            */

            var unitInRange = p.State.Common.EnemyUnits
                .Where(x => x.Distance(p.Hero) >= _minRange) // Only Fire at Range for DMG
                .Where(x => x.Distance(p.Hero) <= _maxRange)
                .FirstOrDefault();

            if (unitInRange != null)
            {
                var fireballAction = fireball
                    .Move(p.Hero, p.State, unitInRange.Coordinate)
                    .WithMessage("HADOUKEN!");
                var damage = (int)(p.Hero.Attribs.Mana * 0.2 + 55 * p.Hero.Distance(unitInRange) / 1000);
                result.Add(
                    new MoveIdea(fireballAction,
                        $"Throw Fireball @ {unitInRange.UnitType} #{unitInRange.UnitId} for {damage} Damage",
                         IdeaResult.HitEnemy(unitInRange, damage),
                         () => fireball.Used(p.State)));
            }
        }
    }
}

public class GoShopping : MoveIdeaMaker
{
    private readonly Priority[] _priorities;
    private readonly Priority _p1;
    private readonly Priority _p2;
    private readonly Priority _p3;

    public enum Priority
    {
        Damage,
        Mana,
        Movement
    }

    public GoShopping(Priority p1 = Priority.Damage, Priority p2 = Priority.Movement, Priority p3 = Priority.Mana)
    {
        _priorities = new[] { p1, p2, p3 };
    }

    protected override void AddIdeas(GetIdeasParameters p, List<MoveIdea> result)
    {
        if (p.HeroBot.Carrying >= Consts.MAX_ITEMS) return;

        var affordable = p.State.Items
            .Where(x => !x.IsInstant)
            .Affordable(p.State.PlayerGold);

        for (int i = 0; i < _priorities.Length; i++)
            affordable = OrderByPriority(affordable, _priorities[i]);

        D.WL("Prioritised Items for Shopping:");
        affordable.ToList().ForEach(x => D.WL($" - {x.ToString()}"));

        var interesting = affordable.FirstOrDefault();

        if (interesting == null) return;

        result.Add(
            new MoveIdea(Actions.Buy(interesting),
                $"{interesting.Name} DPG: {interesting.DamagePerGold} MRPG: {interesting.ManaRegenPerGold} MSPG: {interesting.MoveSpeedPerGold}",
                IdeaResult.ForItem(interesting),
                () =>
                {
                    p.HeroBot.Carrying++;
                    p.State.PlayerGold -= interesting.Cost;
                }));
    }

    private IOrderedEnumerable<Item> OrderByPriority(IEnumerable<Item> items, Priority p)
    {
        var ordered = items as IOrderedEnumerable<Item>;

        switch (p)
        {
            case Priority.Damage:
                return ordered == null
                    ? items.OrderByDescending(x => x.DamagePerGold)
                    : ordered.ThenByDescending(x => x.DamagePerGold);
            case Priority.Mana:
                return ordered == null
                    ? items.OrderByDescending(x => x.ManaRegenPerGold)
                    : ordered.ThenByDescending(x => x.ManaRegenPerGold);
            case Priority.Movement:
                return ordered == null
                    ? items.OrderByDescending(x => x.MoveSpeedPerGold)
                    : ordered.ThenByDescending(x => x.MoveSpeedPerGold);
            default:
                return items.OrderByDescending(x => x.Cost);
        }
    }
}

#endregion

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

    public string Move(Hero hero, GameState state, Coordinate target)
    {
        Target = target;
        return Move(hero, state);
    }
}

public abstract class SkillMove : StrategicMove
{
    protected readonly string ForHero;
    protected readonly string SkillName;
    protected readonly int ManaCost;
    protected readonly int Cooldown;

    private int _availableOnTurn;


    protected virtual string Command => $"{SkillName}";
    protected virtual bool ShouldUse(Hero hero, GameState state) => true;


    public SkillMove(string forHero, string skillName, int manaCost, int cooldown)
    {
        ForHero = forHero;
        SkillName = skillName;
        ManaCost = manaCost;
        Cooldown = cooldown;
    }

    public override string Move(Hero hero, GameState state)
    {
        return CanUse(hero, state)
            ? Command
            : string.Empty;
    }

    public bool CanUse(Hero hero, GameState state)
    {
        if (hero.Attribs.HeroType != ForHero)
        {
            D.WL($"{hero.Attribs.HeroType} Can't Use {SkillName}");
            return false;
        }

        var cooldownRemaining = Math.Max(0, _availableOnTurn - state.TurnNumber);
        if (cooldownRemaining > 0)
        {
            D.WL($"Can't Use {SkillName}, Cooldown at {cooldownRemaining}");
            return false;
        }

        if (hero.Attribs.Mana < ManaCost)
        {
            D.WL($"Can't Use {SkillName}, Insufficient Mana {hero.Attribs.Mana}/{ManaCost}");
            return false;
        }

        return true;
    }

    public void Used(GameState state)
    {
        _availableOnTurn = state.TurnNumber + Cooldown;
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
    public Coordinate Coordinate => new Coordinate(X, Y);
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

    public override string ToString() => $"{UnitType} #{UnitId} {Team} {Health}/{MaxHealth}hp {AttackDamage}dmg @ {AttackRange} (Ranged? {IsRanged}) {MovementSpeed}px";
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
    public float DamagePerGold => Damage / Cost;
    public int Health { get; private set; }
    public int MaxHealth { get; private set; }
    public int Mana { get; private set; }
    public int MaxMana { get; private set; }
    public int MoveSpeed { get; private set; }
    public float MoveSpeedPerGold => MoveSpeed / Cost;
    public int ManaRegeneration { get; private set; }
    public float ManaRegenPerGold => ManaRegeneration / Cost;
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

    public override string ToString() => $"{Name} {Cost}g DMG:{Damage} ({DamagePerGold}) H:{Health} MH:{MaxHealth} M:{Mana} MM:{MaxMana} MR:{ManaRegeneration} ({ManaRegenPerGold}) MV:{MoveSpeed} ({MoveSpeedPerGold}) Potion? {IsPotion} Instant? {IsInstant}";
}

public class HulkJungler : HeroBot
{
    public HulkJungler() : base("HULK", 95)
    {

    }

    protected override List<MoveIdea> GetIdeas(Hero me, GameState state)
    {
        var result = new List<MoveIdea>();

        // Jungler: Hide in Trees, Kill Groots (100g)

        // TODO: Separate Ideas from Rating/Ordering (Role)
        // Ideas = "What I Could Do", Priority = "Given My Role"

        // TODO: Update to Only Run When I Cant Kill a Groot (w/ 250 Helth @ 50 Damage)
        if (me.HealthPercent <= 25)
        {
            var bushAtTower = state.Bushes
                .OrderBy(x => state.Common.MyTower.Distance(x))
                .First();

            var runCmd = me.X == bushAtTower.X && me.Y == bushAtTower.Y
                ? Actions.Wait
                : Actions.Move(bushAtTower);

            result.Add(new MoveIdea(
                    runCmd,
                    $"Health Dangerously Low ({me.HealthPercent}%) - Hide!",
                    IdeaResult.HeroDeath(me)));

            // Can We Buy A Potion?
            var wtb = state.Items
                .Where(x => x.IsInstant)
                .Where(x => x.Health > 0)
                .Affordable(state.PlayerGold)
                .OrderBy(x => x.Health / x.Cost)
                .FirstOrDefault();

            if (wtb != null)
            {
                result.Add(new MoveIdea(
                    Actions.Buy(wtb.Name),
                    $"Health Dangerously Low ({me.HealthPercent}%) - Heal!",
                    new IdeaResult(myHeroDeaths: 1, myHealth: me.Health + wtb.Health)));
            }
        }

        var threatsAboveLane = state.Common.Enemies
            .Where(x => x.Y < 350)
            .Where(x => x.Distance(me) <= x.AttackRange + Consts.SAFETY_DIST)
            .OrderBy(x => x.Distance(me))
            .ToList();

        if (threatsAboveLane.Any())
        {
            foreach (var threat in threatsAboveLane)
            {
                /* Heroes have an attack time of 0.1 and units have an attack time of 0.2
                 * The time used to move is distance / moveSpeed
                 * So if your hero has 75 range and travels a distance of 100 on the map, at 200
                 * moveSpeed, it uses up 100 / 200 = 0.5 turn time and still has half the turn
                 * left to attack. The attack will take place at 0.5 + 0.1 since the hero is melee
                 * in this case.
                 */

                // TODO: Calculate Properly!?
                var threatScore = threat.Health < me.AttackDamage
                    ? new IdeaResult(enemyUnitDeaths: 1, enemyHealth: -threat.Health)
                    : new IdeaResult(enemyHealth: -threat.Health);

                result.Add(new MoveIdea(
                    Actions.Attack(threat),
                    $"Threat {threat.UnitId} @ {threat.Distance(me)}",
                    threatScore));
            }
        }

        var grootTargets = state.Common.Groots
            .Where(x => state.Common.BehindFrontLine(x.X))
            .Where(x => x.Distance(state.Common.EnemyTower) > 400) /* Tower Attack Range */
            .OrderBy(x => x.Distance(me))
            .ToList();

        if (grootTargets.Any())
        {
            var safety = me.MaxHealth / 100 * 25;
            var healthRequired = ((400 / me.AttackDamage) * 35) + safety;
            var nearest = grootTargets.FirstOrDefault();
            if (nearest != null && me.Health > healthRequired)
            {
                int nearestD = nearest.Distance(me);
                if (nearestD <= (me.AttackRange - Consts.SAFETY_DIST))
                {
                    result.Add(new MoveIdea(
                        Actions.Attack(nearest),
                        "Groot In Attack Range",
                        IdeaResult.GrootKill));
                }
                else
                {
                    result.Add(new MoveIdea(
                        Actions.MoveAttack(nearest),
                        "Move to Engage Nearest Attack Range",
                        IdeaResult.GrootKill));

                    if (me.HealthPercent <= 25 && nearestD <= 150)
                    {
                        // Likely have aggro! MUST KILL IT!
                        result.Add(new MoveIdea(
                            Actions.MoveAttack(nearest),
                            "Health Critical With Aggro!",
                            new IdeaResult(myGoldEarned: 150, myHeroDeaths: 1, myHealth: -me.Health)));
                    }
                }
            }
        }

        // Let's Also Smash Some Enemy Units for LOLs
        var nearestEnemy = state.Common.EnemyUnits
            .Where(x => x.Distance(state.Common.EnemyTower) >= 400)
            .OrderBy(x => x.Distance(me))
            .ThenBy(x => x.Health)
            .FirstOrDefault();

        if (nearestEnemy != null)
        {
            result.Add(new MoveIdea(
                Actions.Attack(nearestEnemy),
                    $"No Groots, Lets Just Smash {nearestEnemy.UnitType} #{nearestEnemy.UnitId}",
                    IdeaResult.HitEnemy(me, nearestEnemy)));
        }

        return result;
    }

    protected override List<MoveIdea> Prioritise(List<MoveIdea> ideas)
    {
        return ideas
            .OrderByDescending(x => x.Result.MyHeroDeaths)
            .ThenByDescending(x => x.Result.MyGoldEarned)
            .ThenByDescending(x => x.Result.MyHealth)
            .ThenByDescending(x => x.Result.MyDamage)
            .ThenBy(x => x.Result.EnemyHealth)
            .ToList();
    }
}

public class IronmanCarry : HeroBot
{

    public IronmanCarry() : base("IRONMAN", 270)
    {
        Skills.Add(new FireballSkill());
        Skills.Add(new BurningSkill());
        Skills.Add(new BlinkSkill());

        Moves.Add(new AttackEnemiesInRange());
        Moves.Add(new StayBehindFrontLine(distanceFromFront: 50));
        Moves.Add(new ThrowFireball());
        Moves.Add(new EscapePullOrSpearflip());
        Moves.Add(new GoShopping());
    }

    protected override List<MoveIdea> GetIdeas(Hero me, GameState state)
    {
        // Find Bush Closest to Front, Hide in it. Kill enemies in range/spells
        var result = new List<MoveIdea>();

        // Check for Enemies in Range
        var enemiesInRange = state.Common.Enemies.Where(x => x.Distance(me) <= me.AttackRange).ToList();
        var inEnemiesRange = state.Common.Enemies.Where(x => x.Distance(me) <= x.AttackRange).ToList();
        var threat = inEnemiesRange.Sum(x => x.AttackDamage);

        if (me.Health <= threat + 50)
        {
            var bushAtTower = state.Bushes
                .OrderBy(x => state.Common.MyTower.Distance(x))
                .First();

            bool alreadyThere = me.X == bushAtTower.X && me.Y == bushAtTower.Y;
            var runCmd = alreadyThere
                ? Actions.Wait
                : Actions.Move(bushAtTower);

            var runResult = alreadyThere
                ? new IdeaResult()
                : IdeaResult.HeroDeath(me);

            result.Add(new MoveIdea(
                    runCmd,
                    $"Risk of Death (Threat @ {threat}) - Hide!",
                    runResult));

            // Can We Buy A Potion?
            var wtb = state.Items
                .Where(x => x.IsInstant)
                .Where(x => x.Health > 0)
                .Affordable(state.PlayerGold)
                .OrderBy(x => x.Health / x.Cost)
                .FirstOrDefault();

            if (wtb != null)
            {
                result.Add(new MoveIdea(
                    Actions.Buy(wtb.Name),
                    $"Health Dangerously Low ({me.Health}) - Heal!",
                    new IdeaResult(myHeroDeaths: 1, myHealth: me.Health + wtb.Health)));
            }
        }

        return result;
    }

    protected override List<MoveIdea> Prioritise(List<MoveIdea> ideas)
    {
        return ideas
            .OrderByDescending(x => x.Result.MyHeroDeaths)
            .OrderByDescending(x => x.Result.Items)
            .ThenByDescending(x => x.Result.MyDamage)
            .ThenByDescending(x => x.Result.EnemyHeroDeaths)
            .ThenByDescending(x => x.Result.EnemyUnitDeaths)
            .ThenBy(x => x.Result.EnemyHealth)
            .ThenByDescending(x => x.Result.MyGoldEarned)
            .ToList();
    }
}

public class DrStrangeSupport : HeroBot
{

    public DrStrangeSupport() : base("DOCTOR_STRANGE", 245)
    {
        Skills.Add(new AoEHealSkill());
        Skills.Add(new ShieldSkill());
        Skills.Add(new PullSkill());

        Moves.Add(new StayBehindFrontLine());
        Moves.Add(new EscapePullOrSpearflip());
        Moves.Add(new GoShopping(p1: GoShopping.Priority.Mana, p2: GoShopping.Priority.Damage, p3: GoShopping.Priority.Movement));
    }

    protected override List<MoveIdea> GetIdeas(Hero me, GameState state)
    {
        // Find Bush Closest to Front, Hide in it. Kill enemies in range/spells
        var result = new List<MoveIdea>();

        // Check for Enemies in Range
        var enemiesInRange = state.Common.Enemies.Where(x => x.Distance(me) <= me.AttackRange).ToList();
        var inEnemiesRange = state.Common.Enemies.Where(x => x.Distance(me) <= x.AttackRange).ToList();
        var threat = inEnemiesRange.Sum(x => x.AttackDamage);

        if (me.Health <= threat + 50)
        {
            var bushAtTower = state.Bushes
                .OrderBy(x => state.Common.MyTower.Distance(x))
                .First();

            bool alreadyThere = me.X == bushAtTower.X && me.Y == bushAtTower.Y;
            var runCmd = alreadyThere
                ? Actions.Wait
                : Actions.Move(bushAtTower);

            var runResult = alreadyThere
                ? new IdeaResult()
                : IdeaResult.HeroDeath(me);

            result.Add(new MoveIdea(
                    runCmd,
                    $"Risk of Death (Threat @ {threat}) - Hide!",
                    runResult));

            // Can We Buy A Potion?
            var wtb = state.Items
                .Where(x => x.IsInstant)
                .Where(x => x.Health > 0)
                .Affordable(state.PlayerGold)
                .OrderBy(x => x.Health / x.Cost)
                .FirstOrDefault();

            if (wtb != null)
            {
                result.Add(new MoveIdea(
                    Actions.Buy(wtb.Name),
                    $"Health Dangerously Low ({me.Health}) - Heal!",
                    new IdeaResult(myHeroDeaths: 1, myHealth: me.Health + wtb.Health)));
            }
        }

        if (enemiesInRange.Any())
        {
            var byHealth = enemiesInRange
                .OrderByDescending(x => x.UnitType) /* UNITs Before Heroes to Avoid Aggro */
                .ThenBy(x => x.Health)
                .Select(x => new { Entity = x, WillKill = x.Health <= me.AttackDamage })
                .ToList();

            var target = byHealth.FirstOrDefault(x => x.WillKill) ?? byHealth.FirstOrDefault();

            var score = target.WillKill
                ? IdeaResult.EnemyKill(target.Entity, threat)
                : IdeaResult.Attack(me, target.Entity, threat);

            result.Add(
                new MoveIdea(Actions.Attack(target.Entity),
                    $"Attack Enemy In Range {target.Entity.UnitId} {target.Entity.UnitType} (Kill? {target.WillKill})",
                    score));
        }
        else
        {
            // Move to Front Line
            var frontLine = state.Common.MyFrontLine;
            var shifted = state.Common.ShiftX(frontLine, -50);
            result.Add(
                new MoveIdea(Actions.Move(shifted, state.Common.MyTower.Y),
                    $"Move to Front Line {shifted},{state.Common.MyTower.Y}",
                    IdeaResult.NoChange));
        }

        // TODO: Add Skills - Pull, Shield, AoEHeal

        return result;
    }

    protected override List<MoveIdea> Prioritise(List<MoveIdea> ideas)
    {
        return ideas
            .OrderByDescending(x => x.Result.MyHeroDeaths)
            .ThenByDescending(x => x.Result.Items)
            .ThenByDescending(x => x.Result.MyHealth)
            .ThenByDescending(x => x.Result.MyDamage)
            .ThenByDescending(x => x.Result.EnemyHeroDeaths)
            .ThenByDescending(x => x.Result.EnemyUnitDeaths)
            .ThenBy(x => x.Result.EnemyHealth)
            .ThenByDescending(x => x.Result.MyGoldEarned)
            .ToList();
    }
}

public abstract class HeroBot
{
    public string Name { get; private set; }
    public int Range { get; private set; }
    public int Carrying { get; set; }

    public List<SkillMove> Skills { get; private set; } = new List<SkillMove>();
    protected List<MoveIdeaMaker> Moves { get; private set; } = new List<MoveIdeaMaker>();

    public HeroBot(string name, int range)
    {
        Name = name;
        Range = range;
    }

    public List<MoveIdea> GetIdeas(GameState state)
    {
        var @params = new GetIdeasParameters(this, state);

        // Get Ideas from Registered Moves AND Inline Methods
        var ideas = new List<MoveIdea>();
        ideas.AddRange(Moves.SelectMany(x => x.GetIdeas(@params)));
        ideas.AddRange(GetIdeas(@params.Hero, state));

        return Prioritise(ideas);
    }

    protected abstract List<MoveIdea> GetIdeas(Hero hero, GameState state);
    protected abstract List<MoveIdea> Prioritise(List<MoveIdea> ideas);
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

    public static Coordinate Empty => new Coordinate(0, 0);

    public static bool operator ==(Coordinate a, Coordinate b) => a.X == b.X && a.Y == b.Y;
    public static bool operator !=(Coordinate a, Coordinate b) => a.X != b.X || a.Y != b.Y;
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
    public int MyRear { get; set; }
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
        {
            MyFrontLine = state.MyTeam == 0
                ? MyUnits.Max(x => x.X)
                : MyUnits.Min(x => x.X);
            MyRear = state.MyTeam == 0
                ? MyUnits.Min(x => x.X)
                : MyUnits.Max(x => x.X);
        }
        else
        {
            MyFrontLine = MyTower.X;
            MyRear = MyTower.X;
        }

        EnemyUnits = Enemies
            .Where(x => x.UnitType == Units.UNIT)
            .ToList();
        if (EnemyUnits.Any())
            EnemyFrontLine = state.MyTeam == 0
                ? EnemyUnits.Min(x => x.X)
                : EnemyUnits.Max(x => x.X);
        else
            EnemyFrontLine = EnemyTower.X;
    }

    public int ShiftX(int x, int delta)
    {
        // Negative Delta = "Move Back", Positive = "Move Forward"
        return (_state.MyTeam == 0)
            ? x += delta
            : x -= delta;
    }

    public bool ForwardOf(int posX, int checkX)
    {
        return (_state.MyTeam == 0)
            ? posX > checkX
            : posX < checkX;
    }

    public bool ForwardOfFrontLine(int x)
    {
        return (_state.MyTeam == 0)
            ? x > MyFrontLine
            : x < MyFrontLine;
    }

    public bool ForwardOfRear(int x)
    {
        return (_state.MyTeam == 0)
            ? x > MyRear
            : x < MyRear;
    }

    public bool BehindFrontLine(int x)
    {
        return (_state.MyTeam == 0)
            ? x < MyFrontLine
            : x > MyFrontLine;
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

    public static int Distance(this Coordinate a, Coordinate b)
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

    public static string Default(string heroName) => Actions.Wait.Debug($"{heroName}: Nothing to Do...");

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

public static class Consts
{
    public const int MAX_ITEMS = 4;
    public const int WIDTH = 1920;
    public const int HEIGHT = 750;
    public const int SAFETY_DIST = 5;
}

#endregion