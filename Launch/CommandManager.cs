using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Launch
{
    class Command
    {
        public string Name { get; set; }
        public string Path { get; set; }
    }

    class CommandManager
    {
        private List<Command> commands;
        private List<Command> History;
        const int HistorySize = 10;

        public CommandManager()
        {
            commands = new List<Command>();
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
            var found0 = FindHistory(query);
            var found1 = FindWithSubstring(query);
            var found2 = FindWithRegex(query);
            return found0.Concat(found1).Concat(found2).Distinct().ToArray();
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

        private List<Command> FindWithSubstring(string query)
        {
            var found = new List<Command>();
            foreach (var command in commands)
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
            foreach (var command in commands)
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
                            var found = new Command();
                            found.Name = Path.GetFileNameWithoutExtension(item);
                            found.Path = item;
                            commands.Add(found);
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
