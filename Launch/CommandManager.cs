using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Media.Imaging;
using Wsh = IWshRuntimeLibrary;

namespace Launch
{
    class Command
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public BitmapSource Icon { get; set; }
    }

    class CommandManager
    {
        private List<Command> Commands;
        private List<Command> History;
        const int HistorySize = 10;

        public CommandManager()
        {
            Commands = new List<Command>();
            History = new List<Command>();
            Environment.SpecialFolder[] menus = {
                Environment.SpecialFolder.StartMenu,
                Environment.SpecialFolder.CommonStartMenu
            };
            foreach (var menu in menus)
            {
                MakeCache(Environment.GetFolderPath(menu));
            }
        }

        public Command[] Find(string query)
        {
            if (query == "")
            {
                return new Command[] {};
            }
            var found = FindHistory(query);
            found.AddRange(FindBegining(query));
            found.AddRange(FindWithSubstring(query));
            found.AddRange(FindWithRegex(query));
            return found.Distinct().ToArray();
        }

        private List<Command> FindHistory(string query)
        {
            var found = new List<Command>();
            for (var i = 0; i < History.Count; ++i)
            {
                var command = History[i];
                if (command.Name.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    found.Add(command);
                }
            }
            return found;
        }

        private List<Command> FindBegining(string query)
        {
            var found = new List<Command>();
            foreach (var command in Commands)
            {
                if (command.Name.IndexOf(query, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    found.Add(command);
                }
            }
            return found;
        }

        private List<Command> FindWithSubstring(string query)
        {
            var found = new List<Command>();
            foreach (var command in Commands)
            {
                if (command.Name.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    found.Add(command);
                }
            }
            return found;
        }

        private List<Command> FindWithRegex(string query)
        {
            var chars = new List<string>();
            foreach (var c in query)
            {
                chars.Add(Regex.Escape(c.ToString()));
            }
            var pattern = string.Join(".*", chars);

            var rx = new Regex(pattern, RegexOptions.IgnoreCase);
            var found = new List<Command>();
            foreach (var command in Commands)
            {
                if (rx.IsMatch(command.Name))
                {
                    found.Add(command);
                }
            }
            return found;
        }

        public void Launch(Command command)
        {
            History.Remove(command);
            History.Add(command);
            if (History.Count > HistorySize)
            {
                History = History.GetRange(1, HistorySize);
            }
            System.Diagnostics.Process.Start(command.Path);
        }

        private void MakeCache(string dir)
        {
            try
            {
                foreach (var item in Directory.EnumerateFileSystemEntries(dir))
                {
                    var attr = File.GetAttributes(item);
                    if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                    {
                        MakeCache(item);
                    }
                    else
                    {
                        if (Path.GetExtension(item) == ".lnk")
                        {
                            try
                            {
                                var found = new Command
                                {
                                    Name = Path.GetFileNameWithoutExtension(item),
                                    Path = item,
                                    Icon = GetIcon(item)
                                };
                                Commands.Add(found);
                            }
                            catch (FileNotFoundException)
                            {

                            }
                        }
                    }
                }
            }
            catch (System.UnauthorizedAccessException)
            {
            }
        }

        private static BitmapSource GetIcon(string path)
        {
            var shell = new Wsh.IWshShell_Class();
            var shortcut = (Wsh.IWshShortcut_Class)shell.CreateShortcut(path);
            var exePath = shortcut.IconLocation;
            var iconIndex = 0;
            var pos = exePath.LastIndexOf(",");
            if (pos != -1)
            {
                iconIndex = int.Parse(exePath.Substring(pos + 1));
                exePath = exePath.Substring(0, pos);
            }
            Icon icon;
            try
            {
                icon = IconUtility.GetExeIcon(exePath, iconIndex);
            }
            catch (Exception)
            {
                icon = Icon.ExtractAssociatedIcon(path);
            }
            var iconSrc = System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(
                icon.Handle,
                System.Windows.Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions()
            );
            return iconSrc;
        }
    }
}
