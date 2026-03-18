# StockAnalysis (v0.4)

WPFアプリからPython（yfinance）を起動し、株価データ（OHLCV）を取得し、
75日単純移動平均（MA75）およびクロスシグナルを分析・表示するアプリケーションです。

---

## v0.4 変更点

- クロス判定の根拠情報を表示
  - 前日価格 / MA
  - 当日価格 / MA
  - 差分（PrevDiff / CurrentDiff）
- Buy / Sell シグナルの検証が可能に
- SignalsDataGrid に数値フォーマット（N2）を適用

---

## 概要

- WPF → Python プロセス起動
- stdout の JSON を受け取り → DataGrid 表示
- C#側で MA75 を計算
- C#側でクロスシグナルを検出
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
3. OHLCV + MA75 が表示される
4. シグナル一覧でクロス判定を確認

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

### 判定ロジック

- **Buy**：PrevDiff < 0 → CurrentDiff > 0
- **Sell**：PrevDiff > 0 → CurrentDiff < 0

---

## 今後の予定（v0.5 以降）

- DataGrid 表示改善（列順・見やすさ）
- チャート表示（ローソク足 + MA）
- 複数移動平均（MA25 / MA75 / MA200）
- シグナルフィルタ（直近のみなど）

---

## 補足

- データは `yfinance` を利用
- キャッシュは `tools/fetcher/cache` に保存

