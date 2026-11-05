using System;
using System.Collections.Generic;
using System.Text;

namespace Acdiff
{
    public enum ChangeKind { Context, Addition, Deletion, Header }

    public sealed class DiffLine
    {
        public string Text;
        public ChangeKind Kind;
        public int OldNumber;
        public int NewNumber;
    }

    public sealed class DiffFile
    {
        public string Name;
        public readonly List<DiffLine> Lines = new List<DiffLine>();
        public readonly List<int> Blocks = new List<int>();
        public override string ToString() { return Name; }
        public string DisplayText()
        {
            StringBuilder result = new StringBuilder();
            foreach (DiffLine line in Lines) result.Append(line.Text).Append("\r\n");
            return result.ToString();
        }
    }

    public static class DiffParser
    {
        public static List<DiffFile> Parse(string text)
        {
            List<DiffFile> files = new List<DiffFile>();
            DiffFile file = null;
            bool inHunk = false, inBlock = false;
            int oldLine = 0, newLine = 0, oldRemaining = 0, newRemaining = 0;
            string[] lines = text.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                string raw = lines[i];
                if (i == lines.Length - 1 && raw.Length == 0) break;
                if (raw.StartsWith("diff --git ", StringComparison.Ordinal))
                {
                    file = new DiffFile();
                    file.Name = raw.Substring(11);
                    files.Add(file);
                    inHunk = false; inBlock = false;
                }
                else if (!inHunk && raw.StartsWith("--- ", StringComparison.Ordinal) &&
                    i + 1 < lines.Length && lines[i + 1].StartsWith("+++ ", StringComparison.Ordinal))
                {
                    if (file == null || file.Lines.Exists(delegate(DiffLine l) { return l.Text.StartsWith("+++ ", StringComparison.Ordinal); }))
                    {
                        file = new DiffFile(); files.Add(file);
                    }
                    string path = lines[i + 1].Substring(4).Split('\t')[0];
                    if (path == "/dev/null") path = raw.Substring(4).Split('\t')[0];
                    file.Name = path.StartsWith("a/") || path.StartsWith("b/") ? path.Substring(2) : path;
                }
                if (file == null)
                {
                    file = new DiffFile(); file.Name = "Diff"; files.Add(file);
                }
                if (raw.StartsWith("@@ ", StringComparison.Ordinal))
                {
                    string[] parts = raw.Split(' ');
                    inHunk = parts.Length >= 4 && ReadRange(parts[1], '-', out oldLine, out oldRemaining)
                        && ReadRange(parts[2], '+', out newLine, out newRemaining);
                    inBlock = false;
                    file.Lines.Add(new DiffLine { Text = raw, Kind = ChangeKind.Header });
                    continue;
                }
                DiffLine entry = new DiffLine { Text = raw, Kind = ChangeKind.Header };
                if (inHunk && raw.Length > 0)
                {
                    char prefix = raw[0];
                    if (prefix == '+' || prefix == '-' || prefix == ' ')
                    {
                        entry.Text = raw.Substring(1);
                        entry.Kind = prefix == '+' ? ChangeKind.Addition : prefix == '-' ? ChangeKind.Deletion : ChangeKind.Context;
                        if (prefix != '+') { entry.OldNumber = oldLine++; oldRemaining--; }
                        if (prefix != '-') { entry.NewNumber = newLine++; newRemaining--; }
                        bool changed = prefix != ' ';
                        if (changed && !inBlock) file.Blocks.Add(file.Lines.Count);
                        inBlock = changed;
                    }
                    else if (prefix != '\\') { inHunk = false; inBlock = false; }
                }
                file.Lines.Add(entry);
                if (inHunk && oldRemaining <= 0 && newRemaining <= 0) inHunk = false;
            }
            return files;
        }

        private static bool ReadRange(string value, char prefix, out int start, out int count)
        {
            start = 0; count = 1;
            if (value.Length < 2 || value[0] != prefix) return false;
            string[] parts = value.Substring(1).Split(',');
            return parts.Length <= 2 && int.TryParse(parts[0], out start) && start >= 0
                && (parts.Length == 1 || (int.TryParse(parts[1], out count) && count >= 0));
        }
    }
}
