# StockAnalysis (v0.6)

WPFアプリからPython（yfinance）を起動し、株価データ（OHLCV）を取得し、
75日単純移動平均（MA75）を軸にクロスシグナルの分析と、
シンプルなバックテスト結果の表示まで行うアプリケーションです。

---

## v0.6 変更点

- バックテスト用モデルを追加
  - `BacktestSettings`
  - `BacktestTrade`
  - `BacktestResult`
  - `BacktestSummary`
- `BacktestRunner` を追加
  - `Buy` / `Sell` の対象シグナルを切り替え可能
  - シグナル後の営業日数指定でエントリー
  - 指定保有日数後に決済
- バックテスト結果をUIに表示
  - サマリ表示
  - 取引一覧表示
- バックテスト設定をUIから変更可能にした
  - 対象シグナル
  - エントリーまでの営業日数
  - 保有営業日数
- `結果更新` ボタンを追加
  - 価格データを再取得せず、バックテストだけ再計算可能
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
- `BacktestRunner` でバックテストを実行
  - 対象シグナル抽出
  - エントリー日 / 決済日の決定
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
5. シグナル一覧でクロス判定と強度根拠を確認
6. バックテスト結果で取引一覧とサマリを確認
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
| SignalStrength | MA75からの乖離率 |
| HasVolumeSupport | 出来高を伴う上抜けか |
| IsPullbackBounce | 押し目反発か |
| HasStrongBullishCandle | 強い陽線か |
| Reasons | 判定根拠の要約 |

### 判定ロジック

- **Buy**：PrevDiff < 0 → CurrentDiff > 0
- **Sell**：PrevDiff > 0 → CurrentDiff < 0

---

## バックテスト仕様（v0.6時点）

- 対象シグナルはUIで選択
- エントリー価格は「シグナル後の指定営業日」の始値
- 決済価格は「エントリー後の指定保有営業日」の終値
- 営業日は取得済みデータ上の次バーで判定
- データ末尾で決済できないシグナルはスキップ

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

---

## 今後の予定（v0.6 以降）

- スコアリング機能
- チャート表示（ローソク足 + MA）
- シグナル通知
- バックテスト機能の拡張
  - 損切り / 利確
  - 資金管理
  - 資産曲線

---

## 補足

- データは `yfinance` を利用
- キャッシュは `tools/fetcher/cache` に保存
