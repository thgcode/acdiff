# Repository guidance

## Project

Acdiff is a keyboard-accessible Windows Forms viewer for Git and unified diffs.
It uses .NET Framework 4 and the framework's C# compiler directly, with no SDK,
NuGet dependencies, solution, or project file.

- `Acdiff.cs`: Windows Forms interface, keyboard navigation, search, sound cues,
  and accessibility announcements.
- `Diff.cs`: diff parsing, display text, line metadata, and change blocks.
- `ParserTests.cs`: standalone parser regression checks.
- `build.cmd`: compiles the AnyCPU Windows application into `Acdiff.exe`.
- `test.cmd`: compiles and runs `ParserTests.exe`.

## Build and validation

From PowerShell on Windows, run:

```powershell
.\build.cmd
.\test.cmd
```

The scripts use the .NET Framework compiler under `%WINDIR%`, preferring the
64-bit framework directory and falling back to the 32-bit directory. Keep code
compatible with that compiler and .NET Framework 4; avoid newer C# syntax and
APIs that require a newer runtime. Executables and PDBs are ignored build outputs.

For UI changes, also check opening a patch, clipboard loading, file switching,
F2/Shift+F2 change navigation, search in both directions, and caret status updates.
Screen reader announcements require an interactive check with NVDA or JAWS;
parser tests cannot verify them.

## Code style

Use four spaces for indentation and put opening braces on their own lines.
Keep individual statements on separate lines. Follow the existing naming:
PascalCase for types and methods, camelCase for local variables and private fields.
Preserve the Windows CRLF line endings in C# and command files. Keep formatting
changes separate from behavioral changes when practical.

## Behavior to preserve

- Strip exactly one diff marker from source lines. Preserve source spaces, tabs,
  and literal plus/minus characters, including whitespace in test fixtures.
- Store change kind and old/new line numbers separately from displayed text.
- Keep Git headers, hunk headings, binary notices, and no-newline notices visible
  as diff information. Binary contents and combined merge diffs are not rendered
  as source changes.
- Group adjacent additions and deletions into one change block; context lines
  and new hunks separate blocks.
- Preserve keyboard access, accessible control names, status announcements, and
  configurable sound cues when modifying the interface.
- Update parser regression checks for parsing behavior changes and update
  `README.md` when user-visible behavior or shortcuts change.
