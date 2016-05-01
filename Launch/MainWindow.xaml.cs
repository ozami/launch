using HotKey;
using System;
using System.Windows;

namespace Launch
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        private CommandWindow commandWindow;

        public MainWindow()
        {
            InitializeComponent();
            SourceInitialized += OnSourceInitialized;
            var hotkey = new HotKeyRegister(MOD_KEY.CONTROL, System.Windows.Forms.Keys.Space, this);
            hotkey.HotKeyPressed += (sender) =>
            {
                if (commandWindow.IsVisible)
                {
                    commandWindow.Hide();
                }
                else
                {
                    commandWindow.Show();
                    commandWindow.Activate();
                }
            };
        }

        private void OnSourceInitialized(object sender, EventArgs e)
        {
            commandWindow = new CommandWindow();
            commandWindow.Owner = this;
            Hide();
        }
    }
}
