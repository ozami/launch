using System;
using System.Collections.Generic;
using System.IO;
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

        public CommandManager()
        {
            commands = new List<Command>();
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
            var chars = new List<string>();
            foreach (var c in query)
            {
                chars.Add(Regex.Escape(c.ToString()));
            }
            var pattern = string.Join(".*", chars);

            var rx = new Regex(pattern, RegexOptions.IgnoreCase);
            var found = new List<Command>();
            foreach (var shortcut in commands)
            {
                if (rx.IsMatch(shortcut.Name))
                {
                    found.Add(shortcut);
                }
            }
            return found.ToArray();
        }

        public void Launch(Command command)
        {
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
