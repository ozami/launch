using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Launch
{
    static class IconUtility
    {
        public static Icon GetExeIcon(string path, int index)
        {
            var largeIconHandle = IntPtr.Zero;
            var smallIconHandle = IntPtr.Zero;
            ExtractIconEx(path, index, out largeIconHandle, out smallIconHandle, 1);
            var icon = (Icon)Icon.FromHandle(largeIconHandle).Clone();
            DestroyIcon(largeIconHandle);
            DestroyIcon(smallIconHandle);
            return icon;
        }

        [DllImport("shell32.dll", EntryPoint = "ExtractIconEx", CharSet = CharSet.Auto)]
        private static extern int ExtractIconEx(
            [MarshalAs(UnmanagedType.LPTStr)] string file,
            int index,
            out IntPtr largeIconHandle,
            out IntPtr smallIconHandle,
            int icons
        );

        [DllImport("User32.dll")]
        private static extern bool DestroyIcon(IntPtr hIcon);
    }
}
