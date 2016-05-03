using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace Launch
{
    /// <summary>
    /// CommandWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class CommandWindow : Window
    {
        private CommandManager commandManager;
        private ObservableCollection<Command> commands;

        public CommandWindow()
        {
            InitializeComponent();
            commandManager = new CommandManager();
            commands = new ObservableCollection<Command>();
            suggestList.ItemsSource = commands;
            Activated += CommandWindow_Activated;
            commandInput.TextChanged += CommandInput_TextChanged;
            commandInput.PreviewKeyDown += CommandInput_PreviewKeyDown;
        }

        private void CommandWindow_Activated(object sender, EventArgs e)
        {
            commandInput.Text = "";
            commandInput.Focus();
        }

        private void CommandInput_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            UpdateSuggest();
        }

        private void CommandInput_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Hide();
                return;
            }
            if (e.Key == Key.Enter)
            {
                if (suggestList.SelectedIndex >= 0)
                {
                    commandManager.Launch(commands[suggestList.SelectedIndex]);
                    Hide();
                }
                return;
            }
            if (e.Key == Key.Down)
            {
                if (commands.Count > 0)
                {
                    suggestList.SelectedIndex = (suggestList.SelectedIndex + 1) % commands.Count;
                }
                return;
            }
            if (e.Key == Key.Up)
            {
                if (commands.Count > 0)
                {
                    suggestList.SelectedIndex = (suggestList.SelectedIndex - 1 + commands.Count) % commands.Count;
                }
                return;
            }
        }

        private void UpdateSuggest()
        {
            commands.Clear();
            foreach (var command in commandManager.Find(commandInput.Text))
            {
                commands.Add(command);
            }
            if (commands.Count > 0)
            {
                suggestList.SelectedIndex = 0;
            }
        }
    }
}
