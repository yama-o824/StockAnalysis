using StockAnalyzer.Models;
using StockAnalyzer.Models.Analysis;

namespace StockAnalyzer.Presentation;

public sealed class SignalViewRow
{
    public string Date { get; init; } = string.Empty;
    public SignalType Type { get; init; }
    public double PrevPrice { get; init; }
    public double PrevMa { get; init; }
    public double PrevDiff { get; init; }
    public double Price { get; init; }
    public double Ma { get; init; }
    public double CurrentDiff { get; init; }
    public double? Avg20Volume { get; init; }
    public double? VolumeRatio { get; init; }
    public double? SignalStrength { get; init; }
    public bool HasVolumeSupport { get; init; }
    public bool IsPullbackBounce { get; init; }
    public bool HasStrongBullishCandle { get; init; }
    public string Reasons { get; init; } = string.Empty;

    public static SignalViewRow From(SignalResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        var previous = result.Candidate.Previous;
        var current = result.Candidate.Current;
        var previousMa = previous.Ma75
            ?? throw new InvalidOperationException("Previous MA75 is required.");
        var currentMa = current.Ma75
            ?? throw new InvalidOperationException("Current MA75 is required.");

        return new SignalViewRow
        {
            Date = result.Candidate.Date.ToString("yyyy-MM-dd"),
            Type = result.Candidate.Type,
            PrevPrice = previous.Raw.Close,
            PrevMa = previousMa,
            PrevDiff = previous.Raw.Close - previousMa,
            Price = current.Raw.Close,
            Ma = currentMa,
            CurrentDiff = current.Raw.Close - currentMa,
            Avg20Volume = current.Avg20Volume,
            VolumeRatio = current.VolumeRatio,
            SignalStrength = result.Evaluation.SignalStrength,
            HasVolumeSupport = result.Evaluation.HasVolumeSupport,
            IsPullbackBounce = result.Evaluation.IsPullbackBounce,
            HasStrongBullishCandle = result.Evaluation.HasStrongBullishCandle,
            Reasons = string.Join(" / ", result.Evaluation.Reasons)
        };
    }
}
