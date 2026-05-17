using System;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Launch
{
    static class IconUtility
    {
        public static BitmapSource GetFileIcon(string path)
        {
            var info = new SHFILEINFO();
            SHGetFileInfo(
                path,
                0,
                ref info,
                (uint)Marshal.SizeOf(info),
                SHGetFileInfoFlags.SysIconIndex
            );
            var iid = IID_IImageList;
            if (SHGetImageList(SHIL_EXTRALARGE, ref iid, out IntPtr imageList) != 0 || imageList == IntPtr.Zero)
            {
                throw new Exception();
            }
            var iconHandle = ImageList_GetIcon(imageList, info.iIcon, 0);
            if (iconHandle == IntPtr.Zero)
            {
                throw new Exception();
            }
            var iconHandle2 = DuplicateIcon(IntPtr.Zero, iconHandle);
            if (iconHandle2 == IntPtr.Zero)
            {
                DestroyIcon(iconHandle);
                throw new Exception();
            }
            var icon = System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(
                iconHandle2,
                System.Windows.Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions()
            );
            DestroyIcon(iconHandle);
            return icon;
        }

        public static BitmapSource LoadImage(string path)
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(path);
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();
            bitmap.Freeze();
            return bitmap;
        }

        public static BitmapSource RenderGlyph(string text)
        {
            const int size = 48;
            var typeface = new Typeface(
                new FontFamily("Consolas"),
                FontStyles.Normal,
                FontWeights.Bold,
                FontStretches.Normal);
            var formatted = new FormattedText(
                text,
                CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight,
                typeface,
                size,
                Brushes.DimGray,
                1.0);
            var visual = new DrawingVisual();
            using (var ctx = visual.RenderOpen())
            {
                ctx.DrawText(formatted, new Point(
                    (size - formatted.WidthIncludingTrailingWhitespace) / 2.0,
                    (size - formatted.Height) / 2.0));
            }
            var bitmap = new RenderTargetBitmap(size, size, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(visual);
            bitmap.Freeze();
            return bitmap;
        }

        private enum SHGetFileInfoFlags : uint
        {
            SysIconIndex = 0x4000
        };

        private const int SHIL_EXTRALARGE = 2;
        private static readonly Guid IID_IImageList = new Guid("46EB5926-582E-4017-9FDF-E8998DAA0950");

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

        [DllImport("shell32.dll")]
        private static extern int SHGetImageList(int iImageList, ref Guid riid, out IntPtr ppv);

        [DllImport("shell32.dll")]
        private static extern IntPtr DuplicateIcon(IntPtr hInst, IntPtr hIcon);

        [DllImport("user32.dll")]
        private static extern bool DestroyIcon(IntPtr hIcon);
    }
}
