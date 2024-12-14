using System.Text.RegularExpressions;

namespace DayThirteen;

// NOTE TO THE READER
// While I almost immediately identified this as a "solve a system of equations" problem,
// I took me a few hours and finally some help from the Subreddit to apply that math correctly.
// Other things I tried after my first attempt doing math:
// - Tracer mechanism following the slope of the line to the prize.
//   This did well with the test input, but failed on the actual input (and was difficult to troubleshoot)
// - Binary Tree of all routes, count the shortest one.
//   As you might expect, this took too long to compute. I attempted to use a Dictionary to memoize segments, along with
//   vector-binding the search space. Still took too long.
// Anyway, while I received the stars, I admit that didn't fully earn them by myself.

public class DayThirteen
{
    private static readonly Regex IntPattern = new Regex("""\d+""");
    public static void Main(string[] args)
    {
        string path = args.Length > 0 ? args[0] : "./data/testinput.txt";
        DataProvider<Tuple<Tuple<long, long>, Tuple<long, long>, Tuple<long, long>>> provider = new(path, ToXyTuples);
        long totalOne = 0;
        long totalTwo = 0;
        foreach(var tupleTuple in provider.Parsed)
        {
            totalOne += LeastTokens(tupleTuple);
            totalTwo += LeastTokens(tupleTuple, (x3) => x3 + 10000000000000);

        }
        Console.WriteLine($"PartOne: {totalOne}");
        Console.WriteLine($"PartTwo: {totalTwo}");
    }

    private static long LeastTokens(Tuple<Tuple<long, long>, Tuple<long, long>, Tuple<long, long>> input, Func<long, long>? setupAction = null)
    {
        var ((x1, y1), (x2, y2), (x3, y3)) = input;
        if (setupAction is not null)
        {
            x3 = setupAction(x3);
            y3 = setupAction(y3);
        }
        double a = (double)(x3 * (x2 - y2) - x2 * (x3 - y3)) / (x1 * (x2 - y2) + x2 * (y1 - x1));
        double b = (x3 - x1 * a) / x2;
        if (a == Math.Floor(a) && b == Math.Floor(b))
        {
            return (long)a * 3 + (long)b;
        }
        return 0;
    }

    private static List<Tuple<Tuple<long, long>, Tuple<long, long>, Tuple<long, long>>> ToXyTuples(string text)
    {
        return text.Split("\n\n").Where(g => g != String.Empty).Select(group =>
        {
            var matches = IntPattern.Matches(group).Select(m => int.Parse(m.Value)).ToArray();
            return new Tuple<Tuple<long, long>, Tuple<long, long>, Tuple<long, long>>(
                new Tuple<long, long>(matches[0], matches[1]),
                new Tuple<long, long>(matches[2], matches[3]),
                new Tuple<long, long>(matches[4], matches[5])
                );
        }).ToList();
    }
}

public class DataProvider<T>(string src, Func<string, IEnumerable<T>> parser)
{
    public IEnumerable<T> Parsed = parser(ImportText(src));

    private static string ImportText(string path)
    {
        using StreamReader sr = new(path);
        return sr.ReadToEnd();
    }
}
