namespace DayFour;

public interface IResult
{
    void Print();
}

public interface ISolver
{
    IResult Solve();
}

public class Solution
{
    public static void Main(string[] args)
    {
        string dataPath = args.Length > 0
            ? args[0]
            : "./data/TestData.txt";
        var loader = new DataLoader(dataPath);
        new List<ISolver>
        {
            new PartOneSolver(loader),
            new PartTwoSolver(loader)
        }
            .Select(solver => solver.Solve())
            .ToList().ForEach(result => result.Print());
    }
}

public class DataLoader
{
    private readonly string _raw;

    public DataLoader(string filePath)
    {
        try
        {
            using StreamReader reader = new(filePath);
            _raw = reader.ReadToEnd();
        }
        catch (IOException ex)
        {
            Console.WriteLine($"Fatal: Failed to read file {filePath}: {ex.Message}");
            Environment.Exit(1);
        }
    }

    public string Parsed()
    {
        // may change to return the cardinal graph
        return _raw;
    }
}

public class IntResult : IResult
{
    private readonly int _value;
    public IntResult(int value)
    {
        _value = value;
    }
    public void Print()
    {
        Console.WriteLine($"PartOne: {_value}");
    }
}

public class PartOneSolver : ISolver
{
    private readonly string _data;
    public PartOneSolver(DataLoader loader)
    {
        _data = loader.Parsed();
    }

    public IResult Solve()
    {
        CardinalCharGraph graph = new CardinalCharGraph(_data);
        return new IntResult(graph.CountXmasInGraph());
    }
}

public class PartTwoSolver : ISolver
{
    private readonly string _data;
    public PartTwoSolver(DataLoader loader)
    {
        _data = loader.Parsed();
    }

    public IResult Solve()
    {
        CardinalCharGraph graph = new CardinalCharGraph(_data);
        return new IntResult(graph.CountMasXsInGraph());
    }
}

public class CardinalCharGraph
{
    private const char X = 'X';
    private const char M = 'M';
    private const char A = 'A';
    private const char S = 'S';

    private readonly List<Node> _allNodes = new();
    private static readonly Node EmptyNode = new('#');
    class Node
    {
        public char Value { get; }
        public Node North { get; set; } = EmptyNode;
        public Node NorthEast { get; set; } = EmptyNode;
        public Node East { get; set; } = EmptyNode;
        public Node SouthEast { get; set; } = EmptyNode;
        public Node South { get; set; } = EmptyNode;
        public Node SouthWest { get; set; } = EmptyNode;
        public Node West { get; set; } = EmptyNode;
        public Node NorthWest { get; set; } = EmptyNode;

        public Node(char value)
        {
            Value = value;
        }

        public override string ToString()
        {
            char ne = NorthEast.Value;
            char n = North.Value;
            char nw = NorthWest.Value;
            char w = West.Value;
            char sw = SouthWest.Value;
            char s = South.Value;
            char se = SouthEast.Value;
            char e = East.Value;

            return $"[{nw}][{n}][{ne}]\n[{w}][*{Value}][{e}]\n[{sw}][{s}][{se}]";
        }
    }

    public CardinalCharGraph(string src)
    {
        // turn into d2 array of Graph Nodes
        // iterate through d2 array of Nodes which will preserve the grid-coordinate relationship
        Node[][] letterGrid = src.Split('\n')
            .Where(rowStr => rowStr != String.Empty)
            .Select(row => row.ToCharArray()
                .Select(letter => new Node(letter))
                .ToArray())
            .ToArray();

        // then connect those nodes to their neighbors
        for (int i = 0; i < letterGrid.Length; i += 1)
        {
            Node[] row = letterGrid[i];
            for (int j = 0; j < row.Length; j += 1)
            {
                Node currentNode = row[j];
                bool isEasternEdge = j == row.Length - 1;
                bool isWesternEdge = j == 0;
                bool isNorthernEdge = i == 0;
                bool isSouthernEdge = i == letterGrid.Length - 1;

                if (!isEasternEdge)
                {
                    currentNode.East = letterGrid[i][j + 1];
                }
                if (!isWesternEdge)
                {
                    currentNode.West = letterGrid[i][j - 1];
                }
                if (!isNorthernEdge)
                {
                    currentNode.North = letterGrid[i - 1][j];
                }
                if (!isSouthernEdge)
                {
                    currentNode.South = letterGrid[i + 1][j];
                }
                if (!isNorthernEdge && !isEasternEdge)
                {
                    currentNode.NorthEast = letterGrid[i - 1][j + 1];
                }
                if (!isSouthernEdge && !isEasternEdge)
                {
                    currentNode.SouthEast = letterGrid[i + 1][j + 1];
                }
                if (!isSouthernEdge && !isWesternEdge)
                {
                    currentNode.SouthWest = letterGrid[i + 1][j - 1];
                }
                if (!isNorthernEdge && !isWesternEdge)
                {
                    currentNode.NorthWest = letterGrid[i - 1][j - 1];
                }
                _allNodes.Add(letterGrid[i][j]);
            }
        }
    }

    public int CountMasXsInGraph()
    {
        int masXsCount = 0;

        foreach (Node node in _allNodes)
        {
            if (node.Value != A) continue;

            // M S
            //  A
            // M S
            if (node.NorthWest.Value == M && node.SouthWest.Value == M && node.SouthEast.Value == S && node.NorthEast.Value == S)
            {
                masXsCount += 1;
            }

            // S M
            //  A
            // S M
            if (node.NorthWest.Value == S && node.SouthWest.Value == S && node.SouthEast.Value == M && node.NorthEast.Value == M)
            {
                masXsCount += 1;
            }

            // S S
            //  A
            // M M
            if (node.NorthWest.Value == S && node.SouthWest.Value == M && node.SouthEast.Value == M && node.NorthEast.Value == S)
            {
                masXsCount += 1;
            }

            // M M
            //  A
            // S S
            if (node.NorthWest.Value == M && node.SouthWest.Value == S && node.SouthEast.Value == S && node.NorthEast.Value == M)
            {
                masXsCount += 1;
            }
        }
        return masXsCount;
    }

    public int CountXmasInGraph()
    {
        int xmasCount = 0;

        foreach (Node node in _allNodes)
        {
            if (node.Value != X) continue;

            if (node.East is { Value: M, East: { Value: A, East.Value: S } })
            {
                xmasCount += 1;
            }
            if (node.NorthEast is { Value: M, NorthEast: { Value: A, NorthEast.Value: S }})
            {
                xmasCount += 1;
            }
            if (node.SouthEast is { Value: M, SouthEast: { Value: A, SouthEast.Value: S }})
            {
                xmasCount += 1;
            }
            if (node.North is { Value: M, North: { Value: A, North.Value: S }})
            {
                xmasCount += 1;
            }
            if (node.NorthWest is { Value: M, NorthWest: { Value: A, NorthWest.Value: S }})
            {
                xmasCount += 1;
            }
            if (node.West is { Value: M, West: { Value: A, West.Value: S }})
            {
                xmasCount += 1;
            }
            if (node.SouthWest is { Value: M, SouthWest: { Value: A, SouthWest.Value: S }})
            {
                xmasCount += 1;
            }
            if (node.South is { Value: M, South: { Value: A, South.Value: S }})
            {
                xmasCount += 1;
            }
        }
        return xmasCount;
    }
}
