using AsfiEngine.Extra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsfiEngine
{
    public class MenuObject
    {
        public bool Stackable { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public char Charachter { get; set; }
        public string ID { get; private set; }
        public bool Placable { get; set; }
        public Image PlacedImage = Program.NoImage;
        public bool Rotatable { get; set; }
        public ItemType Type = ItemType.None;
        public MenuObject(string name, char charachter, string description, bool stackable = true)
        {
            Name = name;
            Charachter = charachter;
            Description = description;
            Stackable = stackable;
            ID = Program.RSG.Generate("LlnLlnLln");
            Program.RSG.AddExistingString(ID);
        }
    }

    #region Game Items

    public enum ItemType
    {
        None = 0,
        Miner = 1
    }

    public class MO_Miner : MenuObject
    {
        private List<Resource> _resources = new List<Resource>();
        private List<MapObject> _subs = new List<MapObject>();
        public string[,] RootPicture = new string[,] { { "┌", "─", "┐" }, { "│", "M", "│" }, { "└", "─", "┘" } };

        public GameObject _object;
        public int ResourcesMined = 0;
        public ConsoleColor ForegroundColor = ConsoleColor.White;
        public ConsoleColor BackgroundColor = ConsoleColor.Black;
        public string OutputTile = "0,0";
        private Direction m_fdir = Direction.Up;
        public Direction FacingDirection
        {
            get
            {
                return m_fdir;
            }
            set
            {
                m_fdir = value;
                PlacedImage.CurrentFrame = RootPicture;
                switch (value)
                {
                    case Direction.Down:
                        PlacedImage.CurrentFrame[2, 1].Replace("─", "v");
                        break;
                    case Direction.Up:
                        PlacedImage.CurrentFrame[0, 1].Replace("─", "^");
                        break;
                    case Direction.Left:
                        PlacedImage.CurrentFrame[1, 0].Replace("│", "<");
                        break;
                    case Direction.Right:
                        PlacedImage.CurrentFrame[1, 2].Replace("│", ">");
                        break;
                }
            }
        }

        public MO_Miner() : base("Electric Mining Drill",'M',"Drill which mines ore on which it is placed in a 3x3 area.")
        {
            Type = ItemType.Miner;
            FacingDirection = Direction.Up;
            Placable = true;
            Rotatable = true;
        }

        public void ResetRootImage()
        {
            RootPicture = new string[,] { { "┌", "─", "┐" }, { "│", "M", "│" }, { "└", "─", "┘" } };
        }

        public void Rotate()
        {
            Direction d = FacingDirection;
            switch (d)
            {
                case Direction.Down:
                    FacingDirection = Direction.Left;
                    break;
                case Direction.Up:
                    FacingDirection = Direction.Right;
                    break;
                case Direction.Left:
                    FacingDirection = Direction.Up;
                    break;
                case Direction.Right:
                    FacingDirection = Direction.Down;
                    break;
            }
        }
    }

    #endregion
}