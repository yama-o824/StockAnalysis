using StockAnalyzer.Presentation;
using Xunit;

namespace StockAnalyzer.Tests;

public sealed class PeriodOptionsTests
{
    [Fact(DisplayName = "期間選択肢はyfinanceのperiodに対応する")]
    public void All_ReturnsSupportedYfinancePeriods()
    {
        string[] expected = ["3mo", "6mo", "1y", "3y", "5y"];
        var values = PeriodOptions.All.Select(x => x.Value).ToArray();

        Assert.Equal(expected, values);
    }

    [Fact(DisplayName = "デフォルト期間は1年で選択肢に含まれる")]
    public void DefaultValue_IsOneYearAndIncludedInOptions()
    {
        Assert.Equal("1y", PeriodOptions.DefaultValue);
        Assert.Contains(PeriodOptions.All, x => x.Value == PeriodOptions.DefaultValue);
    }
}
