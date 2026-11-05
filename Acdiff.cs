using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Media;
using System.Windows.Forms;

namespace Acdiff
{
    internal sealed class LiveStatus : Label
    {
        public void Announce(string message)
        {
            Text = message;
            AccessibleName = message;
            AccessibilityNotifyClients(AccessibleEvents.NameChange, -1);
        }
    }

    public sealed class MainForm : Form
    {
        private readonly ListBox files = new ListBox();
        private readonly RichTextBox editor = new RichTextBox();
        private readonly LiveStatus status = new LiveStatus();
        private readonly ToolStripMenuItem soundItem = new ToolStripMenuItem("&Sound cues");
        private List<DiffFile> document = new List<DiffFile>();
        private int lastLine = -1;
        private string query = "";

        public MainForm()
        {
            Text = "Acdiff";
            Width = 1050;
            Height = 720;
            StartPosition = FormStartPosition.CenterScreen;
            MenuStrip menu = new MenuStrip();
            ToolStripMenuItem fileMenu = new ToolStripMenuItem("&File");
            fileMenu.DropDownItems.Add("&Open diff...", null, delegate
            {
                Open();
            });
            fileMenu.DropDownItems.Add("Load from &clipboard", null, delegate
            {
                PasteDiff();
            });
            fileMenu.DropDownItems.Add("E&xit", null, delegate
            {
                Close();
            });
            ToolStripMenuItem navigate = new ToolStripMenuItem("&Navigate");
            navigate.DropDownItems.Add("&Next change (F2)", null, delegate
            {
                MoveBlock(1);
            });
            navigate.DropDownItems.Add("&Previous change (Shift+F2)", null, delegate
            {
                MoveBlock(-1);
            });
            navigate.DropDownItems.Add("&Find (Ctrl+F)", null, delegate
            {
                FindDialog();
            });
            soundItem.Checked = true;
            soundItem.CheckOnClick = true;
            menu.Items.Add(fileMenu);
            menu.Items.Add(navigate);
            menu.Items.Add(soundItem);
            MainMenuStrip = menu;
            SplitContainer split = new SplitContainer();
            split.Dock = DockStyle.Fill;
            split.SplitterDistance = 220;
            files.Dock = DockStyle.Fill;
            files.AccessibleName = "Files in diff";
            files.SelectedIndexChanged += delegate
            {
                ShowFile();
            };
            editor.Dock = DockStyle.Fill;
            editor.ReadOnly = true;
            editor.WordWrap = false;
            editor.DetectUrls = false;
            editor.HideSelection = false;
            editor.Font = new Font(FontFamily.GenericMonospace, 11);
            editor.AccessibleName = "Diff content";
            editor.AccessibleDescription = "Read only. F2 moves to the next change; Shift F2 to the previous change. Control F searches.";
            editor.SelectionChanged += delegate
            {
                DescribeLine();
            };
            split.Panel1.Controls.Add(files);
            split.Panel2.Controls.Add(editor);
            status.Dock = DockStyle.Bottom;
            status.Height = 42;
            status.TextAlign = ContentAlignment.MiddleLeft;
            status.AccessibleRole = AccessibleRole.StatusBar;
            Controls.Add(split);
            Controls.Add(status);
            Controls.Add(menu);
            status.Announce("Open a diff with Ctrl+O, or load the clipboard with Ctrl+Shift+V.");
        }

        public void LoadPath(string path)
        {
            try
            {
                LoadDiff(File.ReadAllText(path));
                Text = "Acdiff - " + Path.GetFileName(path);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Cannot open diff", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadDiff(string text)
        {
            document = DiffParser.Parse(text);
            files.Items.Clear();
            foreach (DiffFile file in document) files.Items.Add(file);
            if (files.Items.Count > 0) files.SelectedIndex = 0;
            else
            {
                editor.Clear();
                status.Announce("The diff is empty.");
            }
            editor.Focus();
        }

        private void Open()
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Filter = "Diff and patch files|*.diff;*.patch|All files|*.*";
                if (dialog.ShowDialog(this) == DialogResult.OK) LoadPath(dialog.FileName);
            }
        }

