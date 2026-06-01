using StockAnalyzer.Models;
using StockAnalyzer.Models.Analysis;
using StockAnalyzer.Services;
using Xunit;

namespace StockAnalyzer.Tests;

public sealed class AnalysisHistoryCsvStoreTests
{
    [Fact(DisplayName = "履歴レコードはヘッダー付きCSVとして保存できる")]
    public void Append_WritesHeaderAndRecords()
    {
        var filePath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName(), "analysis-history.csv");
        var store = new AnalysisHistoryCsvStore(filePath);

        store.Append(
        [
            CreateRecord(
                runId: "run-1",
                symbol: "7203.T",
                reasons: "出来高を伴う上抜け / 強い陽線")
        ]);

        var lines = File.ReadAllLines(filePath);
        Assert.Equal(2, lines.Length);
        Assert.Equal(
            "SchemaVersion,RunId,Symbol,ExecutedAt,RequestedPeriod,AnalysisStartDate,AnalysisEndDate,SignalDate,SignalType,Price,MA75,PrevPrice,PrevMA75,PrevDiff,CurrentDiff,Avg20Volume,VolumeRatio,MA75DeviationRate,Score,Rank,ScoreBreakdown,Reasons",
            lines[0]);
        Assert.Equal(
            "1,run-1,7203.T,2026-06-01T09:30:00.0000000+09:00,1y,2026-01-01,2026-06-01,2026-05-31,Buy,105.5,101.25,99,100,-1,4.25,1200,1.25,0.041975308641975309,80,Strong,出来高 20/20,出来高を伴う上抜け / 強い陽線",
            lines[1]);
    }

    [Fact(DisplayName = "既存CSVにはヘッダーを重複させずに追記する")]
    public void Append_WhenFileExists_AppendsRecordsWithoutDuplicateHeader()
    {
        var filePath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName(), "analysis-history.csv");
        var store = new AnalysisHistoryCsvStore(filePath);

        store.Append([CreateRecord(runId: "run-1", symbol: "7203.T", reasons: "first")]);
        store.Append([CreateRecord(runId: "run-2", symbol: "6758.T", reasons: "second")]);

        var lines = File.ReadAllLines(filePath);
        Assert.Equal(3, lines.Length);
        Assert.Contains(",run-1,7203.T,", lines[1]);
        Assert.Contains(",run-2,6758.T,", lines[2]);
    }

    [Fact(DisplayName = "カンマや引用符を含む値はCSVエスケープする")]
    public void Append_WhenFieldsContainSpecialCharacters_EscapesCsvFields()
    {
        var filePath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName(), "analysis-history.csv");
        var store = new AnalysisHistoryCsvStore(filePath);

        store.Append(
        [
            CreateRecord(
                runId: "run-1",
                symbol: "TEST",
                reasons: "reason, with \"quote\"")
        ]);

        var lines = File.ReadAllLines(filePath);
        Assert.EndsWith(",\"reason, with \"\"quote\"\"\"", lines[1]);
    }

    [Fact(DisplayName = "空の履歴レコードはCSVファイルを作成しない")]
    public void Append_EmptyRecords_DoesNotCreateFile()
    {
        var filePath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName(), "analysis-history.csv");
        var store = new AnalysisHistoryCsvStore(filePath);

        store.Append([]);

        Assert.False(File.Exists(filePath));
    }

    [Fact(DisplayName = "保存済みCSVから履歴レコードを読み込める")]
    public void Load_ReadsRecords()
    {
        var filePath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName(), "analysis-history.csv");
        var store = new AnalysisHistoryCsvStore(filePath);
        store.Append(
        [
            CreateRecord(runId: "run-1", symbol: "7203.T", reasons: "first"),
            CreateRecord(runId: "run-2", symbol: "6758.T", reasons: "second")
        ]);

        var records = store.Load();

        Assert.Equal(2, records.Count);
        Assert.Equal("run-1", records[0].RunId);
        Assert.Equal("7203.T", records[0].Symbol);
        Assert.Equal("run-2", records[1].RunId);
        Assert.Equal("6758.T", records[1].Symbol);
    }

    [Fact(DisplayName = "引用符内のカンマ、引用符、改行を含む値を読み込める")]
    public void Load_WhenFieldsContainEscapedCharacters_ReadsRecords()
    {
        var filePath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName(), "analysis-history.csv");
        var store = new AnalysisHistoryCsvStore(filePath);
        store.Append(
        [
            CreateRecord(
                runId: "run-1",
                symbol: "TEST",
                reasons: "line1, with \"quote\"\nline2")
        ]);

        var records = store.Load();

        Assert.Single(records);
        Assert.Equal("line1, with \"quote\"\nline2", records[0].Reasons);
    }

    [Fact(DisplayName = "履歴CSVが存在しない場合は空の履歴を返す")]
    public void Load_WhenFileDoesNotExist_ReturnsEmptyRecords()
    {
        var filePath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName(), "analysis-history.csv");
        var store = new AnalysisHistoryCsvStore(filePath);

        var records = store.Load();

        Assert.Empty(records);
    }

    [Fact(DisplayName = "履歴CSVが空の場合は空の履歴を返す")]
    public void Load_WhenFileIsEmpty_ReturnsEmptyRecords()
    {
        var filePath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName(), "analysis-history.csv");
        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
        File.WriteAllText(filePath, string.Empty);
        var store = new AnalysisHistoryCsvStore(filePath);

        var records = store.Load();

        Assert.Empty(records);
    }

    [Fact(DisplayName = "履歴CSVのヘッダーが不正な場合は読み込み失敗にする")]
    public void Load_WhenHeaderIsInvalid_Throws()
    {
        var filePath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName(), "analysis-history.csv");
        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
        File.WriteAllText(filePath, "InvalidHeader\n");
        var store = new AnalysisHistoryCsvStore(filePath);

        Assert.Throws<InvalidDataException>(() => store.Load());
    }

    private static AnalysisHistoryRecord CreateRecord(
        string runId,
        string symbol,
        string reasons)
    {
        return new AnalysisHistoryRecord
        {
            RunId = runId,
            Symbol = symbol,
            ExecutedAt = new DateTimeOffset(2026, 6, 1, 9, 30, 0, TimeSpan.FromHours(9)),
            RequestedPeriod = "1y",
            AnalysisStartDate = new DateOnly(2026, 1, 1),
            AnalysisEndDate = new DateOnly(2026, 6, 1),
            SignalDate = new DateOnly(2026, 5, 31),
            SignalType = SignalType.Buy,
            Price = 105.5d,
            Ma75 = 101.25d,
            PrevPrice = 99d,
            PrevMa75 = 100d,
            PrevDiff = -1d,
            CurrentDiff = 4.25d,
            Avg20Volume = 1200d,
            VolumeRatio = 1.25d,
            Ma75DeviationRate = 0.04197530864197531d,
            Score = 80,
            Rank = SignalRank.Strong,
            ScoreBreakdown = "出来高 20/20",
            Reasons = reasons
        };
    }
}
