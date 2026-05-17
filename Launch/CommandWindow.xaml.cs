using System;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

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
            Deactivated += CommandWindow_Deactivated;
            commandInput.TextChanged += CommandInput_TextChanged;
            commandInput.PreviewKeyDown += CommandInput_PreviewKeyDown;
            suggestList.MouseLeftButtonUp += SuggestList_MouseLeftButtonUp;
            SourceInitialized += CommandWindow_SourceInitialized;
        }

        private void CommandWindow_SourceInitialized(object sender, EventArgs e)
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            int preference = DWMWCP_ROUNDSMALL;
            DwmSetWindowAttribute(hwnd, DWMWA_WINDOW_CORNER_PREFERENCE, ref preference, sizeof(int));
        }

        private const int DWMWA_WINDOW_CORNER_PREFERENCE = 33;
        private const int DWMWCP_ROUNDSMALL = 3;

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attribute, ref int pvAttribute, int cbAttribute);

        private void SuggestList_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (suggestList.SelectedIndex < 0)
            {
                return;
            }
            commandManager.Launch(commands[suggestList.SelectedIndex]);
            Hide();
        }

        private void CommandWindow_Activated(object sender, EventArgs e)
        {
            commandInput.Text = "";
            commandInput.Focus();
        }

        private void CommandWindow_Deactivated(object sender, EventArgs e)
        {
            Hide();
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
            suggestList.Visibility = commands.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
            if (commands.Count > 0)
            {
                suggestList.SelectedIndex = 0;
            }
        }
    }
}
