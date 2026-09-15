# PSPSuite

> A cross-platform desktop application for managing, downloading, and transferring music to PSP (PlayStation Portable) devices via USB.

[![.NET](https://img.shields.io/badge/.NET-9.0-blue.svg)](https://dotnet.microsoft.com/)
[![Avalonia](https://img.shields.io/badge/Avalonia-12.1.1-5192dd.svg)](https://avaloniaui.net/)
[![License](https://img.shields.io/badge/license-MIT-green.svg)](LICENSE)
[![Platform](https://img.shields.io/badge/platform-Windows%20%7C%20Linux%20%7C%20macOS-lightgrey.svg)](https://github.com/Brainicism/PSPSuite)

---

## Overview

**PSPSuite** is a desktop application that simplifies the workflow of collecting music from YouTube and local files, then transferring it to a connected PSP device. It provides a unified interface for searching, queuing, and bulk-transferring audio content with automatic metadata embedding, thumbnail extraction, and file normalization.

The app features a modular, plugin-like architecture where each content source (Music, Playlist, Video) is implemented as an independently loadable module, registered and discovered at runtime through dependency injection.

---

## Tech Stack

| Layer | Technology |
|---|---|
| **Runtime** | .NET 9.0 (WinExe) |
| **UI Framework** | Avalonia UI 12.1.1 (Fluent Theme) |
| **MVVM** | CommunityToolkit.Mvvm 8.4.2 |
| **DI Container** | Microsoft.Extensions.DependencyInjection 10.0.12 |
| **Media Processing** | TagLibSharp 2.3.0 (metadata), FFmpeg (bundled) |
| **YouTube Downloading** | YoutubeDLSharp 1.2.0 (yt-dlp wrapper) |
| **Edit Controls** | Avalonia.AvaloniaEdit 12.0.0 |
| **UI Components** | DialogHost.Avalonia 0.12.3, FluentIcons.Avalonia 2.1.337, ItemsRepeater |
| **Git Operations** | LibGit2Sharp 0.32.0 |
| **Runtime Dependencies** | Deno (via yt-dlp plugins), yt-dlp, FFmpeg, FFprobe |
| **System Integration** | System.Management (USB device detection on Windows) |

---

## Key Features

- **YouTube Music Search** — Paste a YouTube URL or playlist link to fetch audio metadata and add tracks to the queue
- **Local Music Import** — Browse and add local audio files (MP3, WAV, WMA, AAC, OGG, FLAC, M4A) with automatic tag extraction (title, artist, album, duration, size)
- **Smart Queue System** — Select individual tracks via checkboxes, bulk-select all, and manage a unified download queue
- **One-Click Transfer** — Send queued items to a connected PSP drive with a single click
- **Auto YouTube Downloading** — Remote (non-local) items are downloaded via yt-dlp with 192kbps MP3 conversion, embedded thumbnails, and ID3 metadata
- **Cookies.txt Support** — Configure a `cookies.txt` file for authenticated YouTube access (premium features, region-restricted content)
- **USB Auto-Detection** — Automatically detects connected PSP drives and populates the target path (`PSP/MUSIC` or `MUSIC/`)
- **Audio Normalization** — Filenames and metadata tags are normalized (Unicode decomposition, diacritic removal, Vietnamese character handling)
- **Activity Log** — Real-time console output piped into an in-app read-only text editor with Fluent styling
- **Modular Architecture** — New modules (Music, Playlist, Video) are registered via `[TabModule]` attributes and auto-discovered by the DI container
- **Hot Reload Support** — Code changes trigger UI rebuilds during development (`#if DEBUG`)
- **Global Error Handling** — Catches unhandled exceptions, async task failures, and UI dispatcher errors with console logging

---

## Project Structure

```
PSPSuite/
├── Program.cs                    # Application entry point, DI configuration, Avalonia bootstrap
├── PSPSuite.csproj               # Project file (target: net9.0, WinExe)
├── app.manifest                  # Windows application manifest (compatibility, DPI)
├── .gitignore
│
├── Attributes/
│   └── TabModuleAttribute.cs     # Custom attribute for declaring tab modules (title + order)
│
├── Data/
│   ├── Audio.cs                  # Audio data model (name, path, duration, artist, album, isLocal)
│   ├── DependencyItem.cs         # Abstract base for runtime dependency items (download + init)
│   └── QueueItem.cs              # Queue item DTO (filename, path, type, status, isLocal)
│
├── Dependencies/
│   ├── Deno.cs                   # Downloads & boots Deno runtime (required by yt-dlp plugins)
│   ├── Ytdlp.cs                  # Downloads FFmpeg, FFprobe, and yt-dlp binaries
│   └── BgutilYtdlpPotProvider.cs # Clones bgutil-ytdlp-pot-provider repo, installs Deno deps, starts POT server
│
├── Helpers/
│   ├── Constants.cs              # App-wide constants (colors, dimensions, enums, file types)
│   ├── DependencyLoader.cs       # Orchestrates dependency download/init/cleanup lifecycle
│   ├── ErrorHandler.cs           # Global exception handlers (app domain, task scheduler, UI dispatcher)
│   ├── FnHelper.cs               # Utility functions (port binding, path ops, URL validation, text normalization)
│   ├── GlobalVar.cs              # Global state holder (e.g., cookies.txt file path)
│   ├── GridExtra.cs              # Extension methods for adding children to Grid with row/col positioning
│   ├── HotReload.cs              # .NET Hot Reload integration (DEBUG only)
│   ├── MessageBox.cs             # Async dialog helpers (error, info, confirm) via DialogHost
│   ├── ModuleLoader.cs           # Extension method to populate TabControl from DI-resolved modules
│   ├── TextWriterExtend.cs       # Redirects Console.Out to a custom action (activity log)
│   └── UsbWatcher.cs             # Detects USB/PSP drive connections (Win32 WMI on Windows, polling on Unix)
│
├── Modules/
│   ├── GenericModule.cs          # Abstract base: UserControl with BuildUI, Init, InitAsync lifecycle
│   ├── Music/
│   │   ├── Music.cs              # Music module logic (local import, single-track search, queue events)
│   │   └── Music.UI.cs           # Music module UI (URL input, search/add buttons, items list)
│   ├── Playlist/
│   │   ├── Playlist.cs           # Playlist module logic (playlist URL search, select-all, queue events)
│   │   └── Playlist.UI.cs        # Playlist module UI (same layout as Music, no local import)
│   └── Video/
│       ├── Video.cs              # Video module (stub)
│       └── Video.UI.cs           # Video module UI (stub)
│
├── Views/
│   ├── Components/
│   │   ├── AudioItemControl.cs   # Custom control: audio row (checkbox, name, artist/album, duration, size)
│   │   ├── QueueItemControl.cs   # Custom control: queue row (name, type pill, status, local/YT badge)
│   │   ├── DividerControl.cs     # Custom control: styled horizontal divider line
│   │   └── MessageBoxControl.cs  # Custom dialog control (icon, title, message, OK/OK+Cancel buttons)
│   └── Windows/
│       ├── GenericWindow.cs      # Abstract base: window with theme, sizing, hot reload, lifecycle hooks
│       └── MainWindow/
│           ├── MainWindow.cs     # MainWindow UI builder (root grid, queue panel, log panel, tab control)
│           └── MainWindow.UI.cs  # MainWindow logic (browse paths, send music/playlist, queue management)
│
└── bin/                          # Build output
└── obj/                          # Intermediate build files
```

---

## Getting Started

### Prerequisites

| Requirement | Minimum Version | Notes |
|---|---|---|
| **.NET SDK** | 9.0.x | Required to build and run; download from [dotnet.microsoft.com](https://dotnet.microsoft.com/download/dotnet/9.0) |
| **IDE (optional)** | Visual Studio 2022 / VS Code / Rider | Visual Studio recommended for full .NET debugging support |
| **Git** | Latest | For cloning the repository |

No other external tools are needed to build — FFmpeg, yt-dlp, and Deno are downloaded automatically at runtime by the dependency loader.

### Installation

```bash
# Clone the repository
git clone https://github.com/Brainicism/PSPSuite.git
cd PSPSuite

# Restore NuGet packages and build
dotnet restore
dotnet build
```

### Environment Setup

PSPSuite does **not** require a `.env` file. All configuration is handled through the UI:

| Setting | How to Configure | Default |
|---|---|---|
| **Target PSP Drive** | Click "Browse" in the main window to select the drive path | Auto-detected via USB watcher |
| **Cookies File** | Click "Browse" next to the Cookies field and select your `cookies.txt` | None (required for authenticated YouTube access) |

The `cookies.txt` file is used by yt-dlp to authenticate with YouTube for premium features and region-restricted content. You can obtain one using browser cookie export tools.

### Build and Run

```bash
# Build in Release mode
dotnet build --configuration Release

# Run the application
dotnet run

# Or run in Release mode
dotnet run --configuration Release
```

The first launch will automatically:

1. Download **FFmpeg**, **FFprobe**, and **yt-dlp** into the application directory
2. Clone and install the **bgutil-ytdlp-pot-provider** plugin server
3. Start the POT (Proof-of-Work) server on a dynamically assigned local port

All dependencies are stored alongside the executable and reused on subsequent launches.

---

## Usage

### Quick Start

1. **Connect your PSP** via USB — the app auto-detects the drive and displays the path
2. **Optionally configure** a `cookies.txt` file for YouTube access
3. **Switch to the Music or Playlist tab** to add content:
   - **Add from YT** — Paste a YouTube video or playlist URL, then search
   - **Add local music** — Browse and select audio files from your computer
4. **Select tracks** using the checkboxes (or "Select all")
5. **Click "Send to queue"** to add selected tracks to the transfer queue
6. **Click "Send"** in the queue panel to transfer everything to your PSP

### Transfer Behavior

| Item Type | Transfer Method |
|---|---|
| **Local files** (MP3/WAV/M4A) | Direct file copy to target drive |
| **Local files** (other formats) | Transcoded via FFmpeg to MP3 (192kbps) with embedded metadata |
| **YouTube tracks/playlist** | Downloaded via yt-dlp → MP3 (192kbps) with embedded thumbnail, metadata, and normalized filenames |

All transferred files are renamed with Unicode-normalized filenames, and ID3 tags (title, artist, album) are cleaned and saved.

---

## Architecture

PSPSuite uses a **dependency-injection-driven modular architecture**:

- **`Program.cs`** scans the entire assembly for types that extend `DependencyItem`, `GenericWindow`, or `GenericModule`, and registers them transiently in the DI container
- **`TabModuleAttribute`** marks module classes with a display title and sort order; `ModuleLoader` reads these attributes to populate the main `TabControl`
- **`DependencyLoader`** orchestrates the download and initialization of runtime dependencies (FFmpeg, yt-dlp, Deno, POT server) in order
- **`GenericModule`** defines the lifecycle: `BuildUI()` → `Init()` → `InitAsync()` — each module implements these to construct its UI and wire up event handlers
- **`UsbWatcher`** uses Win32 WMI events (Windows) or periodic polling (Linux/macOS) to detect PSP drive connections

---

## License

This project is open source. See the [LICENSE](LICENSE) file for details.
