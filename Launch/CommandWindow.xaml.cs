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
            commandInput.Focus();
            commandInput.KeyDown += CommandInput_KeyDown;
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
            }
        }
    }
}
