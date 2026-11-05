using System;
using System.Collections.Generic;
using Acdiff;

internal static class ParserTests
{
    private static void Check(bool condition, string message)
    {
        if (!condition) throw new Exception(message);
    }

    private static void Main()
    {
        List<DiffFile> files = DiffParser.Parse("diff --git a/a.cs b/a.cs\n--- a/a.cs\n+++ b/a.cs\n@@ -1,3 +1,4 @@\n context\n-    old\n+    new\n+\tadded\n tail\ndiff --git a/b b/b\n--- a/b\n+++ b/b\n@@ -0,0 +1 @@\n+hello\n");
        Check(files.Count == 2 && files[0].Name == "a.cs", "File separation");
        DiffFile first = files[0];
        Check(first.Blocks.Count == 1, "Adjacent deletions and additions share a block");
        int block = first.Blocks[0];
        Check(first.Lines[block].Text == "    old", "Preserve spaces");
        Check(first.Lines[block + 2].Text == "\tadded", "Preserve tabs");
        Check(first.Lines[block].Kind == ChangeKind.Deletion && first.Lines[block].OldNumber == 2, "Deletion metadata");
        Check(first.Lines[block + 1].Kind == ChangeKind.Addition && first.Lines[block + 1].NewNumber == 2, "Addition metadata");
        Check(files[1].Lines[files[1].Blocks[0]].NewNumber == 1, "Zero length old range");
        files = DiffParser.Parse("--- a/one\n+++ b/one\n@@ -1 +1 @@\n---literal\n+++literal\n--- a/two\n+++ b/two\n@@ -1 +1 @@\n-x\n+y\n");
        Check(files.Count == 2, "Plain unified file separation");
        Check(files[0].Lines[3].Text == "--literal" && files[0].Lines[4].Text == "++literal", "Strip exactly one marker");
        files = DiffParser.Parse("@@ -1,3 +1,3 @@\n-a\n+b\n context\n-c\n+d\n\\ No newline at end of file\n");
        Check(files[0].Blocks.Count == 2, "Context separates blocks");
        Check(DiffParser.Parse("").Count == 0, "Empty input");
        files = DiffParser.Parse("diff --git a/image b/image\nBinary files a/image and b/image differ\n");
        Check(files.Count == 1 && files[0].Blocks.Count == 0, "Binary metadata");
        Console.WriteLine("All parser tests passed.");
    }
}
