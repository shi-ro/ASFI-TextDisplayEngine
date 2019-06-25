using AsfiEngine.Extra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AsfiEngine
{
    class Program
    {
        // object lists
        public static List<GameObject> Objects = new List<GameObject>();
        public static Dictionary<string, Resource> MapResources = new Dictionary<string, Resource>();
        public static Dictionary<string, MapObject> MapObjects = new Dictionary<string, MapObject>();

        // important presets
        public static RandomStringGenerator RSG = new RandomStringGenerator();

        // camera settings
        public static int CameraX = 0;
        public static int CameraY = 0;

        // game screen settings 
        public static readonly int WIDTH = 100;
        public static readonly int HEIGHT = 30;
        public static readonly bool LOCK = true;
        public static readonly bool SLOWMOTION = false;
        public static readonly bool BEEPONDRAW = false;
        public static readonly bool DEVMODE = true;

        // mainscreen sprites
        public static SimpleSprite S_MAP = null;
        public static SimpleSprite S_UI = null;
        public static SimpleSprite S_INV = null;

        //null image
        public static Image NoImage = new Image(new string[,] { { } });
        
        // inventory menu vars
        public static bool InventoryOpen = false;
        public static int InventoryPage = 0;
        public static int TotalInventoryPages = 1;
        public static bool InventoryScrollRight = false;
        public static bool InventoryScrollLeft = false;
        public static List<InventoryItem> InventoryItems = new List<InventoryItem>();
        public static int MaximumStackSize = 999;
        public static readonly int InventoryLinesPerPage = 16;
        public static int InventoryHighlightedLine = -1;
        public static bool InventoyItemClicked = false;
        public static Image InventoryItemInHandImage = NoImage;
        public static InventoryItem InventoryItemInHand = null;

        static void Main(string[] args)
        {
            Log.Clear();
            Log.Write("Initializing ...");
            Console.WindowWidth = WIDTH;
            Console.WindowHeight = HEIGHT;
            if(LOCK)
            {
                Log.Write("Buffers locked.");
                Console.BufferWidth = WIDTH + 1;
                Console.BufferHeight = HEIGHT + 1;
            } else
            {
                Log.Write("Buffers unlocked.");
                Console.BufferWidth = 9000;
                Console.BufferHeight = 9000;
            }
            Console.CursorVisible = false;
            Console.Title = "Asfi 0.0.1E";
            Extentions.Initialize();
            Log.Write("Finished console initialization.");
            Image map = new Image(AsfiMapGenerator.GetChunk());
            S_MAP = new SimpleSprite(0, 0, map, false);
            S_MAP.RenderLimitBottom = 24;
            Objects.Add(S_MAP);
            foreach (GameObject go in Objects)
            {
                go.Draw();
                go.Update();
            }
            DrawUI("",NoImage);
            if(DEVMODE)
            {
                Log.Write("Developer mode is on");
                Log.Write("Loading test objects");
                LoadTestObjects();
            }
        }

        private static void LoadTestObjects()
        {
            MenuObject stack = new MenuObject("Stackable Item",'S',"If this text displays this means that the description code works");
            MenuObject nonstack = new MenuObject("Unstackable Item", 'U', "This should be a nonstackable item.", false);
            MenuObject longname = new MenuObject("This name is supposed to be super long and break the limit", 'L', "This should have an unreasonably large name");
            AddItemToInventory(new MO_Miner());
            AddItemToInventory(stack);
            AddItemToInventory(stack);
            AddItemToInventory(stack);
            AddItemToInventory(stack);
            AddItemToInventory(stack);
            AddItemToInventory(stack);
            AddItemToInventory(stack);
            AddItemToInventory(nonstack);
            AddItemToInventory(nonstack);
            AddItemToInventory(longname);
            AddItemToInventory(longname);
            for(int i = 0; i < 20; i++)
            {
                string str = RSG.Generate("LlnLlnLln");
                AddItemToInventory(new MenuObject($"TST-{str}", (char)i,RSG.Generate(999)));
                RSG.AddExistingString(str);
            }
        }

        public static void AddItemToInventory(MenuObject item)
        {
            if(item.Stackable)
            {
                for(int i = 0; i < InventoryItems.Count; i++)
                {
                    InventoryItem cur = InventoryItems[i];
                    if (cur.Item.Stackable && cur.Ammount < MaximumStackSize && cur.Item.Name == item.Name && cur.Item.Charachter == item.Charachter)
                    {
                        cur.Ammount += 1;
                        return;
                    }
                }
            }
            InventoryItems.Add(new InventoryItem(item));
        }

        public static void AddMapResource(string pos, Resource obj)
        {
            if(MapResources.Keys.Contains(pos))
            {
                MapResources[pos] = obj;
            }
            else
            {
                MapResources.Add(pos, obj);
            }
        }

        // thick frame        |  ═║╔╗╚╝╠╣╦╩╬
        // thin frame         |  ─│┌┐└┘├┤┬┴┼
        // frame transitions  |  ╒╕╓╖╘╛╙╜╞╡╟╢╤╧╥╨╪╫

        public static void DrawInventory(int page = 0, MenuObject selected = null)
        {
            Image ui = NoImage;

            // if no object is selected
            if (selected==null)
            {
                TotalInventoryPages = (InventoryItems.Count() / (float)InventoryLinesPerPage) > (int)(InventoryItems.Count() / (float)InventoryLinesPerPage) ? (int)(InventoryItems.Count() / (float)InventoryLinesPerPage) + 1 : (int)(InventoryItems.Count() / (float)InventoryLinesPerPage);
                string top = $"╔═════╤{'═'.Mult(29)}╗";
                string lnt = $"║ Amt │ Name{' '.Mult(24)}║";
                string lnt2 = $"╟─────┼{'─'.Mult(29)}╢";
                string lnm = $"║     │{' '.Mult(29)}║";
                string lne = $"╟─────┘{' '.Mult(29)}║";
                string pget = $"╟{"───┬".Mult(8)}───╢";
                string pgem = $"║";
                string bot = $"╚{"═══╧".Mult(8)}═══╝";
                InventoryScrollLeft = page > 0;
                if (page > 0)
                {
                    pgem += " <White>◄<R> │";
                }
                else
                {
                    pgem += " <DarkGray>◄<R> │";
                }
                int total = page;
                for(int  i = 0; i < 7; i++)
                {
                    string tag = i==0?"<White>":"<DarkGray>";
                    //visible gradient to line
                    //tag = i == 1 ? "<Gray>" : tag;
                    if (total<TotalInventoryPages)
                    {
                        pgem += $" {tag}{((page + i + 1 )>9 ? $"{(char)(page+i-9+65)}" : $"{page + i + 1}")}<R> │";
                    }
                    else
                    {
                        pgem += $"   │";
                    }
                    total += 1;
                }
                InventoryScrollRight = page < TotalInventoryPages - 1;
                if (page<TotalInventoryPages-1)
                {
                    pgem += $" <White>►<R> ║";
                }
                else
                {
                    pgem += $" <DarkGray>►<R> ║";
                }
                string[,] ite = new string[,] { { top }, { lnt }, { lnt2 }, { lnm }, { lnm }, { lnm }, { lnm }, { lnm }, { lnm }, { lnm }, { lnm }, { lnm }, { lnm }, { lnm }, { lnm }, { lnm }, { lnm }, { lnm }, { lnm }, { lne }, { pget }, { pgem }, { bot } };
                // load items from set page
                if (InventoryItems.Count==0)
                {
                    top = $"╔{'═'.Mult(35)}╗";
                    lnm = $"║{' '.Mult(35)}║";
                    ite = new string[,] { { top }, { lnm }, { lnm }, { lnm }, { lnm }, { lnm }, { lnm }, { lnm }, { lnm }, { lnm }, { lnm }, { lnm }, { lnm }, { lnm }, { lnm }, { lnm }, { lnm }, { lnm }, { lnm }, { lnm }, { pget }, { pgem }, { bot } };
                    ite[10, 0] = $"║ Your inventory contains no items! ║";
                } else
                {
                    int le = 3;
                    for(int i = InventoryLinesPerPage*page; le < 19; i++)
                    {
                        if (i>=InventoryItems.Count)
                        {
                            break;
                        }
                        InventoryItem cur = InventoryItems[i];
                        string tag = le == InventoryHighlightedLine ?"<White>|DarkGray|":"";
                        ite[le, 0] = $"║ {cur.Ammount}{" ".Mult(3 - $"{cur.Ammount}".Length)} │{tag} {(cur.Item.Name.Length > 27 ? cur.Item.Name.Substring(0, 24) + "..." : cur.Item.Name + " ".Mult(27 - cur.Item.Name.Length))} <R>║";
                        le += 1;
                    }
                }

                ui = new Image(ite);
            }

            S_INV = new SimpleSprite(62, 0, ui);
            S_INV.Draw();
        }

        public static void DrawUI(string name, Image image)
        {
            //═║╔╗╚╝╠╣╦╩╬
            string top = $"╔{'═'.Mult(10)}╦════ {name} {'═'.Mult(WIDTH-20-name.Length)}╗";
            string ln =  $"║{' '.Mult(10)}║{' '.Mult(WIDTH-14)}║";
            string bot = $"╚{'═'.Mult(10)}╩{'═'.Mult(WIDTH-14)}╝";
            Image ui = new Image(new string[,] { { top }, { ln }, { ln } , { ln }, { ln }, { bot } });
            SimpleSprite img = new SimpleSprite(1, 24, image);
            img.RenderLimitBottom = 29;
            img.RenderLimitRight = 10;
            S_UI = new SimpleSprite(0, 23, ui);
            S_UI.Draw();
            img.Draw();
            img.Picture.Advance = true;
        }
    }
}
