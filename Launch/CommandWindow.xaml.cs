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
        private CommandManager commandManger;
        private ObservableCollection<Command> commands;

        public CommandWindow()
        {
            InitializeComponent();
            commandManger = new CommandManager();
            commands = new ObservableCollection<Command>();
            suggestList.ItemsSource = commands;

            Activated += CommandWindow_Activated;
            commandInput.TextChanged += CommandInput_TextChanged;
            commandInput.KeyDown += CommandInput_KeyDown;
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
                var found = commandManger.Find(commandInput.Text);
                if (found.Length > 0)
                {
                    commandManger.Launch(found[0]);
                }
                return;
            }
        }

        private void UpdateSuggest()
        {
            commands.Clear();
            foreach (var command in commandManger.Find(commandInput.Text))
            {
                commands.Add(command);
            }
        }
    }
}
