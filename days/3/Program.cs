

using System.Text.RegularExpressions;

namespace DayThree;

public static class Constants
{
    public static readonly Regex PairRegex = new Regex("""\d+,\d+""");
    public static readonly Regex MulRegex = new Regex("""(mul\(\d+,\d+\))""");
    public static readonly Regex DoRegex = new Regex("""do\(\)""");
    public static readonly Regex DontRegex = new Regex("""don't\(\)""");
    public static readonly string Do = "do()";
    public static readonly string Dont = "don't()";
}

public interface ISolver
{
    Result Solve();
}

public class Solution
{
    public static void Main(string[] args)
    {
        string dataFilePath = args.Length > 0
            ? args[0]
            : "./data/TestData.txt";

        var dataLoader = new DataLoader(dataFilePath);
        new List<ISolver>()
        {
            new PartOne(dataLoader),
            new PartTwo(dataLoader)
        }.Select(solver => solver.Solve())
            .ToList()
            .ForEach(result => result.Print());
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
        // no action to take here yet, simply return
        return _raw;
    }
}

public class PartOne : ISolver
{
    private string _data;

    public PartOne(DataLoader loader)
    {
        _data = loader.Parsed();
    }

    public Result Solve()
    {
        List<int> products = Constants.MulRegex.Matches(_data).Select(ToProduct).ToList();
        return Result.Of(products.Sum(), "PartOne");
    }

    private int ToProduct(Match match)
    {
        return Constants.PairRegex.Match(match.Value).Value.Split(',')
            .Select(int.Parse)
            .Aggregate(1, (acc, next) => acc *= next);
    }
}

public class PartTwo : ISolver
{
    private readonly string _data;

    public PartTwo(DataLoader loader)
    {
        _data = loader.Parsed();
    }

    public Result Solve()
    {
        var muls = new Queue<Match>(Constants.MulRegex.Matches(_data));
        var dos = new Queue<Match>(Constants.DoRegex.Matches(_data));
        var donts = new Queue<Match>(Constants.DontRegex.Matches(_data));

        // merge matches into single list by index
        Queue<Match> matchQueue = new Queue<Match>();
        while (muls.Count > 0)
        {
            // find the lowest index, add that, iterate
            muls.TryPeek(result: out Match? peekedMul);
            dos.TryPeek(result: out Match? peekedDo);
            donts.TryPeek(result: out Match? peekedDont);
            int peekedMulIndex = peekedMul?.Index ?? Int32.MaxValue;
            int peekedDoIndex = peekedDo?.Index ?? Int32.MaxValue;
            int peekedDontIndex = peekedDont?.Index ?? Int32.MaxValue;

            bool isMulLowest = peekedMulIndex < peekedDoIndex && peekedMulIndex < peekedDontIndex;
            bool isDoLowest = peekedDoIndex < peekedMulIndex && peekedDoIndex < peekedDontIndex;

            Queue<Match> queueToUse = isMulLowest
                ? muls
                : isDoLowest
                    ? dos
                    : donts;

            if (queueToUse.TryDequeue(out Match? next))
            {
                matchQueue.Enqueue(next);
            }
        }

        int product = 0;
        // at the beginning, instructions are enabled
        bool isDo = true;
        while(matchQueue.Count > 0)
        {
            Match next = matchQueue.Dequeue();
            bool isDoUpdate = next.Value == Constants.Do || next.Value == Constants.Dont;
            if (isDoUpdate)
            {
                isDo = next.Value == Constants.Do;
            } else if (isDo)
            {
                product += ToProduct(next);
            }
        }

        return Result.Of(product, "PartTwo");
    }

    private int ToProduct(Match match)
    {
        return Constants.PairRegex.Match(match.Value).Value.Split(',')
            .Select(int.Parse)
            .Aggregate(1, (acc, next) => acc *= next);
    }
}

public class Result
{
    private readonly int _value;
    private readonly string _label;
    protected Result(int value, string label)
    {
        _value = value;
        _label = label;
    }

    public static Result Of(int val, string label = "Result")
    {
        return new Result(val, label);
    }

    public override string ToString()
    {
        return $"{_label}: {_value}";
    }

    public void Print()
    {
        Console.WriteLine(this);
    }
}
