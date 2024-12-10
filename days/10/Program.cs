namespace DayTen;

public interface IParser<out TOut>
{
    public TOut Parse(string src);
}

interface ISolver
{
    public Result Solve();
}

public class DayTen
{
    public static void Main(string[] args)
    {
        string dataPath = args.Length > 0 ? args[0] : "./data/testdata.txt";
        int[][] intGrid = new Loader<int[][]>(dataPath, new StringToIntGridParser()).Data;
        new List<ISolver>
        {
            new PartOne(intGrid),
            new PartTwo(intGrid)
        }.ForEach(solver => Console.WriteLine(solver.Solve()));
    }
}

public class PartOne(int[][] data): ISolver
{
    public Result Solve()
    {
        int scores = new TopologicalGraph(data).CalculateTrailheadScores();
        return new Result(scores, "PartOne");
    }
}


public class PartTwo(int[][] data): ISolver
{
    public Result Solve()
    {
        int ratings = new TopologicalGraph(data).CalculateTrailheadRatings();
        return new Result(ratings, "PartTwo");
    }
}

public class TopologicalGraph(int[][] format)
{
    internal class Node(int elevation)
    {
        public readonly int Elevation = elevation;
        public readonly Guid Id = Guid.NewGuid();
        public Node? North { get; set; }
        public Node? East { get; set; }
        public Node? South { get; set; }
        public Node? West { get; set; }

        public override bool Equals(object? obj)
        {
            return obj is Node node && Id == node.Id;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public List<Node> CalculatePaths()
        {
            List<Node> path = new();
            if (Elevation == 9)
            {
                path.Add(this);
                return path;
            }

            if (North?.IsPassableDownward(this) ?? false)
            {
                path.AddRange(North.CalculatePaths());
            }
            if (East?.IsPassableDownward(this) ?? false)
            {
                path.AddRange(East.CalculatePaths());
            }
            if (South?.IsPassableDownward(this) ?? false)
            {
                path.AddRange(South.CalculatePaths());
            }
            if (West?.IsPassableDownward(this) ?? false)
            {
                path.AddRange(West.CalculatePaths());
            }
            path.Add(this);
            return path;
        }

        public bool IsPassableDownward(Node other)
        {
            // if other is 9 and this is 8, it's passable downward
            return other.Elevation + 1 == Elevation;
        }
    }

    private readonly List<Node> _elevationZeroNodes = CompileListOfZeroNodes(format);

    public int CalculateTrailheadScores()
    {
        return _elevationZeroNodes.Select(n => n.CalculatePaths())
            .Select(p => p.Where(n => n.Elevation == 9).ToHashSet())
            .Aggregate(0, (acc,next) => acc + next.Count);
    }

    public int CalculateTrailheadRatings()
    {
        return _elevationZeroNodes.Select(n => n.CalculatePaths())
            .Select(path => path.Count(node => node.Elevation == 9))
            .Sum();
    }

    private static List<Node> CompileListOfZeroNodes(int[][] format)
    {
        List<Node> zeroNodes = new();
        var allNodes = format.Select(row => row.Select(n => new Node(n)).ToArray()).ToArray();
        for (int y = 0; y < allNodes.Length; y += 1)
        {
            for (int x = 0; x < allNodes[y].Length; x += 1)
            {
                Node current = allNodes[y][x];
                if (TryGetNodeAt(allNodes, x, y-1, out Node? northNode) && northNode!.IsPassableDownward(current))
                {
                    current.North = northNode;
                }
                if (TryGetNodeAt(allNodes, x+1, y, out Node? eastNode) && eastNode!.IsPassableDownward(current))
                {
                    current.East = eastNode;
                }
                if (TryGetNodeAt(allNodes, x, y+1, out Node? southNode) && southNode!.IsPassableDownward(current))
                {
                    current.South = southNode;
                }
                if (TryGetNodeAt(allNodes, x-1, y, out Node? westNode) && westNode!.IsPassableDownward(current))
                {
                    current.West = westNode;
                }
                if (current.Elevation == 0)
                {
                    zeroNodes.Add(current);
                }
            }
        }
        return zeroNodes;
    }

    internal static bool TryGetNodeAt(Node[][] arr, int x, int y, out Node? node)
    {
        try
        {
            node = arr[y][x];
            return true;
        } catch (IndexOutOfRangeException)
        {
            node = null;
            return false;
        }
    }

}

public class Loader<TOut>(string path, IParser<TOut> parser)
{
    public readonly TOut Data = Load(path, parser);

    private static TOut Load(string path, IParser<TOut> parser)
    {
        try
        {
            using StreamReader reader = new(path);
            return parser.Parse(reader.ReadToEnd());
        }
        catch (IOException ex)
        {
            Console.WriteLine($"Fatal: Failed to read file {path}: {ex.Message}");
            Environment.Exit(1);
        }
        throw new Exception($"Could not load the data from {path}");
    }
}

public class StringToIntGridParser : IParser<int[][]>
{
    public int[][] Parse(string src)
    {
        return src.Split('\n')
            .Where(line => line != String.Empty)
            .Select(line => line.ToCharArray()
                .Select(c => int.Parse(c.ToString()))
                .ToArray())
            .ToArray();
    }
}

public class Result(int value, string label = "Result")
{
    public override string ToString()
    {
        return $"{label}: {value}";
    }
}
