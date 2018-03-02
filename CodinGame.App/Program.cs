using System;
using System.Linq;

class Player
{
    static void Main(string[] args)
    {
        Func<string> L = Console.ReadLine;
        var g = new G(new GS(L().Split(' ').Select(int.Parse).ToArray()));

        while (true)
        {
            L();
            Console.WriteLine(g.NextMove());
        }
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

    public MoveDirection NextMove()
    {
        var direction = _pathFinder.FindDirection(_state.Thor, _state.Light);
        _state.Thor = _state.Thor.Move(direction);
        return direction;
    }
}

public enum MoveDirection
{
    NoIdea, N, NE, E, SE, S, SW, W, NW
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
        var sameLatitude = target.X == position.X;
        var sameLongitude = target.Y == position.Y;
        var targetToNorth = target.Y < position.Y;
        var targetToEast = target.X > position.X;
        var targetToSouth = target.Y > position.Y;
        var targetToWest = target.X < position.X;

        if (targetToNorth && sameLatitude)
        {
            return MoveDirection.N;
        }

        if (targetToNorth && targetToEast)
        {
            return MoveDirection.NE;
        }

        if (targetToEast && sameLongitude)
        {
            return MoveDirection.E;
        }

        if (targetToSouth && targetToEast)
        {
            return MoveDirection.SE;
        }

        if (targetToSouth && sameLatitude)
        {
            return MoveDirection.S;
        }

        if (targetToSouth && targetToWest)
        {
            return MoveDirection.SW;
        }

        if (targetToWest && sameLongitude)
        {
            return MoveDirection.W;
        }

        if (targetToWest && targetToNorth)
        {
            return MoveDirection.NW;
        }

        return MoveDirection.NoIdea;
    }
}