# StockAnalysis (v0.3)

WPFアプリからPython（yfinance）を起動し、株価データ（OHLCV）を取得し、
75日単純移動平均（MA75）とクロスシグナルを表示するアプリケーションです。

---

## v0.3 変更点

- ゴールデンクロス / デッドクロス検出を追加
- シグナル一覧を DataGrid で表示
- `SignalType` enum により Buy / Sell を型安全に管理
- シグナル一覧の価格・MA表示を小数2桁に統一

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

```bash
git clone https://github.com/yama-o824/StockAnalysis.git
cd StockAnalysis
```

### 2. Python（fetcher）のセットアップ

```bash
cd tools/fetcher
python -m venv .venv
.\.venv\Scripts\python.exe -m pip install -r requirements.txt
```

> python が見つからない場合は、PythonをインストールしてPATHを通してください。

### 3. WPFアプリを起動

Visual Studioで以下のプロジェクトを開いて実行してください。

```text
src/StockAnalyzer
```

---

## 使い方

1. 銘柄コードを入力（例：7203.T）
2. 「取得」ボタンを押す
3. OHLCVおよびMA75が表示されます
4. 条件に一致したクロスシグナルが下部の一覧に表示されます

- 期間は v0.3 では固定（例：1y）
- キャッシュは `tools/fetcher/cache` に保存されます（当日中は再取得しません）

---

## クロス判定ルール

- **Buy**: 前日まで終値がMA75を下回り、当日終値がMA75を上回ったとき
- **Sell**: 前日まで終値がMA75を上回り、当日終値がMA75を下回ったとき

※ MA75 が `null` の期間はシグナル判定を行いません。

---

## 今後の予定（v0.4 以降）

- チャート表示（折れ線）
- シグナルの根拠表示強化
- 指標期間の可変化
- Python取得部のexe化
