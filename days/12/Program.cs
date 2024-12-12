namespace DayTwelve;

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
                current.North ??= current.GetPlotOrAddFence(plotGrid, x, y - 1);
                current.East ??= current.GetPlotOrAddFence(plotGrid, x + 1, y);
                current.South ??= current.GetPlotOrAddFence(plotGrid, x, y + 1);
                current.West ??= current.GetPlotOrAddFence(plotGrid, x - 1, y);
            }
        }
        return plotGrid;
    }

    public int Fence()
    {

        // group regions
        // iterate through via index, checking visited plots
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
            //Console.WriteLine($"A region of {plants.Length} {plants.First().Value} plants with {fences.Length} fences");
            costs.Add(fences.Length * plants.Length);
        }

        return costs.Sum();
    }

    public int BulkFence()
    {
        // link the fences
        Dictionary<Tuple<int, int>, Plot> fencePlots = new();
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

        // connect all the fences with their adjacent neighbor fences
        foreach (HashSet<Plot> region in regions)
        {
            var fences = region.First().GetRegion().Where(p => p.Value == Plot.Fence).ToList();
            LinkFences(fences);
        }

        int cost = 0;
        // now that the fences are joined into subregions, count the fence subregions
        foreach (HashSet<Plot> region in regions)
        {
            HashSet<Plot> seenFences = new();
            var nonFences = region.First().GetRegion().Where(p => p.Value != Plot.Fence).ToList();
            var fences = region.First().GetRegion().Where(p => p.Value == Plot.Fence).ToList();
            List<List<Plot>> sides = new();
            foreach (var fence in fences)
            {
                if (!seenFences.Contains(fence))
                {
                    var parts = fence.GetAdjacentFencesIfThisIsFence();
                    parts.ForEach(f => seenFences.Add(f));
                    sides.Add(parts);
                }
            }
            cost += sides.Count * nonFences.Count;
        }
        return cost;
    }

    private void PrintRegion(HashSet<Plot> region)
    {
        List<List<char>> printable = new();
        for (int y = 0; y < _plotGrid.Length; y += 1)
        {
            List<char> row = new();
            for (int x = 0; x < _plotGrid[0].Length; x += 1)
            {
                row.Add('.');
            }
            printable.Add(row);
        }

        region.First().GetRegion().Where(p => p.Value != Plot.Fence).ToList().ForEach(p =>
        {
            printable[p.Y][p.X] = p.Value;
        });
        foreach(var line in printable)
        {
            Console.WriteLine(line.Aggregate("", (acc, next) => $"{acc}{next}"));
        }
    }

    // Plots can be plants OR fences
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

        public List<Plot> GetAdjacentFencesIfThisIsFence()
        {
            if (Value != Plot.Fence) return [];
            List<Plot> fence = new();
            HashSet<Plot> visited = new();
            Queue<Plot> next = new();
            next.Enqueue(this);
            while (next.Count > 0)
            {
                Plot current = next.Dequeue();
                if (!visited.Add(current)) continue;
                fence.Add(current);
                if (current.East is { Value: Fence })
                    next.Enqueue(current.East);
                if (current.West is { Value: Fence })
                    next.Enqueue(current.West);
                if (current.South is { Value: Fence })
                    next.Enqueue(current.South);
                if (current.West is { Value: Fence })
                    next.Enqueue(current.West);
            }
            return fence;
        }

        public Plot GetPlotOrAddFence(Plot[][] garden, int x, int y)
        {
            try
            {
                Plot other = garden[y][x];
                if (other.Value == Value)
                {
                    return other;
                }
                // set the coords of the fence to the originator
                return new Plot(Fence)
                {
                    X = y,
                    Y = x
                };
            }
            catch (IndexOutOfRangeException)
            {
                return new Plot('|') {
                    X = x,
                    Y = y
                };;
            }
        }

        public override string ToString()
        {
            return $"{Value} - ({X},{Y}) - n: {North?.Value ?? '_'}, e: {East?.Value ?? '_'}, s: {South?.Value ?? '_'}, w: {West?.Value ?? '_'}";
        }
    }

    protected void LinkFences(IEnumerable<Plot> fences)
    {
        foreach (Plot fence in fences)
        {
            Plot originator = _plotGrid[fence.Y][fence.X];
            if (originator.North?.Equals(fence) ?? false)
            {
                fence.South ??= originator;
                if (originator.East is { North.Value: Plot.Fence } and not { Value: Plot.Fence })
                {
                    fence.East ??= originator.East.North;
                    originator.East.North.West ??= fence;
                }
                if (originator.West is { North.Value: Plot.Fence } and not { Value: Plot.Fence })
                {
                    fence.West ??= originator.West.North;
                    originator.West.North.East ??= fence;
                }
            }
            if (originator.South?.Equals(fence) ?? false)
            {
                fence.North ??= originator;
                if (originator.East is { South.Value: Plot.Fence } and not { Value: Plot.Fence })
                {
                    fence.East ??= originator.East.South;
                    originator.East.South.West ??= fence;
                }
                if (originator.West is { South.Value: Plot.Fence } and not { Value: Plot.Fence })
                {
                    fence.West ??= originator.West.South;
                    originator.West.South.East ??= fence;
                }
            }
            if (originator.East?.Equals(fence) ?? false)
            {
                fence.West ??= originator;
                if (originator.North is { East.Value: Plot.Fence } and not { Value: Plot.Fence })
                {
                    fence.North ??= originator.North.East;
                    originator.North.East.South ??= fence;
                }
                if (originator.South is { East.Value: Plot.Fence } and not { Value: Plot.Fence })
                {
                    fence.South ??= originator.South.East;
                    originator.South.East.North ??= fence;
                }
            }
            if (originator.West?.Equals(fence) ?? false)
            {
                fence.East ??= originator;
                if (originator.North is { West.Value: Plot.Fence } and not { Value: Plot.Fence })
                {
                    fence.North ??= originator.North.West;
                    originator.North.West.South ??= fence;
                }
                if (originator.South is { West.Value: Plot.Fence } and not { Value: Plot.Fence })
                {
                    fence.South ??= originator.South.West;
                    originator.South.West.North ??= fence;
                }
            }
        }
    }
}