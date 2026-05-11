# StockAnalysis (v0.7)

WPFアプリからPython（yfinance）を起動し、株価データ（OHLCV）を取得し、
75日単純移動平均（MA75）を軸にクロスシグナルの分析と、
シンプルなバックテスト結果の表示まで行うアプリケーションです。
v0.7 では Buy シグナルのスコアリングを追加し、シグナルの強さを数値・ランク・内訳で確認できるようになりました。

---

## 未リリース変更

- 株価取得期間をUIから選択可能にした
  - 選択肢は `3ヶ月` / `6ヶ月` / `1年` / `3年` / `5年`
  - 取得期間は `yfinance` の `period` に対応
  - デフォルトは `1年`

---

## v0.7 変更点

- シグナルスコア用モデルを追加
  - `SignalScore`
  - `SignalScoreBreakdown`
  - `SignalRank`
- `SignalScoreCalculator` を追加
  - Buy シグナルのみを 100 点満点で採点
  - `SignalEvaluation` をもとにスコア、ランク、内訳を算出
- 分析結果にスコアを接続
  - `SignalResult` に `Score` を追加
  - `StockAnalysisService` で採点を実行
- バックテスト結果にシグナルスコアを引き継ぎ
  - `BacktestTrade` に `SignalScore` を追加
- UI にスコア表示を追加
  - シグナル一覧に `スコア` / `ランク` / `スコア内訳` を追加
  - バックテスト結果に `スコア` / `ランク` / `スコア内訳` を追加
- テストを追加・更新
  - `SignalScoreCalculatorTests`
  - `BacktestRunnerTests`
  - `SignalEvaluatorTests`

## v0.6 変更点

- バックテスト用モデルを追加
- `BacktestRunner` を追加
- バックテスト結果をUIに表示
- バックテスト設定をUIから変更可能にした
- `結果更新` ボタンを追加
- `BacktestRunner` の単体テストを追加
- 画面の初期表示とバックテスト関連文言を調整

---

## 概要

- WPF → Python プロセス起動
- stdout の JSON を受け取り → DataGrid 表示
- C#側で `PriceBar` へ変換
- `StockAnalysisService` で分析を実行
  - MA75
  - 出来高20日平均
  - クロス判定
  - 押し目判定
  - ローソク足分析
  - シグナルスコアリング（Buy のみ）
- `BacktestRunner` でバックテストを実行
  - 対象シグナル抽出
  - エントリー日 / 決済日の決定
  - シグナルスコアの引き継ぎ
  - 損益率集計
- Python側で当日キャッシュ（同一銘柄・同一期間は再取得しない）

---

## 動作環境

- Windows 10 / 11
- .NET 8 SDK
- Python 3.10以上（推奨）

---

## セットアップ手順

### 1. リポジトリをクローン

```
git clone https://github.com/yama-o824/StockAnalysis.git
cd StockAnalysis
```

### 2. Python（fetcher）のセットアップ

```
cd tools/fetcher
python -m venv .venv
.venv\Scripts\python.exe -m pip install -r requirements.txt
```

※ Python が見つからない場合は PATH を通してください

### 3. WPFアプリを起動

Visual Studio で以下を開いて実行：

```
src/StockAnalyzer
```

---

## 使い方

1. 銘柄コードを入力（例：7203.T）
2. バックテスト条件を設定
   - 対象シグナル
   - エントリーまでの営業日数
   - 保有営業日数
3. 「取得」ボタンを押す
4. OHLCV + MA75 + 出来高系指標が表示される
5. シグナル一覧でクロス判定、スコア、ランク、根拠を確認
6. バックテスト結果で取引一覧、サマリ、シグナルスコアを確認
7. 条件変更後は「結果更新」ボタンでバックテストだけ再計算可能

---

## シグナルの見方

| 列名 | 内容 |
|------|------|
| PrevPrice | 前日の終値 |
| PrevMa | 前日のMA75 |
| PrevDiff | 前日の差分（Price - MA） |
| Price | 当日の終値 |
| Ma | 当日のMA75 |
| CurrentDiff | 当日の差分 |
| Avg20Volume | 出来高20日平均 |
| VolumeRatio | 当日出来高 / 出来高20日平均 |
| Ma75DeviationRate | MA75からの乖離率 |
| Score | Buy シグナルの合計スコア（100点満点） |
| Rank | スコアに応じたランク |
| ScoreBreakdown | 各評価項目の配点内訳 |
| HasVolumeSupport | 出来高を伴う上抜けか |
| IsPullbackBounce | 押し目反発か |
| HasStrongBullishCandle | 強い陽線か |
| Reasons | 判定根拠の要約 |

### 判定ロジック

- **Buy**：PrevDiff < 0 → CurrentDiff > 0
- **Sell**：PrevDiff > 0 → CurrentDiff < 0

### スコアリング仕様（v0.7時点）

- スコア対象は Buy シグナルのみ
- 100 点満点
- 配点
  - MA75乖離: 最大 30 点
  - 出来高支持: 25 点
  - 押し目反発: 25 点
  - 強い陽線: 20 点
- ランク判定
  - `VeryStrong`: 90 点以上
  - `Strong`: 75 点以上
  - `Normal`: 50 点以上
  - `Weak`: 1 点以上
  - `None`: 0 点
- Sell シグナルは未採点

---

## バックテスト仕様（v0.7時点）

- 対象シグナルはUIで選択
- エントリー価格は「シグナル後の指定営業日」の始値
- 決済価格は「エントリー後の指定保有営業日」の終値
- 営業日は取得済みデータ上の次バーで判定
- データ末尾で決済できないシグナルはスキップ
- バックテスト結果には、シグナル発生時点のスコアをそのまま保持する

### 表示される集計

- 対象シグナル数
- 取引数
- スキップ数
- 勝率
- 平均損益率
- 平均利益率
- 平均損失率

### 制約

- 手数料、税金、スリッページは未考慮
- 損切り / 利確は未対応
- 複数ポジション管理は未対応
- 資産曲線は未対応
- Sellシグナルによる決済ロジックは未対応
- ランク別 / スコア別の集計は未対応

## 今後の予定（v0.7 以降）

- チャート表示（ローソク足 + MA）
- シグナル通知
- バックテスト機能の拡張
  - ランク別 / スコア別の集計
  - 損切り / 利確
  - 資金管理
  - 資産曲線

---

## 補足

- データは `yfinance` を利用
- キャッシュは `tools/fetcher/cache` に保存
