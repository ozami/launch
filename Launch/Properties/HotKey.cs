using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WinAPI.Const;

namespace WinAPI.Manager
{
    public class HotKey : IDisposable
    {
        private HotkeyForm hWnd = null;

        public int ID { get; private set; }
        public Keys Keys { get; private set; }
        public bool Enabled { get; private set; }
        public event EventHandler HotkeyEvent;

        private HotKey() { }
        public HotKey(Keys key)
        {
            this.Keys = key;

            Register();
        }
        ~HotKey()
        {
            this.Dispose();
        }
        public void Dispose(){
            this.Unregister();
        }

        private void Register() {

            if (hWnd == null)
            {
                hWnd = new HotkeyForm();
                hWnd.CreateHandle(new CreateParams());
            }

            if (!this.Enabled)
            {

                MOD modifiers = 0;

                if ((this.Keys & Keys.Alt) == Keys.Alt)
                    modifiers = modifiers | MOD.MOD_ALT;
                if ((this.Keys & Keys.Control) == Keys.Control)
                    modifiers = modifiers | MOD.MOD_CONTROL;
                if ((this.Keys & Keys.Shift) == Keys.Shift)
                    modifiers = modifiers | MOD.MOD_SHIFT;
                Keys k = this.Keys & ~Keys.Control & ~Keys.Shift & ~Keys.Alt;

                this.ID = (new Random()).Next(0x1000, 0xbfff);

                NativeMethod.RegisterHotKey(hWnd.Handle, this.ID, modifiers, k);
                this.Enabled = true;

                hWnd.Handler += this.hWndHotkeyEvent;
            }
        }
        private void Unregister()
        {
            if (this.Enabled)
            {
                NativeMethod.UnregisterHotKey(hWnd.Handle, this.ID);
                this.ID = 0;
                this.Enabled = false;

                hWnd.Handler -= this.hWndHotkeyEvent;
            }
        }
        
        private void hWndHotkeyEvent(int id)
        {
            if (this.ID == id && HotkeyEvent != null)
            {
                HotkeyEvent(this, EventArgs.Empty);
            }
        }

        internal class HotkeyForm : NativeWindow
        {
            internal HotkeyForm()
            {
            }

            internal delegate void HotkeyHandler(int id);
            internal event HotkeyHandler Handler;

            protected override void WndProc(ref Message m)
            {
                if (m.Msg == (int)WM.WM_HOTKEY)
                {
                    if (Handler != null)
                        Handler((int)m.WParam);
                }

                base.WndProc(ref m);
            }
        }
    }
}