        private void PasteDiff()
        {
            try
            {
                if (Clipboard.ContainsText())
                {
                    LoadDiff(Clipboard.GetText());
                    Text = "Acdiff - Clipboard";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Cannot read clipboard");
            }
        }

        private DiffFile Current
        {
            get
            {
                return files.SelectedItem as DiffFile;
            }
        }

        private void ShowFile()
        {
            lastLine = -1;
            editor.Text = Current == null ? "" : Current.DisplayText();
            editor.SelectionStart = 0;
            DescribeLine();
        }

        private void DescribeLine()
        {
            DiffFile file = Current;
            if (file == null) return;
            int index = editor.GetLineFromCharIndex(editor.SelectionStart);
            if (index == lastLine) return;
            lastLine = index;
            if (index >= file.Lines.Count)
            {
                status.Announce("End of " + file.Name);
                return;
            }
            DiffLine line = file.Lines[index];
            string kind = line.Kind == ChangeKind.Addition ? "Addition" : line.Kind == ChangeKind.Deletion ? "Deletion" : line.Kind == ChangeKind.Context ? "Context" : "Diff information";
            string location = line.OldNumber > 0 ? ", old line " + line.OldNumber : "";
            if (line.NewNumber > 0) location += ", new line " + line.NewNumber;
            status.Announce(kind + location + ", " + file.Name);
            if (soundItem.Checked)
            {
                if (line.Kind == ChangeKind.Addition) SystemSounds.Asterisk.Play();
                if (line.Kind == ChangeKind.Deletion) SystemSounds.Exclamation.Play();
            }
        }

        private void MoveBlock(int direction)
        {
            DiffFile file = Current;
            if (file == null) return;
            int current = editor.GetLineFromCharIndex(editor.SelectionStart);
            if (direction > 0)
            {
                foreach (int line in file.Blocks) if (line > current)
                {
                    GoTo(line);
                    return;
                }
            }
            else for (int i = file.Blocks.Count - 1; i >= 0; i--) if (file.Blocks[i] < current)
            {
                GoTo(file.Blocks[i]);
                return;
            }
            status.Announce(direction > 0 ? "No next change in this file." : "No previous change in this file.");
        }

        private void GoTo(int line)
        {
            editor.Select(editor.GetFirstCharIndexFromLine(line), 0);
            editor.ScrollToCaret();
            editor.Focus();
        }

        private void FindDialog()
        {
            using (Form dialog = new Form())
            {
                dialog.Text = "Find in current file";
                dialog.Width = 420;
                dialog.Height = 155;
                dialog.FormBorderStyle = FormBorderStyle.FixedDialog;
                dialog.StartPosition = FormStartPosition.CenterParent;
                dialog.MinimizeBox = false;
                dialog.MaximizeBox = false;
                Label label = new Label
                {
                    Text = "&Find text:",
                    Left = 12,
                    Top = 12,
                    AutoSize = true
                };
                TextBox input = new TextBox
                {
                    Left = 12,
                    Top = 35,
                    Width = 375,
                    Text = query,
                    AccessibleName = "Find text"
                };
                Button find = new Button
                {
                    Text = "Find next",
                    Left = 202,
                    Top = 70,
                    Width = 90,
                    DialogResult = DialogResult.OK
                };
                Button cancel = new Button
                {
                    Text = "Cancel",
                    Left = 297,
                    Top = 70,
                    DialogResult = DialogResult.Cancel
                };
                dialog.Controls.AddRange(new Control[]
                {
                    label,
                    input,
                    find,
                    cancel
                });
                dialog.AcceptButton = find;
                dialog.CancelButton = cancel;
                dialog.ActiveControl = input;
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    query = input.Text;
                    Search(false);
                }
            }
        }

        private void Search(bool backwards)
        {
            if (query.Length == 0)
            {
                FindDialog();
                return;
            }
            string text = editor.Text;
            int start = editor.SelectionStart;
            int found;
            if (backwards)
            {
                found = start > 0 ? text.LastIndexOf(query, start - 1, StringComparison.OrdinalIgnoreCase) : -1;
                if (found < 0) found = text.LastIndexOf(query, StringComparison.OrdinalIgnoreCase);
            }
            else
            {
                int next = Math.Min(text.Length, start + Math.Max(1, editor.SelectionLength));
                found = text.IndexOf(query, next, StringComparison.OrdinalIgnoreCase);
                if (found < 0) found = text.IndexOf(query, StringComparison.OrdinalIgnoreCase);
            }
            if (found < 0)
            {
                status.Announce("Text not found: " + query);
                return;
            }
            editor.Select(found, query.Length);
            editor.ScrollToCaret();
            editor.Focus();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Control | Keys.O:
                    Open();
                    return true;
                case Keys.Control | Keys.Shift | Keys.V:
                    PasteDiff();
                    return true;
                case Keys.F2:
                    MoveBlock(1);
                    return true;
                case Keys.Shift | Keys.F2:
                    MoveBlock(-1);
                    return true;
                case Keys.Control | Keys.F:
                    FindDialog();
                    return true;
                case Keys.F3:
                    Search(false);
                    return true;
                case Keys.Shift | Keys.F3:
                    Search(true);
                    return true;
                case Keys.F6:
                    if (editor.Focused) files.Focus();
                    else editor.Focus();
                    return true;
                case Keys.Control | Keys.PageDown:
                    if (files.SelectedIndex + 1 < files.Items.Count) files.SelectedIndex++;
                    return true;
                case Keys.Control | Keys.PageUp:
                    if (files.SelectedIndex > 0) files.SelectedIndex--;
                    return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }
    }

    internal static class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            MainForm form = new MainForm();
            if (args.Length > 0) form.Shown += delegate
            {
                form.LoadPath(args[0]);
            };
            Application.Run(form);
        }
    }
}
