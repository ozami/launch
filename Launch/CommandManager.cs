using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Media.Imaging;

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
            for (var i = History.Count - 1; i >= 0; --i)
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
                            var found = new Command {
                                Name = Path.GetFileNameWithoutExtension(item),
                                Path = item,
                                Icon = IconUtility.GetFileIcon(item)
                            };
                            Commands.Add(found);
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
