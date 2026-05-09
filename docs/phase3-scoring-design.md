# Phase3 Scoring Design

Phase3 では、既存の分析結果とバックテスト結果を変えずに、シグナルの優先度を判断するためのスコアリング情報を追加する。

## 目的

- `SignalScore` と `SignalRank` を既存の `SignalEvaluation` と衝突しない形で追加する
- クロス検出、評価、スコアリング、表示用DTOの責務を分ける
- Phase3 の最小実装を小さな差分で進められる配置にする

## 配置方針

### Model

`Models/Scoring` を追加し、スコアリング専用モデルを置く。

- `SignalScore`
  - シグナル単位のスコア結果
  - 合計点、評価内訳、ランクを持つ
- `SignalRank`
  - `A` / `B` / `C` などの表示・分類用ランク
  - 数値スコアからの変換結果として扱う
- `SignalScoreDetail`
  - 必要になった場合のみ追加
  - どの評価要素が何点だったかを表す

`SignalEvaluation` は既存の評価事実を表すモデルとして維持する。`SignalScore` は評価事実から算出される別モデルとし、`Ma75DeviationRate` や `HasVolumeSupport` と同じ階層に無理に混ぜない。

### Scoring Runner

`Services/Scoring` または `Analyzer/Scoring` に `SignalScoringRunner` を追加する。

Phase3 の最小実装では `Services/Scoring/SignalScoringRunner` を優先する。理由は、既存の `SignalResult` を入力にしてスコアを付与する集計処理であり、クロス検出や個別指標計算そのものではないため。

想定する責務:

- `SignalResult` を受け取る
- `SignalEvaluation` の各評価値から点数を計算する
- `SignalScore` を返す

### Analysis Result

最初の実装では `SignalResult` に `SignalScore? Score` を追加する案を第一候補にする。

理由:

- UI のシグナル一覧で `SignalResult` から表示DTOへ変換する既存の流れを保てる
- Backtest は `SignalResult` を参照しているが、`Score` を使わなければ既存挙動は変わらない
- `AnalysisResult.Signals` の構造を大きく変えなくて済む

ただし、スコアリングが複数方式になる場合は、`AnalysisResult` に `IReadOnlyDictionary<SignalId, SignalScore>` 相当を持たせる案を再検討する。

### Presentation

UI に出す場合は `Presentation/SignalViewRow` に表示用プロパティを追加する。

- `Score`
- `Rank`
- `ScoreReasons`

`SignalScore` を直接 XAML にバインドしない。表示形式や文言は `SignalViewRow` 側で整える。

## 今回はやらないこと

- スコア計算ロジックの実装
- UIへのスコア列追加
- バックテスト条件へのスコア利用
- DB保存
- 自動売買
- Python連携の変更

## 段階的な実装順

1. `Models/Scoring` に `SignalScore` / `SignalRank` を追加
2. `SignalScoringRunner` を追加
3. 既存 `SignalResult` に `Score` を追加
4. `StockAnalysisService` で評価後にスコアを付与
5. `SignalViewRow` に表示用プロパティを追加
6. XAML に列を追加
7. スコアリング単体テストを追加

各段階で既存の分析結果、バックテスト結果、Python連携の挙動は変えない。
