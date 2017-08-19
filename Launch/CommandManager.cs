using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Media.Imaging;
using Newtonsoft.Json;

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
        private Command QuitCommand;
        private Command ReloadCommand;
        private Command OpenShortcutsFolderCommand;
        const int HistorySize = 10;

        public CommandManager()
        {
            Commands = new List<Command>();
            History = new List<Command>();
            QuitCommand = new Command
            {
                Name = "Quit",
                Path = ":quit"
            };
            ReloadCommand = new Command
            {
                Name = "Reload",
                Path = ":reload"
            };
            OpenShortcutsFolderCommand = new Command
            {
                Name = "Open the shortcuts folder",
                Path = ":open-shortcuts-folder"
            };
            LoadCommands();
            LoadHistory();
        }

        ~CommandManager()
        {
            SaveHistory();
        }

        public void LoadCommands()
        {
            Commands.Clear();
            Commands.Add(QuitCommand);
            Commands.Add(ReloadCommand);
            Commands.Add(OpenShortcutsFolderCommand);
            string[] menus = {
                ShortcutsFolderPath,
                Environment.GetFolderPath(Environment.SpecialFolder.StartMenu),
                Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu)
            };
            foreach (var menu in menus)
            {
                MakeCache(menu);
            }
        }

        public string AppDataFolderPath
        {
            get
            {
                var path = Environment.GetFolderPath(
                    Environment.SpecialFolder.LocalApplicationData,
                    Environment.SpecialFolderOption.Create
                );
                path = Path.Combine(path, "Coroq", "Launch");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                return path;
            }
        }

        public string ShortcutsFolderPath
        {
            get
            {
                var path = Path.Combine(AppDataFolderPath, "Shortcuts");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                return path;
            }
        }

        public string HistoryFilePath
        {
            get
            {
                return Path.Combine(AppDataFolderPath, "History.json");
            }
        }

        public void OpenShortcutsFolder()
        {
            System.Diagnostics.Process.Start(ShortcutsFolderPath);
        }

        public void SaveHistory()
        {
            var paths = History.Select(command => command.Path);
            var json = JsonConvert.SerializeObject(paths);
            File.WriteAllText(HistoryFilePath, json);
        }

        public void LoadHistory()
        {
            History.Clear();
            var json = File.ReadAllText(HistoryFilePath);
            var paths = JsonConvert.DeserializeObject<string[]>(json);
            foreach (var path in paths)
            {
                foreach (var command in Commands)
                {
                    if (command.Path == path)
                    {
                        History.Add(command);
                        break;
                    }
                }
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
            if (command.Path == ":quit")
            {
                System.Windows.Application.Current.Shutdown();
                return;
            }
            if (command.Path == ":reload")
            {
                SaveHistory();
                LoadCommands();
                LoadHistory();
                return;
            }
            if (command == OpenShortcutsFolderCommand)
            {
                OpenShortcutsFolder();
                return;
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
                        var ext = Path.GetExtension(item);
                        if (ext == ".lnk" || ext == ".url")
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
