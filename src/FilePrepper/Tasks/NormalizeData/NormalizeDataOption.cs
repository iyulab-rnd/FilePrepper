﻿namespace FilePrepper.Tasks.NormalizeData;

/// <summary>
/// Types of normalization methods
/// </summary>
public enum NormalizationMethod
{
    /// <summary>
    /// Min-Max scaling
    /// </summary>
    MinMax,

    /// <summary>
    /// Z-score standardization
    /// </summary>
    ZScore
}

/// <summary>
/// Options for data normalization (e.g., which columns to normalize, which method, etc.)
/// </summary>
public class NormalizeDataOption : BaseColumnOption
{
    /// <summary>
    /// Normalization method: MinMax or ZScore
    /// </summary>
    public NormalizationMethod Method { get; set; } = NormalizationMethod.MinMax;

    /// <summary>
    /// For Min-Max scaling, the target range [MinValue, MaxValue]
    /// </summary>
    public double MinValue { get; set; } = 0.0;
    public double MaxValue { get; set; } = 1.0;

    protected override string[] ValidateInternal()
    {
        var errors = new List<string>();

        if (TargetColumns == null || TargetColumns.Length == 0)
        {
            errors.Add("At least one target column must be specified for normalization.");
        }

        // If using MinMax, ensure MinValue < MaxValue
        if (Method == NormalizationMethod.MinMax)
        {
            if (MinValue >= MaxValue)
            {
                errors.Add("MinValue must be less than MaxValue for MinMax normalization.");
            }
        }

        return [.. errors];
    }
}