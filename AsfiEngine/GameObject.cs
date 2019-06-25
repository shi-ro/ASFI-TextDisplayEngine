using AsfiEngine.Extra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AsfiEngine
{
    public abstract class GameObject
    {
        public int Layer { get; set; }
        public string ID { get; private set; }
        public bool Drawn { get; private set; }
        public bool Animated { get; private set; }
        public double X { get; set; }
        public double Y { get; set; }
        public Image Picture { get; private set; }
        public int RenderLimitBottom = -1;
        public int RenderLimitTop = 0;
        public int RenderLimitLeft = 0;
        public int RenderLimitRight = -1;
        public bool FollowCamera { get; private set; }
        private Random _r = new Random();

        public GameObject(int x, int y, Image image, bool camchild = false)
        {
            X = x;
            Y = y;
            FollowCamera = camchild;
            RenderLimitBottom = Program.HEIGHT;
            RenderLimitRight = Program.WIDTH;
            ID = Program.RSG.Generate("LlnLlnLln");
            Program.RSG.AddExistingString(ID);
            Picture = image;
            if(Picture.Frames.Count()>0)
            {
                Animated = true;
            }
        }

        public abstract void Update();

        public void Draw()
        {
            if (Animated)
            {
                Picture.TryAdvance();
            }
            if(Picture.Advance)
            {
                Picture.Advance = false;
                int ex = (int)Math.Round(X);
                int ey = (int)Math.Round(Y);
                if(FollowCamera)
                {
                    ex -= Program.CameraX;
                    ey -= Program.CameraY;
                }
                int sx = 0;
                int sy = 0;
                if(ex>Program.WIDTH||ey>Program.HEIGHT)
                {
                    return; // loose return statement, may cause problems.
                }

                if (RenderLimitRight < RenderLimitLeft || RenderLimitBottom < RenderLimitTop)
                {
                    return; // loose return statement, may cause problems.
                }

                Console.CursorLeft = ex >= 0 ? ex : 0;
                sx = ex >= 0 ? 0 : -ex;
                Console.CursorTop = ey >= 0 ? ey : 0;
                sy = ey >= 0 ? 0 : -ey;
                
                
                if (RenderLimitTop > 0 && ey <= RenderLimitTop)
                {
                    Console.CursorTop = RenderLimitTop;
                    sy = RenderLimitTop;
                }
                if (RenderLimitLeft > 0 && ex <= RenderLimitLeft)
                {
                    Console.CursorLeft = RenderLimitLeft;
                    sx = RenderLimitLeft;
                }
                for (int y = sy; y < Picture.CurrentFrame.GetLength(0); y++)
                {
                    if (Console.CursorTop >= RenderLimitBottom - 1 && Program.LOCK)
                    {
                        break;
                    }
                    for (int x = sx; x < Picture.CurrentFrame.GetLength(1); x++)
                    {
                        if (Console.CursorLeft >= RenderLimitRight - 1 && Program.LOCK)
                        {
                            break;
                        }
                        string c = Picture.CurrentFrame[y, x];
                        if (c == "|~~~|")
                        {
                            Extentions.SetConsoleBackgroundColor(ConsoleColor.DarkBlue);
                            Extentions.SetConsoleTextColor(ConsoleColor.White);
                            char[] ch = { '~', '-', '_', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', };
                            Console.Write($"{ch[_r.Next(ch.Length)]}");
                            Extentions.ResetConsoleColors();
                        }
                        else if (c == "|iii|")
                        {
                            char wt = _r.Next(2) == 0 ? ',' : ';';
                            Extentions.SetConsoleTextColor(ConsoleColor.White);
                            Console.Write($"{wt}");
                            Extentions.ResetConsoleColors();
                        }
                        else if (c == "|ppp|")
                        {
                            char wt = _r.Next(2) == 0 ? '.' : ':';
                            Extentions.SetConsoleTextColor(ConsoleColor.DarkYellow);
                            Console.Write($"{wt}");
                            Extentions.ResetConsoleColors();
                        }
                        else if (c == "|sss|")
                        {
                            char wt = _r.Next(2) == 0 ? '.' : ':';
                            Extentions.SetConsoleTextColor(ConsoleColor.Gray);
                            Console.Write($"{wt}");
                            Extentions.ResetConsoleColors();
                        }
                        else if (c == "|ccc|")
                        {
                            char wt = _r.Next(2) == 0 ? '#' : '*';
                            Extentions.SetConsoleTextColor(wt == '#' ? ConsoleColor.DarkRed : ConsoleColor.Red);
                            Console.Write($"{wt}");
                            Extentions.ResetConsoleColors();
                        }
                        else
                        {
                            if (c.HasConsoleColor() || c.Contains("<R>"))
                            {
                                List<char> ww = new List<char>();
                                List<char> qq = new List<char>();
                                char rsd = "ResetDefault".ToChar();
                                c = c.Replace("<R>", $"{rsd}");
                                for (int i = 0; i < Extentions.ConsoleColors.Length; i++)
                                {
                                    string cur = Extentions.ConsoleColors[i];
                                    char rpl = cur.ToChar();
                                    c = c.Replace($"<{cur}>", $"{rpl}");
                                    qq.Add(rpl);
                                    rpl = $"|{cur}|".ToChar();
                                    c = c.Replace($"|{cur}|", $"{rpl}");
                                    ww.Add(rpl);
                                }
                                foreach (char cc in c)
                                {
                                    if (qq.Contains(cc))
                                    {
                                        Extentions.SetConsoleTextColor(Extentions.ConsoleColors[qq.IndexOf(cc)].ToConsoleColor());
                                    }
                                    else if (ww.Contains(cc))
                                    {
                                        Extentions.SetConsoleBackgroundColor(Extentions.ConsoleColors[ww.IndexOf(cc)].ToConsoleColor());
                                    }
                                    else if (cc == rsd)
                                    {
                                        Extentions.ResetConsoleColors();
                                    }
                                    else
                                    {
                                        Console.Write(cc);
                                        if (Program.SLOWMOTION)
                                        {
                                            Thread.Sleep(3);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                Console.Write(c);
                                if (Program.SLOWMOTION)
                                {
                                    Thread.Sleep(3);
                                }
                            }
                        }
                    }
                    Console.CursorTop += 1;
                    Console.CursorLeft = ex < 0 ? 0 : FollowCamera == true ? (int)Math.Round(X) - Program.CameraX : RenderLimitLeft > 0 ? RenderLimitLeft : (int)Math.Round(X);
                }
            }
            if (Program.BEEPONDRAW)
            {
                Console.Beep();
            }
        }
    }
}