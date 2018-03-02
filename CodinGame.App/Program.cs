using System;
using System.Linq;

class Player
{
    static void Main(string[] args)
    {
        Func<string> L = Console.ReadLine;
        var g = new G(new GS(L().Split(' ').Select(int.Parse).ToArray()));

        while(true)
            Console.WriteLine(g.NextMove(L()));
    }
}

public class GS
{
    public readonly XY Light;
    public XY Thor { get; set; }

    public GS(params int[] inputs)
    {
        Light = new XY(inputs[0], inputs[1]);
        Thor = new XY(inputs[2], inputs[3]);
    }
}

public class G
{
    private readonly GS _state;
    private readonly PathFinder _pathFinder;

    public G(GS state)
    {
        _state = state;
        _pathFinder = new PathFinder();
    }

    public MoveDirection NextMove(string s)
    {
        var direction = _pathFinder.FindDirection(_state.Thor, _state.Light);
        _state.Thor = _state.Thor.Move(direction);
        return direction;
    }
}

public enum MoveDirection
{
    X, N, NE, E, SE, S, SW, W, NW
}

public struct XY
{
    public XY(int x, int y)
    {
        X = x;
        Y = y;
    }

    public int X { get; set; }
    public int Y { get; set; }

    public XY Move(MoveDirection direction)
    {
        var compass = direction.ToString();
        var result = new XY(X, Y);

        if (compass.Contains("N"))
            result.Y--;

        if (compass.Contains("S"))
            result.Y++;

        if (compass.Contains("E"))
            result.X++;

        if (compass.Contains("W"))
            result.X--;

        return result;
    }
}

public class PathFinder
{
    public MoveDirection FindDirection(XY position, XY target)
    {
        var x = target.X == position.X;
        var y = target.Y == position.Y;
        var n = target.Y < position.Y;
        var e = target.X > position.X;
        var s = target.Y > position.Y;
        var w = target.X < position.X;

        if (n && x) return MoveDirection.N;
        if (n && e) return MoveDirection.NE;
        if (e && y) return MoveDirection.E;
        if (s && e) return MoveDirection.SE;
        if (s && x) return MoveDirection.S;
        if (s && w) return MoveDirection.SW;
        if (w && y) return MoveDirection.W;
        if (w && n) return MoveDirection.NW;
        return MoveDirection.X;
    }
}