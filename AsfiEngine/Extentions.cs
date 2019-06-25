using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Threading;
using static AsfiEngine.Extra.NativeMethods;

namespace AsfiEngine.Extra
{
    public static class Extentions
    {
        public static MOUSE_EVENT_RECORD Mouse;
        public static string[] ConsoleColors = new string[] { "Black", "Blue", "Cyan", "DarkBlue", "DarkCyan", "DarkGray", "DarkGreen", "DarkMagenta", "DarkRed", "DarkYellow", "Gray", "Green", "Magenta", "Red", "White", "Yellow" };
        private static int _pmx = 0;
        private static int _pmy = 0;

        public static void Initialize()
        {
            IntPtr inHandle = GetStdHandle(STD_INPUT_HANDLE);
            uint mode = 0;
            GetConsoleMode(inHandle, ref mode);
            mode &= ~ENABLE_QUICK_EDIT_MODE; 
            mode |= ENABLE_WINDOW_INPUT; 
            mode |= ENABLE_MOUSE_INPUT; 
            SetConsoleMode(inHandle, mode);
            
            // Keyboard event handler
            ConsoleListener.KeyEvent += (KEY_EVENT_RECORD record) => 
            {
                //Console.WriteLine((char)record.AsciiChar+" : "+record.wVirtualKeyCode);
                char key = (char)record.AsciiChar;
                bool down = record.bKeyDown;
                bool up = !down;

                //if an item is currently in hand
                if(Program.InventoyItemClicked)
                {
                    if(Program.InventoryItemInHand.Item.Rotatable && key == 'r' && up)
                    {
                        if(Program.InventoryItemInHand.Item.Type==ItemType.Miner)
                        {
                            ((MO_Miner)Program.InventoryItemInHand.Item).Rotate();
                        }
                    }
                }

                //inventory key pressed
                if (key == 'e' && up)
                {
                    if(Program.InventoryOpen)
                    {
                        Program.InventoryOpen = false;
                        Program.S_MAP.Picture.Advance = true;
                        Program.S_MAP.RenderLimitRight = Program.WIDTH;
                        Program.S_MAP.Draw();
                    }
                    else
                    {
                        Program.InventoryPage = 0;
                        Program.S_MAP.RenderLimitRight = 63;
                        Program.InventoryHighlightedLine = -1;
                        Program.InventoryOpen = true;
                        Program.DrawInventory();
                    }
                }

                if (key=='w'&& down)
                {
                    //move map up
                    Program.S_MAP.Picture.Advance = true;
                    //Program.CameraY += 1;
                    Program.S_MAP.Y += 1;
                    Program.S_MAP.Draw();
                }
                if(key=='s' && down)
                {
                    //move map down
                    Program.S_MAP.Picture.Advance = true;
                    //Program.CameraY -= 1;
                    Program.S_MAP.Y -= 1;
                    Program.S_MAP.Draw();
                }
                if(key=='a' && down)
                {
                    //move map left
                    Program.S_MAP.Picture.Advance = true;
                    //Program.CameraX += 1;
                    Program.S_MAP.X += 1;
                    Program.S_MAP.Draw();
                }
                if(key=='d' && down)
                {
                    //move map right
                    Program.S_MAP.Picture.Advance = true;
                    //Program.CameraX -= 1;
                    Program.S_MAP.X -= 1;
                    Program.S_MAP.Draw();
                }
            };

            // Mouse event handler
            ConsoleListener.MouseEvent += (MOUSE_EVENT_RECORD record) => 
            {
                int x = record.dwMousePosition.X;
                int y = record.dwMousePosition.Y;
                bool left = record.dwButtonState == 1;
                bool right = record.dwButtonState == 2;
                bool changed = x == _pmx && y == _pmy ? false : true;
                
                string tk = $"{record.dwMousePosition.X},{record.dwMousePosition.Y}";

                //if(left)
                //{
                //    Console.WriteLine(tk);
                //}
                
                if(Program.InventoyItemClicked&&(changed))
                {
                    //if mouse is in currently in open map viewing area
                    if (x > Program.S_MAP.RenderLimitLeft && y > Program.S_MAP.RenderLimitTop && x < Program.S_MAP.RenderLimitRight && y < Program.S_MAP.RenderLimitBottom-2 && Program.InventoryItemInHand.Item.Placable)
                    {
                        int mdx = _pmx - x;
                        int mdy = _pmy - y;
                        int bdr = (int)Math.Sqrt(mdx * mdx + mdy * mdy) == 0 ? 1 : (int)Math.Sqrt(mdx * mdx + mdy * mdy) + 1;
                        int imw = Program.InventoryItemInHand.Item.PlacedImage.Frames[0].GetLength(1);
                        int imh = Program.InventoryItemInHand.Item.PlacedImage.Frames[0].GetLength(0);
                        int sx = x - (int)(imw / (float)2)-1;
                        int sy = y - (int)(imh / (float)2)-1;
                        SimpleSprite ss = new SimpleSprite(sx, sy, Program.InventoryItemInHand.Item.PlacedImage, true);
                        ss.RenderLimitRight = Program.S_MAP.RenderLimitRight-2;
                        ss.RenderLimitBottom = Program.S_MAP.RenderLimitBottom;
                        ss.RenderLimitTop = Program.S_MAP.RenderLimitTop;
                        ss.RenderLimitLeft = Program.S_MAP.RenderLimitLeft;
                        Program.S_MAP.Picture.Advance = true;
                        ss.Picture.Advance = true;
                        int rll = Program.S_MAP.RenderLimitLeft;
                        int rlr = Program.S_MAP.RenderLimitRight;
                        int rlt = Program.S_MAP.RenderLimitTop;
                        int rlb = Program.S_MAP.RenderLimitBottom;
                        Program.S_MAP.RenderLimitLeft = sx - bdr + 1 >= 0 ? sx - bdr + 1 : 0;
                        Program.S_MAP.RenderLimitRight = sx + imw + bdr * 2 + 1 <= rlr ? sx + imw + bdr * 2 + 1 : rlr;
                        Program.S_MAP.RenderLimitTop = sy - bdr + 1 >= 0 ? sy - bdr + 1 : 0;
                        Program.S_MAP.RenderLimitBottom = sy + imh + bdr * 2 + 1 <= rlb ? sy + imh + bdr * 2 + 1 : rlb;
                        Program.S_MAP.Draw();
                        Program.S_MAP.RenderLimitLeft = rll;
                        Program.S_MAP.RenderLimitRight = rlr;
                        Program.S_MAP.RenderLimitTop = rlt;
                        Program.S_MAP.RenderLimitBottom = rlb; 
                        ss.Draw();

                        MenuObject item = Program.InventoryItemInHand.Item;
                        // do item specific movement actions
                        if (item.Type!=ItemType.None)
                        {
                            switch(item.Type)
                            {
                                case ItemType.Miner: // if item held in hand is a miner
                                    // update miner image based on resources below image ( miner image can be any size? )
                                    MO_Miner miner = ((MO_Miner)item);
                                    miner.ResetRootImage();
                                    int tvx = 0;
                                    int tvy = 0;
                                    bool hsrc = false;
                                    for (int vy = sy + (int)Program.S_MAP.Y; tvy < miner.RootPicture.GetLength(0); vy++)
                                    {
                                        for(int vx = sx + (int)Program.S_MAP.X; tvx < miner.RootPicture.GetLength(1); vx++)
                                        {
                                            if(Program.MapResources.ContainsKey($"{vx},{vy}"))
                                            {
                                                hsrc = true;
                                                miner.RootPicture[tvy, tvx] = $"<Yellow>{miner.RootPicture[tvy, tvx]}<R>";
                                            }
                                            tvx++;
                                        }
                                        tvx = 0;
                                        tvy++;
                                    }
                                    break;
                            }
                        }
                    }
                }

                if(Program.InventoryOpen)
                {
                    if (x > 68 && x < 98 && y > 2 && y < 19)
                    {
                        if(left)
                        {
                            // Item in row y has been clicked on
                            InventoryItem clk = Program.InventoryItems[Program.InventoryLinesPerPage * Program.InventoryPage + y - 3];
                            Program.InventoyItemClicked = true;
                            Program.InventoryItemInHand = clk;
                        }
                        if(Program.InventoryHighlightedLine != y && !Program.InventoyItemClicked)
                        {
                            Program.InventoryHighlightedLine = y;
                            Program.DrawInventory(Program.InventoryPage);
                        }
                    }
                    if (left)
                    {
                        if(Program.InventoryScrollLeft && tk=="64,21")
                        {
                            Program.InventoryPage -= 1;
                            Program.InventoryHighlightedLine = -1;
                            Program.DrawInventory(Program.InventoryPage);
                        }
                        if(Program.InventoryScrollRight && tk =="96,21")
                        {
                            Program.InventoryPage += 1;
                            Program.InventoryHighlightedLine = -1;
                            Program.DrawInventory(Program.InventoryPage);
                        }
                    }
                }
                
                // code below is test code for map and cursor interaction, will be removed later
                //if (Program.MapResources.Count > 0 && Program.MapResources.Keys.Contains(tk))
                //{
                //    Resource cur = Program.MapResources[tk];
                //    Program.DrawUI(cur.Ammount+"", cur.Picture);
                //}
                //else
                //{
                //    Program.DrawUI("", Program.NoImage);
                //}

                _pmx = x;
                _pmy = y;
            };
            ConsoleListener.Start();
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        internal struct CONSOLE_FONT_INFO_EX
        {
            internal uint cbSize;
            internal uint nFont;
            internal COORD dwFontSize;
            internal int FontFamily;
            internal int FontWeight;
            internal char FaceName;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct COORD
        {
            internal short X;
            internal short Y;

            internal COORD(short x, short y)
            {
                X = x;
                Y = y;
            }
        }

        private const int STD_OUTPUT_HANDLE = -11;
        private const int TMPF_TRUETYPE = 4;
        private const int LF_FACESIZE = 32;
        private static IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool SetCurrentConsoleFontEx(IntPtr consoleOutput, bool maximumWindow, ref CONSOLE_FONT_INFO_EX consoleCurrentFontEx);
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr GetStdHandle(uint dwType);
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr GetStdHandle(int dwType);
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern int SetConsoleFont(IntPtr hOut, uint dwFontNum);

        public static void ResetConsoleColors()
        {
            Console.ResetColor();
        }

        public static void SetConsoleBackgroundColor(ConsoleColor color)
        {
            Console.BackgroundColor = color;
        }

        public static void SetConsoleTextColor(ConsoleColor color)
        {
            Console.ForegroundColor = color;
        }

        public static void SetConsoleFont(string fontName = "Lucida Console")
        {
            IntPtr hnd = GetStdHandle(STD_OUTPUT_HANDLE);
            if (hnd != INVALID_HANDLE_VALUE)
            {
                CONSOLE_FONT_INFO_EX info = new CONSOLE_FONT_INFO_EX();
                info.cbSize = (uint)Marshal.SizeOf(info);

                CONSOLE_FONT_INFO_EX newInfo = new CONSOLE_FONT_INFO_EX();
                newInfo.cbSize = (uint)Marshal.SizeOf(newInfo);
                newInfo.FontFamily = TMPF_TRUETYPE;
                IntPtr ptr = new IntPtr(newInfo.FaceName);
                Marshal.Copy(fontName.ToCharArray(), 0, ptr, fontName.Length);

                newInfo.dwFontSize = new COORD(info.dwFontSize.X, info.dwFontSize.Y);
                newInfo.FontWeight = info.FontWeight;
                SetCurrentConsoleFontEx(hnd, false, ref newInfo);
            }
        }

        public static ConsoleColor ToConsoleColor(this string str)
        {
            switch (str)
            {
                case "Black":
                    return ConsoleColor.Black;
                case "Blue":
                    return ConsoleColor.Blue;
                case "Cyan":
                    return ConsoleColor.Cyan;
                case "DarkBlue":
                    return ConsoleColor.DarkBlue;
                case "DarkCyan":
                    return ConsoleColor.DarkCyan;
                case "DarkGray":
                    return ConsoleColor.DarkGray;
                case "DarkGreen":
                    return ConsoleColor.DarkGreen;
                case "DarkMagenta":
                    return ConsoleColor.DarkMagenta;
                case "DarkRed":
                    return ConsoleColor.DarkRed;
                case "DarkYellow":
                    return ConsoleColor.DarkYellow;
                case "Gray":
                    return ConsoleColor.Gray;
                case "Green":
                    return ConsoleColor.Green;
                case "Magenta":
                    return ConsoleColor.Magenta;
                case "Red":
                    return ConsoleColor.Red;
                case "White":
                    return ConsoleColor.White;
                case "Yellow":
                    return ConsoleColor.Yellow;
                default:
                    return ConsoleColor.White;
            }
        }

        public static char ToChar(this string str)
        {
            int tot = 1;
            char[] ch = str.ToCharArray();
            int fact = 1;
            for(int i = 0; i < ch.Length; i++)
            {
                tot += (int)ch[i]*fact;
                fact *= 10;
            }
            return (char)tot;
        }

        public static bool HasConsoleColor(this string str)
        {
            for(int i = 0; i < ConsoleColors.Length; i++)
            {
                if (str.Contains($"<{ConsoleColors[i]}>")|| str.Contains($"|{ConsoleColors[i]}|"))
                {
                    return true;
                }
            }
            return false;
        }

        public static string Mult(this char chr, int times)
        {
            string res = "";
            for (int i = 0; i < times; i++)
            {
                res += chr;
            }
            return res;
        }

        public static string Mult(this string str, int times)
        {
            string res = "";
            for(int i = 0; i < times; i ++)
            {
                res += str;
            }
            return res;
        }
    }

    public static class ConsoleListener
    {
        public static event ConsoleMouseEvent MouseEvent;

        public static event ConsoleKeyEvent KeyEvent;

        public static event ConsoleWindowBufferSizeEvent WindowBufferSizeEvent;

        private static bool Run = false;

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr GetStdHandle(uint dwType);

        public static void Start()
        {
            if (!Run)
            {
                Run = true;
                IntPtr handleIn = GetStdHandle(STD_INPUT_HANDLE);
                new Thread(() =>
                {
                    while (true)
                    {
                        uint numRead = 0;
                        INPUT_RECORD[] record = new INPUT_RECORD[1];
                        record[0] = new INPUT_RECORD();
                        ReadConsoleInput(handleIn, record, 1, ref numRead);
                        if (Run)
                            switch (record[0].EventType)
                            {
                                case INPUT_RECORD.MOUSE_EVENT:
                                    MouseEvent?.Invoke(record[0].MouseEvent);
                                    break;
                                case INPUT_RECORD.KEY_EVENT:
                                    KeyEvent?.Invoke(record[0].KeyEvent);
                                    break;
                                case INPUT_RECORD.WINDOW_BUFFER_SIZE_EVENT:
                                    WindowBufferSizeEvent?.Invoke(record[0].WindowBufferSizeEvent);
                                    break;
                            }
                        else
                        {
                            uint numWritten = 0;
                            WriteConsoleInput(handleIn, record, 1, ref numWritten);
                            return;
                        }
                    }
                }).Start();
            }
        }

        public static void Stop() => Run = false;


        public delegate void ConsoleMouseEvent(MOUSE_EVENT_RECORD r);

        public delegate void ConsoleKeyEvent(KEY_EVENT_RECORD r);

        public delegate void ConsoleWindowBufferSizeEvent(WINDOW_BUFFER_SIZE_RECORD r);
    }

    public static class NativeMethods
    {
        public struct COORD
        {
            public short X;
            public short Y;

            public COORD(short x, short y)
            {
                X = x;
                Y = y;
            }
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct INPUT_RECORD
        {
            public const ushort KEY_EVENT = 0x0001;
            public const ushort MOUSE_EVENT = 0x0002;
            public const ushort WINDOW_BUFFER_SIZE_EVENT = 0x0004;

            [FieldOffset(0)]
            public ushort EventType;

            [FieldOffset(4)]
            public KEY_EVENT_RECORD KeyEvent;

            [FieldOffset(4)]
            public MOUSE_EVENT_RECORD MouseEvent;

            [FieldOffset(4)]
            public WINDOW_BUFFER_SIZE_RECORD WindowBufferSizeEvent;
        }

        public struct MOUSE_EVENT_RECORD
        {
            public COORD dwMousePosition;

            public const uint FROM_LEFT_1ST_BUTTON_PRESSED = 0x0001;
            public const uint FROM_LEFT_2ND_BUTTON_PRESSED = 0x0004;
            public const uint FROM_LEFT_3RD_BUTTON_PRESSED = 0x0008;
            public const uint FROM_LEFT_4TH_BUTTON_PRESSED = 0x0010;
            public const uint RIGHTMOST_BUTTON_PRESSED = 0x0002;
            public uint dwButtonState;

            public const int CAPSLOCK_ON = 0x0080;
            public const int ENHANCED_KEY = 0x0100;
            public const int LEFT_ALT_PRESSED = 0x0002;
            public const int LEFT_CTRL_PRESSED = 0x0008;
            public const int NUMLOCK_ON = 0x0020;
            public const int RIGHT_ALT_PRESSED = 0x0001;
            public const int RIGHT_CTRL_PRESSED = 0x0004;
            public const int SCROLLLOCK_ON = 0x0040;
            public const int SHIFT_PRESSED = 0x0010;
            public uint dwControlKeyState;

            public const int DOUBLE_CLICK = 0x0002;
            public const int MOUSE_HWHEELED = 0x0008;
            public const int MOUSE_MOVED = 0x0001;
            public const int MOUSE_WHEELED = 0x0004;

            public uint dwEventFlags;
        }

        [StructLayout(LayoutKind.Explicit, CharSet = CharSet.Unicode)]
        public struct KEY_EVENT_RECORD
        {
            [FieldOffset(0)]
            public bool bKeyDown;

            [FieldOffset(4)]
            public ushort wRepeatCount;

            [FieldOffset(6)]
            public ushort wVirtualKeyCode;

            [FieldOffset(8)]
            public ushort wVirtualScanCode;

            [FieldOffset(10)]
            public char UnicodeChar;

            [FieldOffset(10)]
            public byte AsciiChar;

            public const int CAPSLOCK_ON = 0x0080;
            public const int ENHANCED_KEY = 0x0100;
            public const int LEFT_ALT_PRESSED = 0x0002;
            public const int LEFT_CTRL_PRESSED = 0x0008;
            public const int NUMLOCK_ON = 0x0020;
            public const int RIGHT_ALT_PRESSED = 0x0001;
            public const int RIGHT_CTRL_PRESSED = 0x0004;
            public const int SCROLLLOCK_ON = 0x0040;
            public const int SHIFT_PRESSED = 0x0010;

            [FieldOffset(12)]
            public uint dwControlKeyState;
        }

        public struct WINDOW_BUFFER_SIZE_RECORD
        {
            public COORD dwSize;
        }

        public const uint STD_INPUT_HANDLE = unchecked((uint)-10);
        public const uint STD_OUTPUT_HANDLE = unchecked((uint)-11);
        public const uint STD_ERROR_HANDLE = unchecked((uint)-12);

        [DllImport("kernel32.dll")]
        public static extern IntPtr GetStdHandle(uint nStdHandle);


        public const uint ENABLE_MOUSE_INPUT = 0x0010;
        public const uint ENABLE_QUICK_EDIT_MODE = 0x0040;
        public const uint ENABLE_EXTENDED_FLAGS = 0x0080;
        public const uint ENABLE_ECHO_INPUT = 0x0004;
        public const uint ENABLE_WINDOW_INPUT = 0x0008;

        [DllImportAttribute("kernel32.dll")]
        public static extern bool GetConsoleMode(IntPtr hConsoleInput, ref uint lpMode);

        [DllImportAttribute("kernel32.dll")]
        public static extern bool SetConsoleMode(IntPtr hConsoleInput, uint dwMode);


        [DllImportAttribute("kernel32.dll", CharSet = CharSet.Unicode)]
        public static extern bool ReadConsoleInput(IntPtr hConsoleInput, [Out] INPUT_RECORD[] lpBuffer, uint nLength, ref uint lpNumberOfEventsRead);

        [DllImportAttribute("kernel32.dll", CharSet = CharSet.Unicode)]
        public static extern bool WriteConsoleInput(IntPtr hConsoleInput, INPUT_RECORD[] lpBuffer, uint nLength, ref uint lpNumberOfEventsWritten);
    }
    
    public class RandomStringGenerator
    {
        private bool m_UseUpperCaseCharacters, m_UseLowerCaseCharacters, m_UseNumericCharacters, m_UseSpecialCharacters;
        private int m_MinUpperCaseCharacters, m_MinLowerCaseCharacters, m_MinNumericCharacters, m_MinSpecialCharacters;
        private bool PatternDriven;
        private char[] CurrentUpperCaseCharacters;
        private char[] CurrentLowerCaseCharacters;
        private char[] CurrentNumericCharacters;
        private char[] CurrentSpecialCharacters;
        private char[] CurrentGeneralCharacters;
        private RNGCryptoServiceProvider Random;
        private List<string> ExistingStrings;
        public RandomStringGenerator(bool UseUpperCaseCharacters = true, bool UseLowerCaseCharacters = true, bool UseNumericCharacters = true, bool UseSpecialCharacters = true)
        {
            m_UseUpperCaseCharacters = UseUpperCaseCharacters;
            m_UseLowerCaseCharacters = UseLowerCaseCharacters;
            m_UseNumericCharacters = UseNumericCharacters;
            m_UseSpecialCharacters = UseSpecialCharacters;
            CurrentGeneralCharacters = new char[0]; // avoiding null exceptions
            UpperCaseCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
            LowerCaseCharacters = "abcdefghijklmnopqrstuvwxyz".ToCharArray();
            NumericCharacters = "0123456789".ToCharArray();
            SpecialCharacters = ",.;:?!/@#$%^&()=+*-_{}[]<>|~".ToCharArray();
            MinUpperCaseCharacters = MinLowerCaseCharacters = MinNumericCharacters = MinSpecialCharacters = 0;
            RepeatCharacters = true;
            PatternDriven = false;
            Pattern = "";
            Random = new RNGCryptoServiceProvider();
            ExistingStrings = new List<string>();
        }
        
        public bool UseUpperCaseCharacters
        {
            get
            {
                return m_UseUpperCaseCharacters;
            }
            set
            {
                if (CurrentUpperCaseCharacters != null)
                    CurrentGeneralCharacters = CurrentGeneralCharacters.Except(CurrentUpperCaseCharacters).ToArray();
                if (value)
                    CurrentGeneralCharacters = CurrentGeneralCharacters.Concat(CurrentUpperCaseCharacters).ToArray();
                m_UseUpperCaseCharacters = value;
            }
        }
            
        public char[] UpperCaseCharacters
        {
            get
            {
                return CurrentUpperCaseCharacters;
            }
            set
            {
                if (UseUpperCaseCharacters)
                {
                    if (CurrentUpperCaseCharacters != null)
                        CurrentGeneralCharacters = CurrentGeneralCharacters.Except(CurrentUpperCaseCharacters).ToArray();
                    CurrentGeneralCharacters = CurrentGeneralCharacters.Concat(value).ToArray();
                }
                CurrentUpperCaseCharacters = value;
            }
        }
            
        public bool UseLowerCaseCharacters
        {
            get
            {
                return m_UseLowerCaseCharacters;
            }
            set
            {
                if (CurrentLowerCaseCharacters != null)
                    CurrentGeneralCharacters = CurrentGeneralCharacters.Except(CurrentLowerCaseCharacters).ToArray();
                if (value)
                    CurrentGeneralCharacters = CurrentGeneralCharacters.Concat(CurrentLowerCaseCharacters).ToArray();
                m_UseLowerCaseCharacters = value;
            }
        }
            
        public char[] LowerCaseCharacters
        {
            get
            {
                return CurrentLowerCaseCharacters;
            }
            set
            {
                if (UseLowerCaseCharacters)
                {
                    if (CurrentLowerCaseCharacters != null)
                        CurrentGeneralCharacters = CurrentGeneralCharacters.Except(CurrentLowerCaseCharacters).ToArray();
                    CurrentGeneralCharacters = CurrentGeneralCharacters.Concat(value).ToArray();
                }
                CurrentLowerCaseCharacters = value;
            }
        }
            
        public bool UseNumericCharacters
        {
            get
            {
                return m_UseNumericCharacters;
            }
            set
            {
                if (CurrentNumericCharacters != null)
                    CurrentGeneralCharacters = CurrentGeneralCharacters.Except(CurrentNumericCharacters).ToArray();
                if (value)
                    CurrentGeneralCharacters = CurrentGeneralCharacters.Concat(CurrentNumericCharacters).ToArray();
                m_UseNumericCharacters = value;
            }
        }
            
        public char[] NumericCharacters
        {
            get
            {
                return CurrentNumericCharacters;
            }
            set
            {
                if (UseNumericCharacters)
                {
                    if (CurrentNumericCharacters != null)
                        CurrentGeneralCharacters = CurrentGeneralCharacters.Except(CurrentNumericCharacters).ToArray();
                    CurrentGeneralCharacters = CurrentGeneralCharacters.Concat(value).ToArray();
                }
                CurrentNumericCharacters = value;
            }
        }
            
        public bool UseSpecialCharacters
        {
            get
            {
                return m_UseSpecialCharacters;
            }
            set
            {
                if (CurrentSpecialCharacters != null)
                    CurrentGeneralCharacters = CurrentGeneralCharacters.Except(CurrentSpecialCharacters).ToArray();
                if (value)
                    CurrentGeneralCharacters = CurrentGeneralCharacters.Concat(CurrentSpecialCharacters).ToArray();
                m_UseSpecialCharacters = value;
            }
        }
            
        public char[] SpecialCharacters
        {
            get
            {
                return CurrentSpecialCharacters;
            }
            set
            {
                if (UseSpecialCharacters)
                {
                    if (CurrentSpecialCharacters != null)
                        CurrentGeneralCharacters = CurrentGeneralCharacters.Except(CurrentSpecialCharacters).ToArray();
                    CurrentGeneralCharacters = CurrentGeneralCharacters.Concat(value).ToArray();
                }
                CurrentSpecialCharacters = value;
            }
        }

        public int MinUpperCaseCharacters
        {
            get { return MinUpperCaseCharacters1; }
            set { MinUpperCaseCharacters1 = value; }
        }
            
        public int MinLowerCaseCharacters
        {
            get { return MinLowerCaseCharacters1; }
            set { MinLowerCaseCharacters1 = value; }
        }
            
        public int MinNumericCharacters
        {
            get { return MinNumericCharacters1; }
            set { MinNumericCharacters1 = value; }
        }
            
        public int MinSpecialCharacters
        {
            get { return MinSpecialCharacters1; }
            set { MinSpecialCharacters1 = value; }
        }

        private string m_pattern;
        private string Pattern
        {
            get
            {
                return m_pattern;
            }
            set
            {
                if (!value.Equals(String.Empty))
                    PatternDriven = true;
                else
                    PatternDriven = false;
                m_pattern = value;
            }
        }

        public int MinUpperCaseCharacters1 { get => MinUpperCaseCharacters2; set => MinUpperCaseCharacters2 = value; }
        public int MinUpperCaseCharacters2 { get => m_MinUpperCaseCharacters; set => m_MinUpperCaseCharacters = value; }
        public int MinLowerCaseCharacters1 { get => m_MinLowerCaseCharacters; set => m_MinLowerCaseCharacters = value; }
        public int MinNumericCharacters1 { get => m_MinNumericCharacters; set => m_MinNumericCharacters = value; }
        public int MinSpecialCharacters1 { get => m_MinSpecialCharacters; set => m_MinSpecialCharacters = value; }
            

        /// <summary>
        /// Generate a string which follows the pattern.
        /// Possible characters are:
        /// L - for upper case letter
        /// l - for lower case letter
        /// n - for number
        /// s - for special character
        /// * - for any character
        /// </summary>
        /// <param name="Pattern">The pattern to follow while generation</param>
        /// <returns>A random string which follows the pattern</returns>
        public string Generate(string Pattern)
        {
            this.Pattern = Pattern;
            string res = GenerateString(Pattern.Length);
            this.Pattern = "";
            return res;
        }

          
        public string Generate(int MinLength, int MaxLength)
        {
            if (MaxLength < MinLength)
                throw new ArgumentException("Maximal length should be grater than minumal");
            int length = MinLength + (GetRandomInt() % (MaxLength - MinLength));
            return GenerateString(length);
        }

           
        public string Generate(int FixedLength)
        {
            return GenerateString(FixedLength);
        }

        private string GenerateString(int length)
        {
            if (length == 0)
                throw new ArgumentException("You can't generate a string of a zero length");
            if (!UseUpperCaseCharacters && !UseLowerCaseCharacters && !UseNumericCharacters && !UseSpecialCharacters)
                throw new ArgumentException("There should be at least one character set in use");
            if (!RepeatCharacters && (CurrentGeneralCharacters.Length < length))
                throw new ArgumentException("There is not enough characters to create a string without repeats");
            string result = ""; 
            if (PatternDriven)
            {
                result = PatternDrivenAlgo(Pattern);
            }
            else if (MinUpperCaseCharacters == 0 && MinLowerCaseCharacters == 0 &&
                        MinNumericCharacters == 0 && MinSpecialCharacters == 0)
            {
                result = SimpleGenerateAlgo(length);
            }
            else
            {
                result = GenerateAlgoWithLimits(length);
            }
            if (UniqueStrings && ExistingStrings.Contains(result))
                return GenerateString(length);
            AddExistingString(result); 
            return result;
        }
            
        private string PatternDrivenAlgo(string Pattern)
        {
            string result = "";
            List<char> Characters = new List<char>();
            foreach (char character in Pattern.ToCharArray())
            {
                char newChar = ' ';
                switch (character)
                {
                    case 'L':
                    {
                        newChar = GetRandomCharFromArray(CurrentUpperCaseCharacters, Characters);
                        break;
                    }
                    case 'l':
                    {
                        newChar = GetRandomCharFromArray(CurrentLowerCaseCharacters, Characters);
                        break;
                    }
                    case 'n':
                    {
                        newChar = GetRandomCharFromArray(CurrentNumericCharacters, Characters);
                        break;
                    }
                    case 's':
                    {
                        newChar = GetRandomCharFromArray(CurrentSpecialCharacters, Characters);
                        break;
                    }
                    case '*':
                    {
                        newChar = GetRandomCharFromArray(CurrentGeneralCharacters, Characters);
                        break;
                    }
                    default:
                    {
                        throw new Exception("The character '" + character + "' is not supported");
                    }
                }
                Characters.Add(newChar);
                result += newChar;
            }
            return result;
        }
            
        private string SimpleGenerateAlgo(int length)
        {
            string result = "";
            for (int i = 0; i < length; i++)
            {
                char newChar = CurrentGeneralCharacters[GetRandomInt() % CurrentGeneralCharacters.Length];
                if (!RepeatCharacters && result.Contains(newChar))
                {
                    while (result.Contains(newChar))
                    {
                        newChar = CurrentGeneralCharacters[GetRandomInt() % CurrentGeneralCharacters.Length];
                    }
                }
                result += newChar;
            }
            return result;
        }

        private string GenerateAlgoWithLimits(int length)
        {
            if (MinUpperCaseCharacters + MinLowerCaseCharacters +
                MinNumericCharacters + MinSpecialCharacters > length)
            {
                throw new ArgumentException("Sum of MinUpperCaseCharacters, MinLowerCaseCharacters," +
                    " MinNumericCharacters and MinSpecialCharacters is greater than length");
            }
            if (!RepeatCharacters && (MinUpperCaseCharacters > CurrentUpperCaseCharacters.Length))
                throw new ArgumentException("Can't generate a string with this number of MinUpperCaseCharacters");
            if (!RepeatCharacters && (MinLowerCaseCharacters > CurrentLowerCaseCharacters.Length))
                throw new ArgumentException("Can't generate a string with this number of MinLowerCaseCharacters");
            if (!RepeatCharacters && (MinNumericCharacters > CurrentNumericCharacters.Length))
                throw new ArgumentException("Can't generate a string with this number of MinNumericCharacters");
            if (!RepeatCharacters && (MinSpecialCharacters > CurrentSpecialCharacters.Length))
                throw new ArgumentException("Can't generate a string with this number of MinSpecialCharacters");
            int AllowedNumberOfGeneralChatacters = length - MinUpperCaseCharacters - MinLowerCaseCharacters
                - MinNumericCharacters - MinSpecialCharacters;

            string result = "";
            List<char> Characters = new List<char>();
                
            for (int i = 0; i < MinUpperCaseCharacters; i++)
                Characters.Add(GetRandomCharFromArray(UpperCaseCharacters, Characters));
            for (int i = 0; i < MinLowerCaseCharacters; i++)
                Characters.Add(GetRandomCharFromArray(LowerCaseCharacters, Characters));
            for (int i = 0; i < MinNumericCharacters; i++)
                Characters.Add(GetRandomCharFromArray(NumericCharacters, Characters));
            for (int i = 0; i < MinSpecialCharacters; i++)
                Characters.Add(GetRandomCharFromArray(SpecialCharacters, Characters));
            for (int i = 0; i < AllowedNumberOfGeneralChatacters; i++)
                Characters.Add(GetRandomCharFromArray(CurrentGeneralCharacters, Characters));
                
            for (int i = 0; i < length; i++)
            {
                int position = GetRandomInt() % Characters.Count;
                char CurrentChar = Characters[position];
                Characters.RemoveAt(position);
                result += CurrentChar;
            }
            return result;
        }
            
        public bool RepeatCharacters;
        public bool UniqueStrings;
        
        public void AddExistingString(string s)
        {
            ExistingStrings.Add(s);
        }
        
        private int GetRandomInt()
        {
            byte[] buffer = new byte[2];
            Random.GetNonZeroBytes(buffer);
            int index = BitConverter.ToInt16(buffer, 0);
            if (index < 0)
                index = -index; 
            return index;
        }

        private char GetRandomCharFromArray(char[] array, List<char> existentItems)
        {
            char Character = ' ';
            do
            {
                Character = array[GetRandomInt() % array.Length];
            } while (!RepeatCharacters && existentItems.Contains(Character));
            return Character;
        } 
    }

    public enum SelectionMethod
    {
        Mean = 0,
        Median = 1
    }

    public enum Direction
    {
        Up = 1,
        Down = 2,
        Left = 3,
        Right = 4
    }

    public enum ResourceType
    {
        None = 0,
        Water = 1,
        Iron = 2,
        Copper = 3,
        Coal = 4,
        Stone = 5,
        Crystal = 6,
        Oil = 7
    }
    
    public static class NoiseMap
    {
        private static Random _r = new Random();
        public static int[,] GetNotSmoothedNoise(int w, int h, int variace)
        {
            int[,] map = new int[h, w];
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    map[y, x] = _r.Next(variace);
                }
            }
            return map;
        }

        public static int GetApproximateNoiseVariance(this int[,] noise)
        {
            int max = 0;
            for (int y = 0; y < noise.GetLength(0); y++)
            {
                for (int x = 0; x < noise.GetLength(1); x++)
                {
                    if (noise[y, x] > max)
                    {
                        max = noise[y, x];
                    }
                }
            }
            return max;
        }

        public static int[,] ChangeVariance(this int[,] noise, int variance)
        {
            int var = noise.GetApproximateNoiseVariance();
            double sec = (var + 0.0f) / (variance + 0.0f);
            for (int y = 0; y < noise.GetLength(0); y++)
            {
                for (int x = 0; x < noise.GetLength(1); x++)
                {
                    noise[y, x] = (int)(noise[y, x] / sec);
                }
            }
            return noise;
        }

        private static int GetValueUsingMatrix(this int[,] values, int[,] matrix, SelectionMethod method)
        {
            int i = 0;
            int tot = 0;
            List<int> all = new List<int>();
            for (int y = 0; y < values.GetLength(0); y++)
            {
                for (int x = 0; x < values.GetLength(1); x++)
                {
                    int cur = values[y, x] * matrix[y, x];
                    if (cur > 0)
                    {
                        if (method == SelectionMethod.Mean)
                        {
                            tot += cur;
                            i++;
                        }
                        else if (method == SelectionMethod.Median)
                        {
                            all.Add(cur);
                        }
                    }
                }
            }
            if (all.Count > 0)
            {
                all.Sort();
                return all[all.Count / 2];
            }
            if (i == 0)
            {
                return 0;
            }
            return tot / i;
        }

        private static int[,] GetInRange(this int[,] noise, int x, int y, int w, int h)
        {
            int[,] ret = new int[h, w];
            int left = (int)Math.Round(w / 2.0);
            int top = (int)Math.Round(h / 2.0);
            for (int wy = 0; wy < h; wy++)
            {
                for (int ex = 0; ex < w; ex++)
                {
                    int cur;
                    int cx = x - left + ex;
                    int cy = y - top + wy;
                    if (cx < 0 || cy < 0 || cx > noise.GetLength(1) - 1 || cy > noise.GetLength(1) - 1)
                    {
                        cur = 0;
                    }
                    else
                    {
                        cur = noise[cy, cx];
                    }
                    ret[wy, ex] = cur;
                }
            }
            return ret;
        }

        public static int[,] ApplyMatrix(this int[,] noise, int[,] matrix, SelectionMethod method)
        {
            int[,] ret = new int[noise.GetLength(0), noise.GetLength(1)];
            for (int y = 0; y < noise.GetLength(0); y++)
            {
                for (int x = 0; x < noise.GetLength(1); x++)
                {
                    int[,] values = noise.GetInRange(x, y, matrix.GetLength(1), matrix.GetLength(0));
                    int cur = values.GetValueUsingMatrix(matrix, method);
                    ret[y, x] = cur;
                }
            }
            return ret;
        }

        public static int[,] MedianBlur(this int[,] noise, int range)
        {
            int[,] matrix = new int[range, range];
            for (int y = 0; y < range; y++)
            {
                for (int x = 0; x < range; x++)
                {
                    matrix[y, x] = 1;
                }
            }
            return noise.ApplyMatrix(matrix, SelectionMethod.Median);
        }

        public static int[,] MeanBlur(this int[,] noise, int range)
        {
            int[,] matrix = new int[range, range];
            for (int y = 0; y < range; y++)
            {
                for (int x = 0; x < range; x++)
                {
                    matrix[y, x] = 1;
                }
            }
            return noise.ApplyMatrix(matrix, SelectionMethod.Mean);
        }

        public static int[,] Quiver(this int[,] noise, int shuffle)
        {
            int var = noise.GetApproximateNoiseVariance();
            for (int y = 0; y < noise.GetLength(0); y++)
            {
                for (int x = 0; x < noise.GetLength(1); x++)
                {
                    noise[y, x] = Math.Abs(_r.Next(shuffle) * (_r.Next(2) == 0 ? -1 : 1) + noise[y, x]) % var;
                }
            }
            return noise;
        }
    }

    public static class Log
    {

        public static string Directory = @"C:\Users\usagi\Desktop\ASFI_log.txt";
        public static void Clear()
        {
            ReWrite("");
        }
        public static string Read()
        {
            using (StreamReader sr = new StreamReader(Directory))
            {
                return sr.ReadToEnd();
            }
        }
        public static void Error(string err)
        {
            Write($"(!!!) {err}");
        }
        public static void Write(string text)
        {
            string rd = Read();
            ReWrite($"{rd}\n[{DateTime.Now}] : {text}");
        }
        private static void ReWrite(string text)
        {
            File.WriteAllText(Directory, text);
        }
    }
}
