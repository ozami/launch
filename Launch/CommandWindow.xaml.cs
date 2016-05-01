using System;
using System.Collections.Generic;
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

        public CommandWindow()
        {
            InitializeComponent();
            commandManger = new CommandManager();
            Activated += CommandWindow_Activated;
            commandInput.KeyDown += CommandInput_KeyDown;
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
                var found = commandManger.find(commandInput.Text);
                if (found.Length > 0)
                {
                    commandManger.Launch(found[0]);
                }
            }
        }

    }
}
