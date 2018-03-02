using System;
using System.Linq;

class P
{
    static void Main(string[] a)
    {
        Func<string> L = Console.ReadLine;
        var g = new G(new GS(L().Split(' ').Select(int.Parse).ToArray()));

        while(true)
            Console.WriteLine(g.N(L()));
    }
}

class GS
{
    public readonly XY Light;
    public XY Thor { get; set; }

    public GS(params int[] inputs)
    {
        Light = new XY(inputs[0], inputs[1]);
        Thor = new XY(inputs[2], inputs[3]);
    }
}

class G
{
    private readonly GS st;
    private readonly PF p = new PF();

    public G(GS s)
    {
        st = s;
    }

    public Dir N(string s)
    {
        var d = p.D(st.Thor, st.Light);
        st.Thor = st.Thor.Move(d);
        return d;
    }
}

enum Dir
{
    X, N, NE, E, SE, S, SW, W, NW
}

struct XY
{
    public XY(int x, int y)
    {
        X = x;
        Y = y;
    }

    public int X { get; set; }
    public int Y { get; set; }

    public XY Move(Dir direction)
    {
        var c = direction.ToString();
        var r = new XY(X, Y);

        if (c.Contains("N")) r.Y--;
        if (c.Contains("S")) r.Y++;
        if (c.Contains("E")) r.X++;
        if (c.Contains("W")) r.X--;

        return r;
    }
}

class PF
{
    public Dir D(XY p, XY t)
    {
        var x = t.X == p.X;
        var y = t.Y == p.Y;
        var n = t.Y < p.Y;
        var e = t.X > p.X;
        var s = t.Y > p.Y;
        var w = t.X < p.X;

        if (n & x) return Dir.N;
        if (n & e) return Dir.NE;
        if (e & y) return Dir.E;
        if (s & e) return Dir.SE;
        if (s & x) return Dir.S;
        if (s & w) return Dir.SW;
        if (w & y) return Dir.W;
        if (w & n) return Dir.NW;
        return Dir.X;
    }
}