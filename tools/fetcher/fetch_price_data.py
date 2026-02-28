import sys, json, os
from datetime import datetime
import yfinance as yf

def cache_path(symbol: str, period: str) -> str:
    safe = symbol.replace("/", "_")
    return os.path.join("cache", f"{safe}_{period}.json")

def is_cache_valid(path: str) -> bool:
    if not os.path.exists(path):
        return False
    mtime = datetime.fromtimestamp(os.path.getmtime(path))
    return mtime.date() == datetime.now().date()

def main():
    if len(sys.argv) < 3:
        print("Usage: python fetch_price_data.py <SYMBOL> <PERIOD>", file=sys.stderr)
        sys.exit(2)

    symbol = sys.argv[1]
    period = sys.argv[2]

    os.makedirs("cache", exist_ok=True)
    path = cache_path(symbol, period)

    if is_cache_valid(path):
        with open(path, "r", encoding="utf-8") as f:
            print(f.read())
        return

    try:
        t = yf.Ticker(symbol)
        df = t.history(period=period, interval="1d", auto_adjust=False)

        if df is None or df.empty:
            print("No data returned. Symbol may be invalid or delisted.", file=sys.stderr)
            sys.exit(3)

        df = df.reset_index()

        rows = []
        for _, r in df.iterrows():
            d = r["Date"]
            date_str = d.strftime("%Y-%m-%d") if hasattr(d, "strftime") else str(d)[:10]
            rows.append({
                "date": date_str,
                "open": float(r["Open"]),
                "high": float(r["High"]),
                "low": float(r["Low"]),
                "close": float(r["Close"]),
                "volume": int(r["Volume"]) if not (r["Volume"] is None) else 0
            })

        out = json.dumps(rows, ensure_ascii=False)
        with open(path, "w", encoding="utf-8") as f:
            f.write(out)
        print(out)

    except Exception as e:
        print(f"Fetch failed: {e}", file=sys.stderr)
        sys.exit(1)

if __name__ == "__main__":
    main()