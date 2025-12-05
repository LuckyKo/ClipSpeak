# ClipSpeak

ClipSpeak is a lightweight Windows background application that automatically reads aloud text copied to the clipboard using a Text-to-Speech (TTS) API (compatible with Kokoro-FastAPI and OpenAI-like endpoints).

## Features

*   **Clipboard Monitoring**: Automatically detects text changes in the system clipboard.
*   **TTS Integration**: Sends text to a configurable API endpoint to generate audio.
*   **Audio Playback**: Plays the generated audio stream immediately using `NAudio`.
*   **System Tray Integration**: Runs silently in the background with a system tray icon.
    *   **Context Menu**: Right-click to Stop Speech, Pause Monitoring, access Settings, or Exit.
*   **Settings**:
    *   **API URL**: Configurable TTS server endpoint (default: `http://localhost:8880/v1/audio/speech`).
    *   **Voice Selection**: Fetch available voices from the server and select your preferred one.
    *   **Speed**: Adjust playback speed.
    *   **Volume**: Control playback volume.
    *   **Connection Status**: Visual indicator of server connectivity.

## Requirements

*   Windows OS
*   .NET 9.0 Runtime (or SDK to build)
*   A running TTS server (e.g., [Kokoro-FastAPI](https://github.com/remsky/Kokoro-FastAPI))

## Installation & Usage

1.  **Build the project**:
    ```powershell
    dotnet build
    ```
2.  **Run the application**:
    ```powershell
    dotnet run
    ```
    Or run the executable from the `bin` directory.
3.  **Using the App**:
    *   The app starts in the system tray.
    *   **Right-click** the tray icon to open the menu.
    *   Select **Settings** to configure your TTS server URL and choose a voice.
    *   **Copy text** to your clipboard, and ClipSpeak will read it aloud!

## Configuration

*   **API URL**: Point this to your TTS server's speech endpoint.
    *   For Kokoro-FastAPI, use: `http://localhost:8880/v1/audio/speech`
*   **Voice**: Click the refresh button (↻) to load voices from the server, then select one from the dropdown.

## Technical Details

*   **Language**: C# (Windows Forms)
*   **Audio Library**: [NAudio](https://github.com/naudio/NAudio)
*   **JSON Parsing**: [Newtonsoft.Json](https://github.com/JamesNK/Newtonsoft.Json)
*   **Icon**: Custom application icon converted from PNG.

## License

MIT
