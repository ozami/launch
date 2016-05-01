using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.IO;
using System.Text.RegularExpressions;

namespace Launch
{
    public class StartMenuItem
    {
        public string name;
        public string path;
    }

    /// <summary>
    /// CommandWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class CommandWindow : Window
    {
        private List<StartMenuItem> shortcuts;

        public CommandWindow()
        {
            InitializeComponent();
            CacheStartMenuItems();
            Activated += CommandWindow_Activated;
            commandInput.KeyDown += CommandInput_KeyDown;
        }

        private void CacheStartMenuItems()
        {
            shortcuts = new List<StartMenuItem>();
            Environment.SpecialFolder[] menus = {
                Environment.SpecialFolder.StartMenu,
                Environment.SpecialFolder.CommonStartMenu
            };
            foreach (var menu in menus)
            {
                ListShortcuts(Environment.GetFolderPath(menu));
            }
        }

        private void ListShortcuts(string path)
        {
            try
            {
                foreach (var item in Directory.EnumerateFileSystemEntries(path))
                {
                    var attr = File.GetAttributes(item);
                    if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                    {
                        ListShortcuts(item);
                    }
                    else
                    {
                        if (Path.GetExtension(item) == ".lnk")
                        {
                            var found = new StartMenuItem();
                            found.name = Path.GetFileNameWithoutExtension(item);
                            found.path = item;
                            shortcuts.Add(found);
                        }
                    }
                }
            }
            catch (System.UnauthorizedAccessException)
            {
            }
        }

        private void CommandWindow_Activated(object sender, EventArgs e)
        {
            commandInput.Text = "";
            commandInput.Focus();
        }

        private void CommandInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Hide();
                return;
            }
            if (e.Key == Key.Enter)
            {
                Hide();
                var items = findCommand(commandInput.Text);
                if (items.Length > 0)
                {
                    Launch(items[0]);
                }
            }
        }

        private StartMenuItem[] findCommand(string command)
        {
            var chars = new List<string>();
            foreach (var c in command)
            {
                chars.Add(Regex.Escape(c.ToString()));
            }
            var pattern = string.Join(".*", chars);

            var rx = new Regex(pattern, RegexOptions.IgnoreCase);
            var found = new List<StartMenuItem>();
            foreach (var shortcut in shortcuts)
            {
                if (rx.IsMatch(shortcut.name))
                {
                    found.Add(shortcut);
                }
            }
            return found.ToArray();
        }

        private void Launch(StartMenuItem item)
        {
            System.Diagnostics.Process.Start(item.path);
        }
    }
}
