using StockAnalyzer.Analyzer.Indicators;
using StockAnalyzer.Analyzer.Signals;
using StockAnalyzer.Models.Analysis;
using StockAnalyzer.Models.Market;

namespace StockAnalyzer.Services;

public sealed class StockAnalysisService
{
    private readonly AnalysisBarBuilder _analysisBarBuilder = new();
    private readonly CrossSignalDetector _crossSignalDetector = new();
    private readonly SignalEvaluator _signalEvaluator = new();
    private readonly SignalScoreCalculator _signalScoreCalculator = new();

    public AnalysisResult Analyze(IReadOnlyList<PriceBar> priceBars)
    {
        ArgumentNullException.ThrowIfNull(priceBars);

        var analysisBars = _analysisBarBuilder.Build(priceBars);
        var signalCandidates = _crossSignalDetector.Detect(analysisBars);
        var signalResults = signalCandidates
            .Select(_signalEvaluator.Evaluate)
            .Select(AddScore)
            .ToList();

        return new AnalysisResult
        {
            Bars = analysisBars,
            Signals = signalResults
        };
    }

    private SignalResult AddScore(SignalResult signalResult)
    {
        return new SignalResult
        {
            Candidate = signalResult.Candidate,
            Evaluation = signalResult.Evaluation,
            Score = _signalScoreCalculator.Calculate(
                signalResult.Candidate,
                signalResult.Evaluation)
        };
    }
}
