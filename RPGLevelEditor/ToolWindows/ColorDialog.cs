using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace RPGLevelEditor.ToolWindows
{
    public unsafe class ColorDialog
    {
        [Flags]
        protected enum CC
        {
            CC_ANYCOLOR = 0x00000100,
            CC_ENABLEHOOK = 0x00000010,
            CC_ENABLETEMPLATE = 0x00000020,
            CC_ENABLETEMPLATEHANDLE = 0x00000040,
            CC_FULLOPEN = 0x00000002,
            CC_PREVENTFULLOPEN = 0x00000004,
            CC_RGBINIT = 0x00000001,
            CC_SHOWHELP = 0x00000008,
            CC_SOLIDCOLOR = 0x00000080
        }

        [StructLayout(LayoutKind.Sequential)]
        protected ref struct CHOOSECOLORW
        {
            public int lStructSize;
            public IntPtr hwndOwner;
            public IntPtr hInstance;
            public int rgbResult;
            public int* lpCustColors;
            public CC Flags;
            public IntPtr lCustData;
            public IntPtr lpfnHook;
            public IntPtr lpTemplateName;
        }

        [DllImport("comdlg32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        protected static extern bool ChooseColorW(CHOOSECOLORW* lpcc);

        public Color SelectedColor { get; set; }
        public bool PreventFullOpen { get; set; }
        public bool StartFullOpen { get; set; }
        public Color[] CustomColors { get; } = new Color[16];

        public bool ShowDialog(Window owner)
        {
            CC flags = CC.CC_RGBINIT;
            if (PreventFullOpen)
            {
                flags |= CC.CC_PREVENTFULLOPEN;
            }
            if (StartFullOpen)
            {
                flags |= CC.CC_FULLOPEN;
            }

            int[] packedCustomColors = new int[16];
            for (int i = 0; i < packedCustomColors.Length; i++)
            {
                Color color = CustomColors[i];
                packedCustomColors[i] = color.R + (color.G << 8) + (color.B << 16);
            }

            fixed (int* packedCustomColorsPtr = packedCustomColors)
            {
                CHOOSECOLORW lpcc = new()
                {
                    lStructSize = Marshal.SizeOf(typeof(CHOOSECOLORW)),
                    hwndOwner = new WindowInteropHelper(owner).Handle,
                    hInstance = IntPtr.Zero,
                    rgbResult = SelectedColor.R
                        + (SelectedColor.G << 8)
                        + (SelectedColor.B << 16),
                    lpCustColors = packedCustomColorsPtr,
                    Flags = flags,
                    lCustData = IntPtr.Zero,
                    lpfnHook = IntPtr.Zero,
                    lpTemplateName = IntPtr.Zero
                };

                bool result = ChooseColorW(&lpcc);

                SelectedColor = new Color()
                {
                    R = (byte)lpcc.rgbResult,
                    G = (byte)(lpcc.rgbResult >> 8),
                    B = (byte)(lpcc.rgbResult >> 16),
                    A = byte.MaxValue
                };

                return result;
            }
        }
    }
}
