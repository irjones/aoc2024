namespace DayEleven;

// did Pt 1 as an iterative state generator, f(s0) -> s1 kinda thing, but it fell apart at the early 30's. Re-wrote to use recursion + memoization and not create strings.
public class DayEleven
{
    public static void Main(string[] args)
    {
        string stones = args.Length > 0 ? args[0] : throw new Exception("Stones not provided");
        int blinks = args.Length > 1 ? int.Parse(args[1]) : throw new Exception("Blinks not provided");
        DayEleven solver = new DayEleven();
        var result = stones.Split(' ').Select(stone => solver.Enumerate(stone, blinks)).ToList();
        Console.WriteLine($"Answer for {stones} at {blinks}:\n{result.Aggregate(0ul, (acc, next) => acc + next)}");
    }
    private readonly Dictionary<string, ulong> _memo = new();
    private ulong Enumerate(string stone, int blinks)
    {
        string key = $"{stone}:{blinks}";
        if (_memo.TryGetValue(key, out ulong value))
        {
            return value;
        }

        if (blinks == 0)
        {
            return 1;
        }

        if (stone == "0")
        {
            ulong stonesCount = Enumerate("1", blinks - 1);
            _memo[key] = stonesCount;
            return stonesCount;
        }

        if (stone.Length >= 2 && stone.Length % 2 == 0)
        {
            int halfway = stone.Length / 2;
            ulong leftCount = Enumerate(stone.Substring(0, halfway), blinks - 1);
            ulong rightCount = Enumerate(int.Parse(stone.Substring(halfway, halfway)).ToString(), blinks - 1);
            ulong total = leftCount + rightCount;
            _memo[key] = total;
            return total;
        }

        string mult = (Int64.Parse(stone) * 2024).ToString();
        ulong multCount = Enumerate(mult, blinks - 1);
        _memo[key] = multCount;
        return multCount;
    }
}
