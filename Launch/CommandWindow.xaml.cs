using System;
using System.Windows;
using System.Windows.Input;

namespace Launch
{
    /// <summary>
    /// CommandWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class CommandWindow : Window
    {
        public CommandWindow()
        {
            InitializeComponent();
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
                Launch(commandInput.Text);
            }
        }

        private void Launch(string command)
        {
            if (command == "chrome")
            {
                System.Diagnostics.Process.Start("Chrome.exe");
                return;
            }
        }

        
    }
}
