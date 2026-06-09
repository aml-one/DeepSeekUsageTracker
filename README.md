<p align="center">
    <img src="https://github.com/aml-one/DeepSeekUsageTracker/blob/master/Images/deepseek.png?raw=true" width="96" alt="DeepSeek Usage Tracker">
</p>

<h1 align="center">DeepSeek Usage Tracker</h1>

<p align="center">
  A tiny desktop widget that shows your DeepSeek API balance at a glance.<br>
  Always on top. Dark themed. Updates automatically.
</p>

<p align="center">
  <img src="https://img.shields.io/badge/platform-Windows%2010%2B-blue?style=flat-square" alt="Platform">
  <img src="https://img.shields.io/badge/.NET-10.0-blueviolet?style=flat-square" alt=".NET">
  <img src="https://img.shields.io/badge/license-MIT-green?style=flat-square" alt="License">
</p>

---

## What is this?

A lightweight **240×140 px** WPF widget that sits in the corner of your screen and periodically checks your DeepSeek API balance using the official [`/user/balance`](https://api-docs.deepseek.com/api/get-user-balance) endpoint. No bloat, no browser tab — just your remaining balance or usage percentage, always visible.

### Features

- **Two display modes** — toggle between balance ($) and usage percentage with an iOS-style switch
- **Balance mode** — shows remaining funds in your DeepSeek account
- **Percentage mode** — set your topped-up amount, see a yellow progress bar filling up as you spend
- **Live API fetch** — calls the DeepSeek API directly with your key
- **Automatic refresh** — configurable from 1m to 60m (default 10m)
- **Countdown timer** — shows `04:32` or `56s` until next refresh
- **Double-click refresh** — tap the balance to update immediately
- **Always on top** — stays visible over other windows
- **Borderless & draggable** — click anywhere to move it around
- **Dark theme** — custom-styled controls, radial gradient background, no Windows chrome
- **Status indicator** — green dot when connected, red on errors or invalid key
- **Settings popup** — click the ⚙ gear to configure API key, display mode, topped-up amount, and refresh interval
- **Portable** — settings stored in `%ProgramData%\DeepSeekUsageTracker\`

### Screenshot

<p align="center">
  <img src="https://github.com/aml-one/DeepSeekUsageTracker/blob/master/widget.png?raw=true" width="240" alt="Widget screenshot">
</p>

> Balance mode (left) and percentage mode (right) with progress bar.

---

## Download

Grab the latest build from the [Executable](../Executable/) folder:

<p align="center">
  <a href="Executable/DeepSeekUsageTracker.exe">
    <strong>⬇ Download DeepSeekUsageTracker.exe</strong>
  </a>
</p>

Just double-click to run. No installer needed.

> **Windows may show a SmartScreen warning** on first launch — click "More info" → "Run anyway".

---

## How to use

### 1. Get your API key

Go to [platform.deepseek.com/api_keys](https://platform.deepseek.com/api_keys) and create or copy your key. It starts with `sk-`.

### 2. Configure the widget

- Click the **⚙** gear icon in the bottom-right corner
- Paste your API key into the password field
- Choose a **display mode**:
  - **Balance** — shows remaining funds in USD
  - **Percentage** — shows used % with a progress bar (requires setting your topped-up amount)
- Choose a refresh interval (`1m` / `5m` / `10m` / `30m` / `60m`)
- Click **Save**

The widget immediately fetches your balance and begins counting down to the next refresh.

### 3. That's it

| Action | How |
|--------|-----|
| Move the widget | Click & drag anywhere |
| Refresh now | Double-click the balance card |
| Change settings | Click ⚙ |
| Switch display mode | Settings → iOS toggle |
| Close the widget | Alt+F4 or right-click taskbar → Close |

---

## Build from source

```bash
git clone https://github.com/your-org/deepseek-usage.git
cd deepseek-usage/DeepSeekUsageTracker
dotnet run
```

Requires [.NET 10 SDK](https://dotnet.microsoft.com/download).

To produce a single-file executable:

```bash
dotnet publish -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true -o ../Executable
```

---

## Tech stack

| Layer | Technology |
|-------|-----------|
| UI | WPF (Windows Presentation Foundation) |
| API | `GET https://api.deepseek.com/user/balance` |
| Config | JSON in `%ProgramData%\DeepSeekUsageTracker\` |
| Timer | `DispatcherTimer` with per-second countdown |

---

## License

MIT — use it, fork it, ship it.

---

<p align="center">
  <sub>Made by <strong>AmL</strong> · 2026</sub>
</p>
