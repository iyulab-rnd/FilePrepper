// Utils/MathUtils.cs
using System.Collections.Generic;
using System.Linq;

namespace FilePrepper.Utils;

public static class MathUtils
{
    public static double CalculateMean(IEnumerable<double> values)
    {
        var enumerable = values as double[] ?? values.ToArray();
        return enumerable.Any() ? enumerable.Average() : 0;
    }

    public static double CalculateMedian(IEnumerable<double> values)
    {
        var sorted = values.OrderBy(x => x).ToList();
        if (!sorted.Any()) return 0;

        int n = sorted.Count;
        return n % 2 == 0
            ? (sorted[n / 2 - 1] + sorted[n / 2]) / 2
            : sorted[n / 2];
    }

    public static double CalculateStandardDeviation(IEnumerable<double> values)
    {
        var enumerable = values as double[] ?? values.ToArray();
        if (!enumerable.Any()) return 0;

        double mean = CalculateMean(enumerable);
        double sumOfSquares = enumerable.Sum(x => Math.Pow(x - mean, 2));
        return Math.Sqrt(sumOfSquares / enumerable.Length);
    }

    public static double CalculateMAD(IEnumerable<double> values)
    {
        var enumerable = values as double[] ?? values.ToArray();
        if (!enumerable.Any()) return 0;

        double median = CalculateMedian(enumerable);
        var absoluteDeviations = enumerable.Select(x => Math.Abs(x - median)).OrderBy(x => x).ToList();
        return CalculateMedian(absoluteDeviations);
    }

    public static (double value, int rank) CalculatePercentRank(double value, IEnumerable<double> values)
    {
        var sortedValues = values.OrderBy(x => x).ToList();
        if (!sortedValues.Any()) return (0, 0);

        if (value <= sortedValues[0]) return (0, 0);
        if (value >= sortedValues[^1]) return (100, sortedValues.Count - 1);

        int rank = sortedValues.Count(x => x < value);
        double percentRank = (rank * 100.0) / (sortedValues.Count - 1);
        return (percentRank, rank);
    }

    public static double CalculateZScore(double value, double mean, double standardDeviation)
    {
        return standardDeviation != 0 ? (value - mean) / standardDeviation : 0;
    }

    public static double CalculateRobustZScore(double value, double median, double mad)
    {
        return mad != 0 ? (value - median) / (mad * 1.4826) : 0;
    }

    public static double GetWinsorizedValue(double value, double median, double mad, double threshold = 3.0)
    {
        double deviation = Math.Abs(value - median);
        double maxDeviation = mad * threshold;

        if (deviation > maxDeviation)
        {
            return value > median ? median + maxDeviation : median - maxDeviation;
        }
        return value;
    }
}
