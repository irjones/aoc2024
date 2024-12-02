namespace DayTwo;

internal interface ISolver
{
    DayTwoResult Solve();
}

public class Solution
{
    public static void Main(string[] args)
    {
        string dataPath = args.Length > 0
            ? args[0]
            : "./data/TestData.txt";
        Console.WriteLine("Day Two\n---");
        var dataLoader = new DayTwoDataLoader(dataPath);
        new List<ISolver>
        {
            new PartOneSolver(dataLoader),
            new PartTwoSolver(dataLoader)
        }.Select(solver => solver.Solve())
            .ToList()
            .ForEach(solution => solution.Print());
    }
}

public class DayTwoDataLoader
{
    private readonly string _raw;
    private readonly int[][] _parsed;

    public DayTwoDataLoader(string src)
    {
        try
        {
            using StreamReader reader = new(src);
            _raw = reader.ReadToEnd();
        }
        catch (IOException e)
        {
            Console.WriteLine("File could not be read: " + e.Message);
            Environment.Exit(1);
        }
        _parsed = _raw.Split('\n')
            .Where(line => line != String.Empty)
            .Select(line => line.Split(' ').Select(int.Parse).ToArray())
            .ToArray();
    }

    public int[][] GetParsedData()
    {
        return _parsed;
    }
}

public class PartOneSolver : ISolver
{
    private readonly int[][] _data;

    public PartOneSolver(DayTwoDataLoader loader)
    {
        _data = loader.GetParsedData();
    }

    public DayTwoResult Solve()
    {
        int safeCount = 0;
        int unsafeCount = 0;

        foreach (int[] report in _data)
        {
            List<int> diffResults = new List<int>();
            for (int i = 1; i < report.Length; i += 1)
            {
                diffResults.Add(report[i-1] - report[i]);
            }

            bool isAllDecreasing = diffResults.TrueForAll(n => n > 0);
            bool isAllIncreasing = diffResults.TrueForAll(n => n < 0);
            bool isSafeGaps = diffResults.TrueForAll(n => Int32.Abs(n) <= 3);

            bool isSafe = (isAllDecreasing || isAllIncreasing) && isSafeGaps;
            if (isSafe)
            {
                safeCount += 1;
            }
            else
            {
                unsafeCount += 1;
            }
        }

        return new DayTwoResult(safeCount, unsafeCount, "PartOne");
    }
}

public class PartTwoSolver : ISolver
{
    private readonly int[][] _data;

    public PartTwoSolver(DayTwoDataLoader loader)
    {
        _data = loader.GetParsedData();
    }

    public DayTwoResult Solve()
    {
        int safeCount = 0;
        int unsafeCount = 0;

        foreach (int[] report in _data)
        {

            List<int> diffResults = Diff(report);
            bool isSafe = IsSafe(diffResults);

            if (!isSafe)
            {
                List<List<int>> potentialDiffLists = new List<List<int>>();
                int skipIndex = 0;
                int currentIndex = 0;
                while (skipIndex < report.Length)
                {
                    List<int> subReport = new List<int>();
                    foreach (int value in report)
                    {
                        if (currentIndex == skipIndex)
                        {
                            currentIndex += 1;
                            continue;
                        }
                        subReport.Add(value);
                        currentIndex += 1;
                    }
                    potentialDiffLists.Add(Diff(subReport.ToArray()));
                    skipIndex += 1;
                    currentIndex = 0;
                }

                isSafe = potentialDiffLists.Select(IsSafe).Any(result => result);
            }

            if (isSafe)
            {
                safeCount += 1;
            }
            else
            {
                unsafeCount += 1;
            }
        }

        return new DayTwoResult(safeCount, unsafeCount, "PartTwo");
    }

    private List<int> Diff(int[] report)
    {
        List<int> diffResults = new List<int>();
        for (int i = 1; i < report.Length; i += 1)
        {
            diffResults.Add(report[i-1] - report[i]);
        }
        return diffResults;
    }

    private bool IsSafe(List<int> diffList)
    {
        bool isAllDecreasing = diffList.TrueForAll(n => n > 0);
        bool isAllIncreasing = diffList.TrueForAll(n => n < 0);
        bool isSafeGaps = diffList.TrueForAll(n => Int32.Abs(n) <= 3);

        return (isAllDecreasing || isAllIncreasing) && isSafeGaps;
    }
}

public class DayTwoResult
{
    private readonly int _safeCount;
    private readonly int _unsafeCount;
    private readonly string _label;

    internal DayTwoResult(int safeCount, int unsafeCount, string label = "Result")
    {
        _safeCount = safeCount;
        _unsafeCount = unsafeCount;
        _label = label;
    }

    public void Print()
    {
        Console.WriteLine(_label + ": " + "[Safe: " + _safeCount + ", Unsafe: " + _unsafeCount + "]");
    }
}