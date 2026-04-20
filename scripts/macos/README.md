# macOS Auto-Start (launchd)

এটা লোকাল ডেভেলপমেন্টে chat / frontend বার বার হাতে চালু করার ঝামেলা কমায়।

## Install (একবারই)

```bash
chmod +x scripts/macos/install-launchd.sh scripts/macos/uninstall-launchd.sh
./scripts/macos/install-launchd.sh
```

এবার আপনি Mac restart/login করার পরেও automatically চলবে:
- Frontend: `http://localhost:8000/assets/html/index.html`
- Chat health: `http://localhost:5001/api/chat/health`

## Uninstall

```bash
./scripts/macos/uninstall-launchd.sh
```

## Logs

`~/Library/Logs/mrshop/`

## Note (iCloud/Desktop)

কিছু Mac-এ (বিশেষ করে iCloud-synced Desktop) `.venv`-এ `com.apple.quarantine` xattr থাকলে launchd থেকে Python run করতে গিয়ে সমস্যা হতে পারে।
`install-launchd.sh` এখন best-effort হিসেবে `.venv` থেকে quarantine xattr remove করে।
