# StockAnalysis (v0.5)

WPFアプリからPython（yfinance）を起動し、株価データ（OHLCV）を取得し、
75日単純移動平均（MA75）を軸にクロスシグナルの強さまで分析・表示するアプリケーションです。

---

## v0.5 変更点

- 分析用モデルを導入
  - `PriceBar`
  - `AnalysisBar`
  - `SignalCandidate`
  - `SignalResult`
- 分析フローを `StockAnalysisService` に集約
- Phase1 向けの分析器を追加
  - 出来高20日平均
  - `VolumeRatio`
  - MA75乖離率
  - 押し目判定
  - ローソク足特徴量
- シグナル一覧に強さ判定の根拠を表示

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
2. 「取得」ボタンを押す
3. OHLCV + MA75 + 出来高系指標が表示される
4. シグナル一覧でクロス判定と強度根拠を確認

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

## 今後の予定（v0.5 以降）

- バックテスト機能
- スコアリング機能
- チャート表示（ローソク足 + MA）
- シグナル通知

---

## 補足

- データは `yfinance` を利用
- キャッシュは `tools/fetcher/cache` に保存
