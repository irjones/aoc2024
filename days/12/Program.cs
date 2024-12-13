namespace DayTwelve;

// NOTE TO THE READER:
// I'm not terribly proud of the efficiency or readability of this code, but it works and I got the right answer.
public class DayTwelve
{
    public static void Main(string[] args)
    {
        string path = args.Length > 0 ? args[0] : "./data/testinput.txt";
        var layout = new Input(path).Parse();
        Garden garden = new(layout);

        Console.WriteLine($"PartOne: {garden.Fence()}");
        Console.WriteLine($"PartTwo: {garden.BulkFence()}");
    }
}

public class Input(string path)
{
    private readonly string _rawData = Read(path);
    private static string Read(string path)
    {
        try
        {
            using StreamReader sr = new(path);
            return sr.ReadToEnd();
        } catch (IOException ex)
        {
            Console.WriteLine($"Fatal: Could not read input file: {ex.Message}");
            Environment.Exit(1);
        }
        return "";
    }

    public char[][] Parse()
    {
        return _rawData.Split('\n')
            .Where(line => line != String.Empty)
            .Select(line => line.ToCharArray())
            .ToArray();
    }
}

public class Garden(char[][] layout)
{
    private static readonly Dictionary<char, List<Plot>> PlotsByChar = new();
    private readonly Plot[][] _plotGrid = LinkGrid(layout.Select(row => row.Select(cell => new Plot(cell)).ToArray()).ToArray());

    private static Plot[][] LinkGrid(Plot[][] plotGrid)
    {
        // establish links and partition regions
        for (int y = 0; y < plotGrid.Length; y += 1)
        {
            for (int x = 0; x < plotGrid[y].Length; x += 1)
            {
                Plot current = plotGrid[y][x];
                current.X = x;
                current.Y = y;
                current.North = current.GetPlotOrAddFence(plotGrid, x, y - 1, p => p.South = current);
                current.East = current.GetPlotOrAddFence(plotGrid, x + 1, y, p => p.West = current);
                current.South = current.GetPlotOrAddFence(plotGrid, x, y + 1, p => p.North = current);
                current.West = current.GetPlotOrAddFence(plotGrid, x - 1, y, p => p.East = current);
                if (PlotsByChar.TryGetValue(current.Value, out List<Plot>? plotList))
                {
                    PlotsByChar[current.Value] = plotList.Concat([current]).ToList();
                } else
                {
                    PlotsByChar[current.Value] = [current];
                }

            }
        }
        return plotGrid;
    }

    public int Fence()
    {
        List<HashSet<Plot>> regions = new();
        for (int y = 0; y < _plotGrid.Length; y += 1)
        {
            for (int x = 0; x < _plotGrid.Length; x += 1)
            {
                Plot current = _plotGrid[y][x];
                if (!regions.Any(region => region.Contains(current)))
                {
                    regions.Add(current.GetRegion().ToHashSet());
                }
            }
        }

        List<int> costs = new();
        foreach (var region in regions)
        {
            var fullRegion = region.First().GetRegion();
            var fences = fullRegion.Where(p => p.Value == Plot.Fence).ToArray();
            var plants = fullRegion.Where(p => p.Value != Plot.Fence).ToArray();
            costs.Add(fences.Length * plants.Length);
        }

        return costs.Sum();
    }

    public int BulkFence()
    {
        int cost = 0;
        foreach (List<Plot> nation in PlotsByChar.Values)
        {
            // need to split them out into their locales
            HashSet<Plot> seenPlots = new();
            List<List<Plot>> regions = new();
            foreach (var plot in nation)
            {
                if (seenPlots.Contains(plot)) continue;
                var region = plot.GetRegion();
                region.ForEach(p => seenPlots.Add(p));
                regions.Add(region);
            }
            foreach (var region in regions)
            {
                int sides = 0;
                var nonFencePlots = region.Where(p => p.Value != Plot.Fence).ToArray();
                foreach (Plot plot in nonFencePlots)
                {
                    int plotSides = plot.CountCorners();
                    sides += plotSides;
                }
                cost += sides * nonFencePlots.Length;
            }
        }
        return cost;
    }

    // Plots can contain plants OR fences
    protected class Plot(char value)
    {
        public readonly char Value = value;
        public readonly Guid Id = Guid.NewGuid();
        public int X { get; set; }
        public int Y { get; set; }
        public Plot? North { get; set; }
        public Plot? East { get; set; }
        public Plot? South { get; set; }
        public Plot? West { get; set; }

        public override bool Equals(object? obj)
        {
            return obj is Plot other && other.Id == Id;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public const char Fence = '|';

        public int CountCorners()
        {
            int count = 0;
            bool northIsFence = North is { Value: Fence };
            bool southIsFence = South is { Value: Fence };
            bool westIsFence = West is { Value: Fence };
            bool eastIsFence = East is { Value: Fence };

            // no corners
            if (((northIsFence && southIsFence) && (!westIsFence && !eastIsFence))
                || (westIsFence && eastIsFence) && (!northIsFence && !southIsFence))
            {
                return 0;
            }

            // outside corners
            if (northIsFence && eastIsFence) count += 1;
            if (eastIsFence && southIsFence) count += 1;
            if (southIsFence && westIsFence) count += 1;
            if (northIsFence && westIsFence) count += 1;

            // inner corners
            if (!northIsFence && !eastIsFence && North is { East.Value: Fence }) count += 1;
            if (!northIsFence && !westIsFence && North is { West.Value: Fence }) count += 1;
            if (!southIsFence && !eastIsFence && South is { East.Value: Fence }) count += 1;
            if (!southIsFence && !westIsFence && South is { West.Value: Fence }) count += 1;

            return count;
        }

        public List<Plot> GetRegion()
        {
            List<Plot> neighbors = new();
            HashSet<Plot> visited = new();
            Queue<Plot> next = new();
            next.Enqueue(this);
            while(next.Count > 0)
            {
                Plot current = next.Dequeue();
                if (!visited.Add(current)) continue;
                neighbors.Add(current);
                if (current.North is not null) next.Enqueue(current.North);
                if (current.East is not null) next.Enqueue(current.East);
                if (current.South is not null) next.Enqueue(current.South);
                if (current.West is not null) next.Enqueue(current.West);
            }
            return neighbors;
        }

        public Plot GetPlotOrAddFence(Plot[][] garden, int x, int y, Action<Plot> callback)
        {
            try
            {
                Plot other = garden[y][x];
                if (other.Value == Value)
                {
                    return other;
                }
                // set the coords of the fence to the originator
                Plot p = new Plot(Fence)
                {
                    X = x,
                    Y = y
                };
                callback(p);
                return p;
            }
            catch (IndexOutOfRangeException)
            {
                Plot p = new Plot(Fence)
                {
                    X = x,
                    Y = y
                };
                callback(p);
                return p;
            }
        }
    }
}