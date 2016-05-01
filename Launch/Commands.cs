using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace Launch
{
    class Shortcut
    {
        public string name;
        public string path;
    }

    class Commands
    {
        private List<Shortcut> shortcuts;

        public Commands()
        {
            shortcuts = new List<Shortcut>();
            Environment.SpecialFolder[] menus = {
                Environment.SpecialFolder.StartMenu,
                Environment.SpecialFolder.CommonStartMenu
            };
            foreach (var menu in menus)
            {
                ListShortcuts(Environment.GetFolderPath(menu));
            }
        }

        public Shortcut[] findShortcut(string query)
        {
            var chars = new List<string>();
            foreach (var c in query)
            {
                chars.Add(Regex.Escape(c.ToString()));
            }
            var pattern = string.Join(".*", chars);

            var rx = new Regex(pattern, RegexOptions.IgnoreCase);
            var found = new List<Shortcut>();
            foreach (var shortcut in shortcuts)
            {
                if (rx.IsMatch(shortcut.name))
                {
                    found.Add(shortcut);
                }
            }
            return found.ToArray();
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
                            var found = new Shortcut();
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
    }
}
