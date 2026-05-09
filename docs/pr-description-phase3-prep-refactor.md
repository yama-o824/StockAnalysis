# PR: Phase3前の準備リファクタ

## 概要

Phase3 のスコアリング機能を安全に追加できるように、シグナル評価周辺を小さく整理しました。

このPRでは、既存の分析結果、バックテスト結果、Python連携、UIの見た目は変えない方針です。スコアリング機能そのものはまだ実装していません。

## 変更内容

- `SignalEvaluator` の既存挙動を固定する単体テストを追加
- `SignalStrength` を `Ma75DeviationRate` に命名整理
- 強い陽線判定を `StrongBullishCandleEvaluator` に分離
- シグナル根拠文の生成を `SignalReasonBuilder` に分離
- Phase3 の `SignalScore` / `SignalRank` 配置案を文書化
- README に「挙動変更なしの準備リファクタ」であることを明記

## 設計意図

- `SignalEvaluation` は評価事実を表す責務に寄せる
- `SignalScore` は Phase3 で別モデルとして追加し、既存評価値と混ぜない
- 表示用の整形は `Presentation` 層に閉じる
- Backtest は既存の `SignalResult` を参照するままにし、スコア追加後も挙動を変えない

## テスト方針

- `SignalEvaluatorTests` で既存の評価値と理由文を固定
- `BacktestRunnerTests` は既存バックテスト挙動の回帰確認に使う
- ローカルでは以下を実行する

```bash
dotnet test src/StockAnalyzer/StockAnalyzer.sln
```

## 確認事項

- 分析結果の計算ロジックは変更しない
- バックテストのエントリー/決済/集計ロジックは変更しない
- Python fetcher には触れない
- UI の列構成や見た目は大きく変えない

## 今回はやらないこと

- `SignalScore` / `SignalRank` の実装
- スコア列のUI追加
- スコアを使ったバックテスト条件の追加
- DB保存
- 自動売買
- Python連携の変更
