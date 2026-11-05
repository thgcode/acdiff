# Acdiff

A small, keyboard accessible Windows application for reading Git and unified diffs. The read only text view removes exactly one diff marker from source lines, preserving spaces, tabs, and literal plus/minus characters in the code. Change types and old/new line numbers are stored separately from the displayed text.

## Build and run

Requires Windows with .NET Framework 4 or later. No NuGet packages, SDK, or installer are needed. Run `build.cmd`, then launch `Acdiff.exe`. You can also pass a diff path: `Acdiff.exe "C:\work\changes.patch"`. The executable can be copied by itself to another Windows computer with the framework installed.

The build script uses `%WINDIR%\Microsoft.NET\Framework64\v4.0.30319\csc.exe`, falling back to the 32 bit framework compiler. It builds an AnyCPU Windows executable.

Generate a patch with `git diff --no-color > changes.diff`, open an existing patch, or load plain text from the clipboard. Files are read as UTF-8 with BOM detection. Git headers, hunk headings, binary notices, and no-newline notices remain visible as diff information. Binary contents and combined merge diffs are not rendered as source changes.

## Keyboard

| Shortcut | Action |
| --- | --- |
| Ctrl+O | Open a diff or patch |
| Ctrl+Shift+V | Load the clipboard as a new diff |
| F6 | Switch between the file list and diff text |
| Up/Down in file list | Select a file |
| Ctrl+PageDown / Ctrl+PageUp | Next / previous file |
| F2 / Shift+F2 | Next / previous change block in the current file |
| Ctrl+F | Search the current file, ignoring case |
| F3 / Shift+F3 | Find next / previous occurrence, wrapping at the ends |
| Ctrl+C | Copy selected displayed text |

A change block is a consecutive run of additions and deletions. Context or a new hunk separates blocks. Navigation stops at the first/last block and reports that boundary in the status area. Standard text navigation, selection, and menu access keys also work.

## Accessibility

The file list, text view, and search field have accessible names. Moving the caret onto a different line updates the status area with Added, Deleted, Context, or Diff information, plus available line numbers and filename. The status area is a polite UI Automation live region and raises LiveRegionChanged events for status updates, including added and deleted lines, navigation boundaries, and unsuccessful searches. Live region announcements require Windows 8 or later and a screen reader with UI Automation live region support. These announcements remain enabled when Sound cues is turned off. The text itself remains free of spoken change prefixes so its indentation can be read using the screen reader's normal settings.

Additions play the Windows Asterisk sound; deletions play Exclamation. Toggle these through the Sound cues menu. Sounds depend on the Windows sound scheme and may be silent or identical in a customized scheme. Screen reader announcement behavior depends on its UI Automation and live region settings. Validate with your preferred screen reader and indentation settings before relying on announcements.

## Verification

Run `test.cmd` for parser regression checks covering multiple Git/unified files, indentation, literal markers, line numbers, zero length ranges, block boundaries, empty input, and binary notices.

For an interactive check, open a patch with additions and deletions, move through it with arrow keys, and check the status and sound on each changed line. Use F2/Shift+F2, switch files, search in both directions, and verify status announcements with NVDA or JAWS. The parser tests do not validate screen reader output.

## License

Licensed under the [MIT License](LICENSE).
