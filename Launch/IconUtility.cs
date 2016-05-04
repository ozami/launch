using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Launch
{
    static class IconUtility
    {
        public static Icon GetFileIcon(string path)
        {
            var info = new SHFILEINFO();
            var imageList = SHGetFileInfo(
                path,
                0,
                ref info,
                (uint)Marshal.SizeOf(info),
                SHGetFileInfoFlags.SysIconIndex
            );
            if (imageList == IntPtr.Zero)
            {
                throw new Exception();
            }
            var iconHandle = ImageList_GetIcon(imageList, info.iIcon, 0);
            var icon = (Icon)Icon.FromHandle(iconHandle).Clone();
            DestroyIcon(iconHandle);
            return icon;
        }

        private enum SHGetFileInfoFlags : uint
        {
            SysIconIndex = 0x4000
        };

        private struct SHFILEINFO
        {
            public IntPtr hIcon;
            public int iIcon;
            public uint dwAttributes;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szDisplayName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public string szTypeName;
        };

        [DllImport("shell32.dll")]
        private static extern IntPtr SHGetFileInfo(
            string pszPath,
            uint dwFileAttributes,
            ref SHFILEINFO psfi,
            uint cbSizeFileInfo,
            SHGetFileInfoFlags uFlags
        );

        [DllImport("comctl32.dll")]
        private static extern IntPtr ImageList_GetIcon(IntPtr himl, int i, int flags);

        [DllImport("user32.dll")]
        private static extern bool DestroyIcon(IntPtr handle);
    }
}
