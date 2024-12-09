namespace DayEight;

public class DayEight
{
    public static void Main(string[] args)
    {
        string dataPath = args.Length > 0 ? args[0] : "./data/testinput.txt";

        char[][] parsedData = new Parser(new Loader(dataPath).LoadedData).Parsed;
        new List<Result> {
            new Solver(parsedData, useFullExtrapolation: false).Solve(),
            new Solver(parsedData, useFullExtrapolation: true).Solve()
            }.ForEach(Console.WriteLine);
    }
}

public class Loader(string path)
{
    public readonly string LoadedData = Load(path);

    private static string Load(string path)
    {
        string data = "";
        try
        {
            using StreamReader reader = new(path);
            data = reader.ReadToEnd();
        }
        catch (IOException e)
        {
            Console.WriteLine("File could not be read: " + e.Message);
            Environment.Exit(1);
        }
        return data;
    }
}
public class Parser(string data)
{
    public readonly char[][] Parsed = data.Split('\n')
        .Where(line => line.Length > 0)
        .Select(line => line.ToCharArray())
        .ToArray();
}

public class PointDiff(Coordinate a, Coordinate b)
{
    public int XLen = a.X - b.X;
    public int YLen = a.Y - b.Y;
    // prob should abstract this out but gonna just do it here until it's working
    public List<Coordinate> Antinodes(Coordinate limit, bool isFullExtrapolation)
    {
        if (!isFullExtrapolation)
        {
            return
                [
                    new Coordinate(a.X + XLen, a.Y + YLen),
                    new Coordinate(b.X - XLen, b.Y - YLen)
                ];
        }

        List<Coordinate> series = new();
        int aX = a.X + XLen;
        int aY = a.Y + YLen;
        int bX = b.X - XLen;
        int bY = b.Y - YLen;
        Coordinate nextA = new(aX, aY);
        Coordinate nextB = new(bX, bY);
        while(nextA.InBoundsOf(limit.X, limit.Y) || nextB.InBoundsOf(limit.X, limit.Y))
        {
            series.AddRange([nextA, nextB]);
            aX += XLen;
            aY += YLen;
            bX -= XLen;
            bY -= YLen;
            nextA = new(aX, aY);
            nextB = new(bX, bY);
        }
        return series;
    }
}

public class Coordinate(int x, int y)
{
    public readonly int X = x;
    public readonly int Y = y;
    public bool InBoundsOf(int inclusiveBoundX, int inclusiveBoundY)
    {
        return X >= 0
            && X <= inclusiveBoundX
            && Y >= 0
            && Y <= inclusiveBoundY;
    }

    public override bool Equals(object? obj)
    {
        if (obj is Coordinate other)
        {
            return other.X == X && other.Y == Y;
        }
        return false;
    }

    public override int GetHashCode()
    {
        int hash = 13;
        hash = hash * 7 + X.GetHashCode();
        hash = hash * 7 + Y.GetHashCode();
        return hash;
    }

    public override string ToString()
    {
        return $"({X}, {Y})";
    }
}

public class Solver(char[][] data, bool useFullExtrapolation)
{
    private const char EmptySpace = '.';
    // assumes a square
    private readonly int _xLen = data[0].Length - 1;
    private readonly int _yLen = data.Length - 1;

    public Result Solve()
    {
        // PLACE: get the coords of each tower by frequency
        // e.g. 'A' -> [(2, 3), (3, 7)], 'a' -> [(1, 1)]
        Dictionary<char, HashSet<Coordinate>?> frequenciesAtCoordinates = new();
        for (int y = 0; y < data.Length; y += 1)
        {
            for (int x = 0; x < data[y].Length; x += 1)
            {
                char current = data[y][x];
                if (current != EmptySpace)
                {
                    var currentCoordinate = new Coordinate(x, y);


                    HashSet<Coordinate>? coordinates = frequenciesAtCoordinates.GetValueOrDefault(current, new HashSet<Coordinate>());
                    if (coordinates == null) throw new NullReferenceException("Did not return default HashSet :(");
                    coordinates.Add(currentCoordinate);
                    frequenciesAtCoordinates[current] = coordinates;
                }
            }
        }


        // TRIANGULATE: using the coordinates for each frequency, determine the antinodes
        Coordinate limitCoordinate = new Coordinate(_xLen, _yLen);
        List<List<Coordinate>> antinodeLocationsByFrequency = new();
        foreach (var (_, coordinates) in frequenciesAtCoordinates)
        {
            if (coordinates == null) throw new NullReferenceException("Coordinates were unexpectedly null");
            foreach (var coordinate in coordinates)
            {
                List<Coordinate> antinodes = useFullExtrapolation ? [coordinate] : [];
                foreach (Coordinate other in coordinates)
                {
                    if (coordinate.Equals(other)) continue;
                    var inBoundsAntinodes = new PointDiff(coordinate, other).Antinodes(limitCoordinate, useFullExtrapolation).Where(antinode => antinode.InBoundsOf(_xLen, _yLen));
                    antinodes.AddRange(inBoundsAntinodes);
                }
                antinodeLocationsByFrequency.Add(antinodes);
            }
        }

        // FILTER/REDUCE: remove antinodes not in bounds of OG map. The count is the answer.
        HashSet<Coordinate> inboundsAntinodes = antinodeLocationsByFrequency
            .Aggregate(new List<Coordinate>(), (acc, next) => acc.Concat(next).ToList())
            .ToHashSet();

        return new Result(inboundsAntinodes.Count, "PartOne");
    }
}

public class Result(int value, string label = "Result")
{
    public readonly int Value = value;
    public override string ToString()
    {
        return $"{label}: {Value}";
    }
}

