//  iTunesInfo: Displays iTunes information and uses system keyboard shortcuts to control iTunes
//  Copyright (C) 2011  Jeffrey Bush <jeff@coderforlife.com>
//
//  This file is part of iTunesInfo
//
//  iTunesInfo is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  iTunesInfo is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with iTunesInfo. If not, see <http://www.gnu.org/licenses/>.


using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace iTunesInfo
{
    /// <summary>Static utility class providing utilities for using glass windows in Vista/7.</summary>
    static class Glass
    {
        #region Windows API

        /// <summary>Windows Message that informs all top-level windows that Desktop Window Manager (DWM) composition has been enabled or disabled</summary>
        public const uint WM_DWMCOMPOSITIONCHANGED = 0x031E;

        /// <summary>Enables the blur effect on a specified window</summary>
        /// <remarks>http://msdn.microsoft.com/library/aa969508.aspx</remarks>
        /// <param name="hWnd">The handle to the window on which the blur behind data is applied</param>
        /// <param name="pBlurBehind">A pointer to a DWM_BLURBEHIND structure that provides blur behind data</param>
        [DllImport("dwmapi.dll", PreserveSig = false)] private static extern void DwmEnableBlurBehindWindow(IntPtr hWnd, DWM_BLURBEHIND pBlurBehind);
        /// <summary>Extends the window frame into the client area</summary>
        /// <remarks>http://msdn.microsoft.com/library/aa969512.aspx</remarks>
        /// <param name="hWnd">The handle to the window in which the frame will be extended into the client area</param>
        /// <param name="pMargins">A pointer to a MARGINS structure that describes the margins to use when extending the frame into the client area</param>
        [DllImport("dwmapi.dll", PreserveSig = false)] private static extern void DwmExtendFrameIntoClientArea(IntPtr hWnd, MARGINS pMargins);
        /// <summary>Gets whether Desktop Window Manager (DWM) composition is enabled or not</summary>
        /// <remarks>http://msdn.microsoft.com/library/aa969518.aspx</remarks>
        /// <returns>Returns true if DWM composition is enabled; otherwise, false</returns>
        [DllImport("dwmapi.dll", PreserveSig = false)] private static extern bool DwmIsCompositionEnabled();
        /// <summary>Enables or disables Desktop Window Manager (DWM) composition</summary>
        /// <remarks>http://msdn.microsoft.com/library/aa969510.aspx</remarks>
        /// <param name="bEnable">True to enable DWM composition; false to disable composition</param>
        [DllImport("dwmapi.dll", PreserveSig = false)] private static extern void DwmEnableComposition(bool bEnable);

        /// <summary>Retrieves a handle to a device context (DC) for the client area of a specified window or for the entire screen</summary>
        /// <remarks>http://msdn.microsoft.com/library/dd144871.aspx</remarks>
        /// <param name="hdc">A handle to the window whose DC is to be retrieved, or NULL to retrieve the DC for the entire screen</param>
        /// <returns>If the function succeeds, the return value is a handle to the DC for the specified window's client area</returns>
        [DllImport("user32.dll", SetLastError = true)] private static extern IntPtr GetDC(IntPtr hdc);
        /// <summary>Releases a device context (DC), freeing it for use by other applications</summary>
        /// <remarks>http://msdn.microsoft.com/library/dd162920.aspx</remarks>
        /// <param name="hdc">A handle to the window whose DC is to be released</param>
        /// <param name="state">A handle to the DC to be released</param>
        /// <returns>The return value indicates whether the DC was released, if the DC was released the return value is 1 otherwise 0</returns>
        [DllImport("user32.dll", SetLastError = true)] private static extern int ReleaseDC(IntPtr hdc, int state);
        /// <summary>Fills a rectangle by using the specified brush, this function includes the left and top borders but excludes the right and bottom borders of the rectangle</summary>
        /// <remarks>http://msdn.microsoft.com/library/dd162719.aspx</remarks>
        /// <param name="hDC">A handle to the device context</param>
        /// <param name="lprc">A pointer to a RECT structure that contains the logical coordinates of the rectangle to be filled</param>
        /// <param name="hBrush">A handle to the brush used to fill the rectangle</param>
        /// <returns>If the function succeeds the return value is nonzero</returns>
        [DllImport("user32.dll", SetLastError = true)] private static extern int FillRect(IntPtr hDC, [In] ref RECT lprc, IntPtr hBrush);

        /// <summary>Saves the current state of the specified device context (DC) by copying data describing selected objects and graphic modes to a context stack</summary>
        /// <remarks>http://msdn.microsoft.com/library/dd162945.aspx</remarks>
        /// <param name="hdc">A handle to the DC whose state is to be saved</param>
        /// <returns>If the function succeeds the return value identifies the saved state</returns>
        [DllImport("gdi32.dll", SetLastError = true)] private static extern int SaveDC(IntPtr hdc);
        /// <summary>Creates a memory device context (DC) compatible with the specified device</summary>
        /// <remarks>http://msdn.microsoft.com/library/dd183489.aspx</remarks>
        /// <param name="hDC">A handle to an existing DC, if this is NULL the function creates a memory DC compatible with the application's current screen</param>
        /// <returns>If the function succeeds, the return value is the handle to a memory DC</returns>
        [DllImport("gdi32.dll", SetLastError = true)] private static extern IntPtr CreateCompatibleDC(IntPtr hDC);
        /// <summary>Selects an object into the specified device context (DC), the new object replaces the previous object of the same type</summary>
        /// <remarks>http://msdn.microsoft.com/library/dd162957.aspx</remarks>
        /// <param name="hDC">A handle to the DC</param>
        /// <param name="hObject">A handle to the object to be selected</param>
        /// <returns>If the selected object is not a region and the function succeeds, the return value is a handle to the object being replaced, if the selected object is a region and the function succeeds, the return value is one of the following values...</returns>
        [DllImport("gdi32.dll", SetLastError = true)] private static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObject);
        /// <summary>Deletes a logical pen, brush, font, bitmap, region, or palette, freeing all system resources associated with the object</summary>
        /// <remarks>http://msdn.microsoft.com/library/dd183539.aspx</remarks>
        /// <param name="hObject">A handle to a logical pen, brush, font, bitmap, region, or palette</param>
        /// <returns>If the function succeeds, the return value is nonzero</returns>
        [DllImport("gdi32.dll", SetLastError = true)] private static extern bool DeleteObject(IntPtr hObject);
        /// <summary>Deletes the specified device context (DC)</summary>
        /// <remarks>http://msdn.microsoft.com/library/dd183533.aspx</remarks>
        /// <param name="hdc">A handle to the device context</param>
        /// <returns>If the function succeeds, the return value is nonzero</returns>
        [DllImport("gdi32.dll", SetLastError = true)] private static extern bool DeleteDC(IntPtr hdc);
        /// <summary>Creates a DIB that applications can write to directly, the function gives you a pointer to the location of the bitmap bit values</summary>
        /// <remarks>http://msdn.microsoft.com/library/dd183494.aspx</remarks>
        /// <param name="hdc">A handle to a device context</param>
        /// <param name="pbmi">A pointer to a BITMAPINFO structure that specifies various attributes of the DIB, including the bitmap dimensions and colors</param>
        /// <param name="iUsage">The type of data contained in the bmiColors array member of the BITMAPINFO structure pointed to by pbmi</param>
        /// <param name="ppvBits">A pointer to a variable that receives a pointer to the location of the DIB bit values</param>
        /// <param name="hSection">A handle to a file-mapping object that the function will use to create the DIB, can be NULL</param>
        /// <param name="dwOffset">The offset from the beginning of the file-mapping object referenced by hSection where storage for the bitmap bit values is to begin</param>
        /// <returns>If the function succeeds, the return value is a handle to the newly created DIB, and *ppvBits points to the bitmap bit values</returns>
        [DllImport("gdi32.dll", SetLastError = true)] private static extern IntPtr CreateDIBSection(IntPtr hdc, ref BITMAPINFO pbmi, uint iUsage, int ppvBits, IntPtr hSection, uint dwOffset);
        /// <summary>Performs a bit-block transfer of the color data corresponding to a rectangle of pixels from the specified source device context into a destination device context</summary>
        /// <remarks>http://msdn.microsoft.com/library/dd183370.aspx</remarks>
        /// <param name="hdc">A handle to the destination device context</param>
        /// <param name="nXDest">The x-coordinate, in logical units, of the upper-left corner of the destination rectangle</param>
        /// <param name="nYDest">The y-coordinate, in logical units, of the upper-left corner of the destination rectangle</param>
        /// <param name="nWidth">The width, in logical units, of the source and destination rectangles</param>
        /// <param name="nHeight">The height, in logical units, of the source and the destination rectangles</param>
        /// <param name="hdcSrc">A handle to the source device context</param>
        /// <param name="nXSrc">The x-coordinate, in logical units, of the upper-left corner of the source rectangle</param>
        /// <param name="nYSrc">The y-coordinate, in logical units, of the upper-left corner of the source rectangle</param>
        /// <param name="dwRop">A raster-operation code, which defines how the color data for the source rectangle is to be combined with the color data for the destination rectangle to achieve the final color</param>
        /// <returns>If the function succeeds, the return value is nonzero</returns>
        [DllImport("gdi32.dll", SetLastError = true)] private static extern bool BitBlt(IntPtr hdc, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hdcSrc, int nXSrc, int nYSrc, uint dwRop);

        /// <summary>Draws text using the color and font defined by the visual style, extending DrawThemeText by allowing additional text format options</summary>
        /// <remarks>http://msdn.microsoft.com/library/bb773317.aspx</remarks>
        /// <param name="hTheme">Handle to a window's specified theme data</param>
        /// <param name="hdc">HDC to use for drawing</param>
        /// <param name="iPartId">The control part that has the desired text appearance</param>
        /// <param name="iStateId">The control state that has the desired text appearance, or 0 if the text should be drawn in the font selected into the device context</param>
        /// <param name="text">Pointer to a string that contains the text to draw</param>
        /// <param name="iCharCount">Value of type int that contains the number of characters to draw, or -1 for all the characters in the string</param>
        /// <param name="dwFlags">Contains one or more values that specify the string's formatting</param>
        /// <param name="pRect">Pointer to a RECT structure that contains the rectangle, in logical coordinates, in which the text is to be drawn</param>
        /// <param name="pOptions">A DTTOPTS structure that defines additional formatting options that will be applied to the text being drawn</param>
        [DllImport("UxTheme.dll", PreserveSig = false, CharSet = CharSet.Unicode, ExactSpelling = true)]
        private static extern void DrawThemeTextEx(IntPtr hTheme, IntPtr hdc, int iPartId, int iStateId, string text, int iCharCount, DrawTextFormat dwFlags, ref RECT pRect, ref DTTOPTS pOptions);
        /// <summary>Sets attributes to control how visual styles are applied to a specified window</summary>
        /// <remarks>http://msdn.microsoft.com/library/bb759829.aspx</remarks>
        /// <param name="hWnd">Handle to a window to apply changes to</param>
        /// <param name="wtype">Value of type WINDOWTHEMEATTRIBUTETYPE that specifies the type of attribute to set</param>
        /// <param name="attributes">A pointer that specifies attributes to set</param>
        /// <param name="size">Specifies the size, in bytes, of the data pointed to by pvAttribute</param>
        [DllImport("UxTheme.dll", PreserveSig = false)] private static extern void SetWindowThemeAttribute(IntPtr hWnd, WindowThemeAttributeType wtype, ref WTA_OPTIONS attributes, uint size);

        /// <summary>Define the margins of windows that have visual styles applied</summary>
        /// <remarks>http://msdn.microsoft.com/library/bb773244.aspx</remarks>
        [StructLayout(LayoutKind.Sequential)]
        private class MARGINS
        {
            public int cxLeftWidth, cxRightWidth, cyTopHeight, cyBottomHeight;
            public MARGINS(int left, int top, int right, int bottom) { cxLeftWidth = left; cyTopHeight = top; cxRightWidth = right; cyBottomHeight = bottom; }
        }

        /// <summary>Specifies Desktop Window Manager (DWM) blur-behind properties, used by the DwmEnableBlurBehindWindow function</summary>
        /// <remarks>http://msdn.microsoft.com/library/aa969500.aspx</remarks>
        [StructLayout(LayoutKind.Sequential)]
        private struct DWM_BLURBEHIND
        {
            /// <summary>A bitwise combination of DWM Blur Behind constant values that indicates which of the members of this structure have been set</summary>
            public Flags dwFlags;
            /// <summary>TRUE to register the window handle to DWM blur behind; FALSE to unregister the window handle from DWM blur behind</summary>
            [MarshalAs(UnmanagedType.Bool)] public bool fEnable;
            /// <summary>The region within the client area where the blur behind will be applied, a NULL value will apply the blur behind the entire client area</summary>
            public IntPtr hRegionBlur;
            /// <summary>TRUE if the window's colorization should transition to match the maximized windows; otherwise, FALSE</summary>
            [MarshalAs(UnmanagedType.Bool)] public bool fTransitionOnMaximized;

            [Flags]
            public enum Flags : uint
            {
                /// <summary>A value for the fEnable member has been specified</summary>
                ENABLE = 0x00000001,
                /// <summary>A value for the hRgnBlur member has been specified</summary>
                BLURREGION = 0x00000002,
                /// <summary>A value for the fTransitionOnMaximized member has been specified</summary>
                TRANSITIONONMAXIMIZED = 0x00000004,
            }
        }

        /// <summary>The values that are used with the dwTextFlags parameter of the DrawThemeText and GetThemeTextExtent functions</summary>
        /// <remarks>http://msdn.microsoft.com/library/bb773199.aspx</remarks>
        [Flags]
        private enum DrawTextFormat : uint
        {
            /// <summary>Renders the text at the top of the display rectangle</summary>
            TOP                     = 0x00000000,
            /// <summary>Aligns text to the left</summary>
            LEFT                    = 0x00000000,
            /// <summary>Centers text horizontally in the display rectangle</summary>
            CENTER                  = 0x00000001,
            /// <summary>Aligns text to the right</summary>
            RIGHT                   = 0x00000002,
            /// <summary>Centers text vertically; this value is used only with the DT_SINGLELINE value</summary>
            VCENTER                 = 0x00000004,
            /// <summary>Renders the text string at the bottom of the display rectangle; this value is used only with the DT_SINGLELINE value</summary>
            BOTTOM                  = 0x00000008,
            /// <summary>Breaks lines between words if a word would extend past the edge of the display rectangle; a carriage return/line feed (CR/LF) sequence also breaks the line</summary>
            WORDBREAK               = 0x00000010,
            /// <summary>Displays text on a single line; carriage returns and line feeds do not break the line</summary>
            SINGLELINE              = 0x00000020,
            /// <summary>Expands tab characters; the default number of characters per tab is eight and cannot be used with the DT_WORD_ELLIPSIS, DT_PATH_ELLIPSIS, and DT_END_ELLIPSIS</summary>
            EXPANDTABS              = 0x00000040,
            /// <summary>Sets tab stops</summary>
            TABSTOP                 = 0x00000080,
            /// <summary>Draws the text string without clipping the display rectangle</summary>
            NOCLIP                  = 0x00000100,
            /// <summary>Includes the external leading of a font in the line height; normally, external leading is not included in the height of a line of text</summary>
            EXTERNALLEADING         = 0x00000200,
            /// <summary>Determines the width and height of the display rectangle</summary>
            CALCRECT                = 0x00000400,
            /// <summary>Turns off processing of prefix characters (&)</summary>
            NOPREFIX                = 0x00000800,
            /// <summary>Duplicates the text-displaying characteristics of a multiline edit control</summary>
            EDITCONTROL             = 0x00002000,
            /// <summary>Replaces characters in the middle of text with an ellipsis so that the result fits in the display rectangle. If the string contains backslash (\) characters, DT_PATH_ELLIPSIS preserves as much as possible of the text after the last backslash. The string is not modified unless the DT_MODIFYSTRING flag is specified.</summary>
            PATH_ELLIPSIS           = 0x00004000,
            /// <summary>Truncates a text string that is wider than the display rectangle and adds an ellipsis to indicate the truncation. The string is not modified unless the DT_MODIFYSTRING flag is specified.</summary>
            END_ELLIPSIS            = 0x00008000,
            /// <summary>Modifies a string to match the displayed text; has no effect unless DT_END_ELLIPSIS or DT_PATH_ELLIPSIS is specified</summary>
            MODIFYSTRING            = 0x00010000,
            /// <summary>Lays out text in right-to-left order for bidirectional text, for example, text in a Hebrew or Arabic font</summary>
            RTLREADING              = 0x00020000,
            /// <summary>Truncates any word that does not fit in the display rectangle and adds an ellipsis</summary>
            WORD_ELLIPSIS           = 0x00040000,
            /// <summary>Prevents a line break at a double-byte character set (DBCS), so that the line-breaking rule is equivalent to single-byte character set (SBCS); this value has no effect unless DT_WORDBREAK is specified</summary>
            NOFULLWIDTHCHARBREAK    = 0x00080000,
            /// <summary>Ignores the prefix character (&) in the text</summary>
            HIDEPREFIX              = 0x00100000,
            /// <summary>Draws only an underline at the position of the character following the prefix character (&)</summary>
            PREFIXONLY              = 0x00200000,
        }

        /// <summary>Copies the source rectangle directly to the destination rectangle</summary>
        /// <remarks>http://msdn.microsoft.com/library/dd183370.aspx</remarks>
        private const int SRCCOPY = 0x00CC0020;
        /// <summary>Combines the colors of the source and destination rectangles by using the Boolean OR operator</summary>
        /// <remarks>http://msdn.microsoft.com/library/dd183370.aspx</remarks>
        private const int SRCPAINT = 0xEE0086;

        /// <summary>The color table contains literal RGB values</summary>
        /// <remarks>http://msdn.microsoft.com/library/dd162974.aspx</remarks>
        private const int DIB_RGB_COLORS = 0; //color table in RGBs

        /// <summary>Defines the x- and y- coordinates of a point</summary>
        /// <remarks>http://msdn.microsoft.com/library/dd162805.aspx</remarks>
        [StructLayout(LayoutKind.Sequential)]
        private struct POINT { public int x; public int y; }

        /// <summary>Defines the options for the DrawThemeTextEx function</summary>
        /// <remarks>http://msdn.microsoft.com/library/bb773236.aspx</remarks>
        [StructLayout(LayoutKind.Sequential)]
        private struct DTTOPTS
        {
            /// <summary>Size of the structure</summary>
            public uint dwSize;
            /// <summary>A combination of flags that specify whether certain values of the DTTOPTS structure have been specified, and how to interpret these values</summary>
            public Flags dwFlags;
            /// <summary>Specifies the color of the text that will be drawn</summary>
            public uint crText;
            /// <summary>Specifies the color of the outline that will be drawn around the text</summary>
            public uint crBorder;
            /// <summary>Specifies the color of the shadow that will be drawn behind the text</summary>
            public uint crShadow;
            /// <summary>Specifies the type of the shadow that will be drawn behind the text. This member can have one of the following values</summary>
            public int iTextShadowType;
            /// <summary>Specifies the amount of offset, in logical coordinates, between the shadow and the text</summary>
            public POINT ptShadowOffset;
            /// <summary>Specifies the radius of the outline that will be drawn around the text</summary>
            public int iBorderSize;
            /// <summary>Specifies an alternate font property to use when drawing text, see GetThemeSysFont (http://msdn.microsoft.com/library/bb759783.aspx) for a list of possible values</summary>
            public int iFontPropId;
            /// <summary>Specifies an alternate color property to use when drawing text, see GetSysColor (http://msdn.microsoft.com/library/ms724371.aspx) for the nIndex parameter for a list of possible values</summary>
            public int iColorPropId;
            /// <summary>Specifies an alternate state to use (This member is not used by DrawThemeTextEx)</summary>
            public int iStateId;
            /// <summary>If TRUE, text will be drawn on top of the shadow and outline effects, or if FALSE just the shadow and outline effects will be drawn</summary>
            public bool fApplyOverlay;
            /// <summary>Specifies the size of a glow that will be drawn on the background prior to any text being drawn</summary>
            public int iGlowSize;
            /// <summary>Pointer to callback function for DrawThemeTextEx</summary>
            public IntPtr pfnDrawTextCallback;
            /// <summary>Parameter for callback back function specified by pfnDrawTextCallback</summary>
            public int lParam;

            [Flags]
            public enum Flags : uint
            {
                /// <summary>The crText member value is valid</summary>
                TEXTCOLOR = (int)(1UL << 0),
                /// <summary>The iGlowSize member value is valid</summary>
                GLOWSIZE = (int)(1UL << 11),
                /// <summary>Draws text with antialiased alpha, use of this flag requires a top-down DIB section</summary>
                COMPOSITED = (int)(1UL << 13),
            }
        }

        /// <summary>The BITMAPINFOHEADER structure contains information about the dimensions and color format of a DIB</summary>
        /// <remarks>http://msdn.microsoft.com/library/dd183376.aspx</remarks>
        [StructLayout(LayoutKind.Sequential)]
        private struct BITMAPINFOHEADER
        {
            /// <summary>The number of bytes required by the structure</summary>
            public uint biSize;
            /// <summary>The width of the bitmap, in pixels</summary>
            public int biWidth;
            /// <summary>The height of the bitmap, in pixels; if biHeight is positive, the bitmap is a bottom-up DIB otherwise it is a top-down</summary>
            public int biHeight;
            /// <summary>The number of planes for the target device; must be set to 1</summary>
            public ushort biPlanes;
            /// <summary>The number of bits-per-pixel</summary>
            public ushort biBitCount;
            /// <summary>The type of compression for a compressed bottom-up bitmap (top-down DIBs cannot be compressed)</summary>
            public Compression biCompression;
            /// <summary>The size, in bytes, of the image; this may be set to zero for BI_RGB bitmaps</summary>
            public uint biSizeImage;
            /// <summary>The horizontal resolution, in pixels-per-meter, of the target device for the bitmap</summary>
            public int biXPelsPerMeter;
            /// <summary>The vertical resolution, in pixels-per-meter, of the target device for the bitmap</summary>
            public int biYPelsPerMeter;
            /// <summary>The number of color indexes in the color table that are actually used by the bitmap or zero to use the maximum number of colors corresponding to the value of the biBitCount member for the compression mode specified by biCompression</summary>
            public uint biClrUsed;
            /// <summary>The number of color indexes that are required for displaying the bitmap; if this value is zero, all colors are required</summary>
            public uint biClrImportant;

            public enum Compression : uint
            {
                /// <summary>An uncompressed format</summary>
                RGB = 0,
            }
        }

        /// <summary>The BITMAPINFO structure defines the dimensions and color information for a DIB</summary>
        /// <remarks>http://msdn.microsoft.com/library/dd183375.aspx</remarks>
        [StructLayout(LayoutKind.Sequential)]
        private struct BITMAPINFO
        {
            /// <summary>A BITMAPINFOHEADER structure that contains information about the dimensions of color format</summary>
            public BITMAPINFOHEADER bmiHeader;
            /// <summary>The bitmap data (the data type is just a filler)</summary>
            public int bmiColors;
        }

        /// <summary>Specifies the type of visual style attribute to set on a window</summary>
        /// <remarks>http://msdn.microsoft.com/en-us/library/bb759870.aspx</remarks>
        private enum WindowThemeAttributeType
        {
            /// <summary>Specifies non-client related attributes, the pvAttribute must be a pointer of type WTA_OPTIONS</summary>
            WTA_NONCLIENT = 1,
        };

        /// <summary>Defines options that are used to set window visual style attributes</summary>
        /// <remarks>http://msdn.microsoft.com/library/bb773248.aspx</remarks>
        [StructLayout(LayoutKind.Sequential)]
        private struct WTA_OPTIONS
        {
            /// <summary>A combination of flags that modify window visual style attributes</summary>
            public Flags dwFlags;
            /// <summary>A bitmask that describes how the values specified in dwFlags should be applied. If the bit corresponding to a value in dwFlags is 0, that flag will be removed. If the bit is 1, the flag will be added.</summary>
            public Flags dwMask;

            /// <summary>Specifies flags that modify window visual style attributes</summary>
            /// <remarks>http://msdn.microsoft.com/en-us/library/bb759875.aspx</remarks>
            [Flags]
            public enum Flags : uint
            {
                /// <summary>Prevents the window caption from being drawn</summary>
                NODRAWCAPTION = 0x00000001,
                /// <summary>Prevents the system icon from being drawn</summary>
                NODRAWICON = 0x00000002,
                /// <summary>Prevents the system icon menu from appearing</summary>
                NOSYSMENU = 0x00000004,
                /// <summary>Prevents mirroring of the question mark, even in right-to-left (RTL) layout</summary>
                NOMIRRORHELP = 0x00000008,
            }
        }

        #endregion

        /// <summary>True if glass effects are supported on the current machine. Cannot change while program is running.</summary>
        public static bool Supported { get { return Environment.OSVersion.Version.Major >= 6; } }

        /// <summary>True if glass effects are enabled on the current machine. May change while program is running.</summary>
        public static bool Enabled { get { return DwmIsCompositionEnabled(); } }


        /// <summary>Cause an entire form to use a glass background. Only need to call this once for a form.</summary>
        /// <param name="f">The form to set glass for</param>
        public static void ForEntireForm(Form f)
        {
            // DwmExtendFrameIntoClientArea only works on windows that have a caption bar (frame)
            if (f.FormBorderStyle == FormBorderStyle.None || f.Text == "" && f.ControlBox == false && f.ShowIcon == false)
            {
                // This may or may not work, I don't use it
                DWM_BLURBEHIND blur = new DWM_BLURBEHIND();
                blur.dwFlags = DWM_BLURBEHIND.Flags.ENABLE;
                blur.fEnable = true;
                DwmEnableBlurBehindWindow(f.Handle, blur);
            }
            else
            {
                DwmExtendFrameIntoClientArea(f.Handle, new MARGINS(-1, -1, -1, -1));
            }
        }

        /// <summary>Hide anything that is in the caption bar, such as the Form text and buttons, but does not remove the space it takes</summary>
        /// <param name="f">The form to set the invisible caption for</param>
        public static void MakeCaptionInvisible(Form f)
        {
            WTA_OPTIONS.Flags flags = WTA_OPTIONS.Flags.NODRAWCAPTION | WTA_OPTIONS.Flags.NODRAWICON;
            WTA_OPTIONS wta = new WTA_OPTIONS() { dwFlags = flags, dwMask = flags };
            SetWindowThemeAttribute(f.Handle, WindowThemeAttributeType.WTA_NONCLIENT, ref wta, (uint)Marshal.SizeOf(typeof(WTA_OPTIONS)));
        }

        /// <summary>Utility to create a DIB section for a memory DC object</summary>
        /// <param name="memdc">The memory DC to use</param>
        /// <param name="width">The width of the bitmap data</param>
        /// <param name="height">The height of the bitmap data</param>
        /// <returns>The 32-bit DIB (bitmap) section created</returns>
        private static IntPtr createDIBSection(IntPtr memdc, int width, int height)
        {
            BITMAPINFO dib = new BITMAPINFO
            {
                bmiHeader =
                {
                    biSize = (uint)Marshal.SizeOf(typeof(BITMAPINFOHEADER)),
                    biWidth = width,
                    biHeight = -height,   // negative because DrawThemeTextEx() uses a top-down DIB
                    biPlanes = 1,
                    biBitCount = 32,
                    biCompression = BITMAPINFOHEADER.Compression.RGB,
                }
            };
            return CreateDIBSection(memdc, ref dib, DIB_RGB_COLORS, 0, IntPtr.Zero, 0);
        }

        /// <summary>Show the glass of a form. This needs to be called at the start of each repaint.</summary>
        /// <param name="g">The graphics object of the form to be used</param>
        /// <param name="r">The rectangle where glass will be used</param>
        public static void Show(Graphics g, Rectangle r)
        {
            int width = r.Width, height = r.Height;

            // Get the graphics DC handle and create a compatible memory DC that will be blank and bltted onto the form graphics
            IntPtr destdc = g.GetHdc(), memdc = CreateCompatibleDC(destdc);
            if (SaveDC(memdc) != 0)
            {
                // Create the bitmap data for the memory DC, it is initially blank (all transparent? black)
                IntPtr bitmap = createDIBSection(memdc, width, height);
                if (bitmap != IntPtr.Zero)
                {
                    // Select the bitmap we just made
                    IntPtr bitmapOld = SelectObject(memdc, bitmap);

                    // No need for the following since the bitmap is initially blank
                    // And it is probably wrong anyways
                    //RECT rc = new RECT(r.Left, r.Top, r.Right, r.Bottom);
                    //FillRect(memdc, ref rc, Brushes.Black.GetNativeHandle());

                    // Draw the black image onto the entire form, which will cause it all to be glass
                    BitBlt(destdc, r.Left, r.Top, width, height, memdc, 0, 0, SRCCOPY);

                    // Cleanup
                    SelectObject(memdc, bitmapOld);
                    DeleteObject(bitmap);
                }
            }

            // More cleanup
            ReleaseDC(memdc, -1);
            DeleteDC(memdc);
            g.ReleaseHdc();
        }

        /// <summary>Draws text onto glass, giving it a glow so that it can be easily read</summary>
        /// <param name="g">The form's graphics</param>
        /// <param name="text">The text to draw</param>
        /// <param name="font">The font to use for the text</param>
        /// <param name="c">The color to use for the text</param>
        /// <param name="r">The maximum rectangle that can contain text</param>
        /// <param name="glowSize">The size of the glowing effect</param>
        /// <param name="margin">The size of the margin around 'r' that may contain glow effect but not text</param>
        public static void DrawTextGlow(Graphics g, string text, Font font, Color c, Rectangle r, int glowSize, int margin)
        {
            const DrawTextFormat uFormat = DrawTextFormat.LEFT | DrawTextFormat.TOP | DrawTextFormat.SINGLELINE | DrawTextFormat.WORD_ELLIPSIS | DrawTextFormat.NOPREFIX;

            // Preserve the offset transform in the graphics object (which is not preserved in the handle returned by GetHdc)
            r.Offset((int)g.Transform.OffsetX, (int)g.Transform.OffsetY);

            // Setup basic variables
            int width = r.Width, height = r.Height;
            int full_width = width + 2 * margin, full_height = height + 2 * margin;

            // Prepare the necessary DCs
            IntPtr destdc = g.GetHdc();                 // must be the handle of form, not control
            IntPtr memdc = CreateCompatibleDC(destdc);  // set up a memory DC where we'll draw the text.

            if (SaveDC(memdc) != 0)
            {
                // Create a 32-bit bmp for use in off-screen drawing when glass is on
                IntPtr bitmap = createDIBSection(memdc, full_width, full_height);
                if (bitmap != IntPtr.Zero)
                {
                    // Prepare the DCs for drawing in the given font and the system visual style
                    IntPtr hFont = font.ToHfont();
                    IntPtr bitmapOld = SelectObject(memdc, bitmap), fontOld = SelectObject(memdc, hFont);

                    VisualStyleRenderer renderer = new VisualStyleRenderer(VisualStyleElement.Window.Caption.Active);

                    // Setup the DrawTextTheme options: text color and glow size
                    DTTOPTS dttOpts = new DTTOPTS();
                    dttOpts.dwSize = (uint)Marshal.SizeOf(typeof(DTTOPTS));
                    dttOpts.dwFlags = DTTOPTS.Flags.COMPOSITED | DTTOPTS.Flags.TEXTCOLOR | DTTOPTS.Flags.GLOWSIZE;
                    dttOpts.crText = (uint)c.ToArgb() & 0x00FFFFFF;
                    dttOpts.iGlowSize = glowSize;

                    // Get the rectangle to draw in
                    RECT rc = new RECT(margin, margin, width + margin, height + margin);

                    // Draw the text, first to the memory object than the form's graphics
                    DrawThemeTextEx(renderer.Handle, memdc, 0, 0, text, -1, uFormat, ref rc, ref dttOpts);
                    BitBlt(destdc, r.Left - margin, r.Top - margin, full_width, full_height, memdc, 0, 0, SRCPAINT);

                    // Cleanup
                    SelectObject(memdc, bitmapOld);
                    SelectObject(memdc, fontOld);
                    DeleteObject(hFont);
                    DeleteObject(bitmap);
                }
            }
            // More cleanup
            ReleaseDC(memdc, -1);
            DeleteDC(memdc);
            g.ReleaseHdc();
        }


        /// <summary>Get the Euclidean distance between two points</summary>
        /// <param name="x0">First point's x coord</param>
        /// <param name="y0">First point's y coord</param>
        /// <param name="x1">Second point's x coord</param>
        /// <param name="y1">Second point's y coord</param>
        /// <returns>The distance</returns>
        private static double Distance(int x0, int y0, int x1, int y1) { int x = x1 - x0, y = y1 - y0; return Math.Sqrt(x * x + y * y); }

        /// <summary>Get the amount of alpha for the glow of a particular pixel</summary>
        /// <param name="X">The x coordinate of the pixel</param>
        /// <param name="Y">The y coordinate of the pixel</param>
        /// <param name="glowSize">The size of the glow effect</param>
        /// <param name="alphas">The matrix of alphas in the original image</param>
        /// <returns>The alpha value to use for the glow effect</returns>
        private static byte GetVal(int X, int Y, int glowSize, byte[,] alphas)
        {
            // If the target pixel is solid, then there is no glow there
            if (alphas[X, Y] == 255) return 0;

            // Search a glow-sized radius rectangle for opaque-ish pixels
            byte max_val = 0;
            int x_start = Math.Max(X - glowSize + 1, 0), x_end = Math.Min(X + glowSize, alphas.GetLength(0));
            int y_start = Math.Max(Y - glowSize + 1, 0), y_end = Math.Min(Y + glowSize, alphas.GetLength(1));
            for (int x = x_start; x < x_end; ++x)
            {
                for (int y = y_start; y < y_end; ++y)
                {
                    byte alpha = alphas[x, y];

                    // If the current pixel is completely transparent then it won't be used
                    if (alpha == 0) continue;

                    // Weight the alpha value by the distance from the target pixel
                    byte val = Convert.ToByte(alpha * (1.0 - Math.Min(Distance(X, Y, x, y) / glowSize, 1.0)));

                    // Use the maximum found value
                    if (max_val < val)
                        max_val = val;
                }
            }
            return max_val;
        }

        /// <summary>Gets the rectangle that will fit the glow effect around a particular rectangle</summary>
        /// <param name="r">The rectangle that will be glowing</param>
        /// <param name="glowSize">The size of the glow effect</param>
        /// <returns>The expanded rectangle taking into effect the glow size</returns>
        public static Rectangle GetGlowBox(Rectangle r, int glowSize)
        {
            return new Rectangle(r.X - glowSize, r.Y - glowSize, r.Width + glowSize * 2, r.Height + glowSize * 2);
        }

        /// <summary>Creates a bitmap with the glow of the image, taking into account the alpha channel information in the image</summary>
        /// <remarks>Uses unsafe code to greatly speed up the operation</remarks>
        /// <param name="img">The image to draw a glow effect for</param>
        /// <param name="glowSize">The size of the glow effect</param>
        /// <returns>The glow effect image</returns>
        public unsafe static Bitmap CreateImageGlow(Bitmap img, int glowSize)
        {
            int w = img.Width, h = img.Height;
            int W = w + 2 * glowSize, H = h + 2 * glowSize;

            // Gather the alpha channel data
            BitmapData data = img.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            byte[,] alphas = new byte[W, H];
            for (int y = 0; y < h; ++y)
            {
                byte* row = (byte*)data.Scan0 + (y * data.Stride);
                for (int x = 0; x < w; ++x)
                    alphas[x + glowSize, y + glowSize] = row[x * 4 + 3];
            }
            img.UnlockBits(data);

            // Draw the glow
            Bitmap b = new Bitmap(W, H);
            data = b.LockBits(new Rectangle(0, 0, W, H), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            for (int y = 0; y < H; ++y)
            {
                byte* row = (byte*)data.Scan0 + (y * data.Stride);
                for (int x = 0; x < W; ++x)
                {
                    int alpha = GetVal(x, y, glowSize, alphas);
                    if (alpha > 0)
                    {
                        // White color, that is transparent
                        row[x * 4 + 0] = 0xFF; // blue
                        row[x * 4 + 1] = 0xFF; // green
                        row[x * 4 + 2] = 0xFF; // red
                        row[x * 4 + 3] = (byte)(alpha / 2); // alpha
                    }
                }
            }
            b.UnlockBits(data);

            return b;
        }

        /// <summary>The cache of glow images so that they don't have to all be recreated</summary>
        private static Dictionary<string, Bitmap> glowImageCache = new Dictionary<string, Bitmap>();
        /// <summary>Get a glow effect for an image, possibly from a cache if something in the cache matches the name given</summary>
        /// <param name="name">The name to use for lookups in the cache</param>
        /// <param name="img">The image to draw a glow effect for</param>
        /// <param name="glowSize">The size of the glow effect</param>
        /// <returns>The glow effect image, possibly from the cache</returns>
        public static Bitmap GetImageGlow(string name, Bitmap img, int glowSize)
        {
            Bitmap b;
            if (!glowImageCache.TryGetValue(name, out b))
                glowImageCache[name] = b = CreateImageGlow(img, glowSize);
            return b;
        }
    }
}
