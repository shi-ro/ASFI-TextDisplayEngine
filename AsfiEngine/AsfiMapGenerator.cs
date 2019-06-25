using AsfiEngine.Extra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsfiEngine
{
    public static class AsfiMapGenerator
    {
        public static int ChunksGenerated = 0;
        public static int ChunkSize = 128;//128;
        public static int Variance = 100;
        public static int WaterLevel = 103;
        public static int IronLevel = 103;
        public static int CopperLevel = 103;
        public static int StoneLevel = 103;
        public static int CoalLevel = 103;
        public static int CrystalLevel = 108;
        public static int MinimumRichness = 1000;
        public static int MaximumRichness = 10000;

        private static int _priority = 0;
        private static Image _crystal = new Image(new string[,] { { "<Red>|DarkRed|\\<Magenta>@<Red>\\<Magenta>@<Red>||<Magenta>@<Red>/<Magenta>@<Red>/" }, { "<Magenta>@<Red>#<Magenta>&<Red>####<Magenta>&<Red>#<Magenta>@" }, { "<Magenta>@&<Red>##<Magenta>&<Red>###<Magenta>&@" }, { "<Red>/<Magenta>@<Red>/<Magenta>@<Red>||<Magenta>@<Red>\\<Magenta>@<Red>\\<R>" } });
        private static Image _iron = new Image(new string[,] { { "<White>|DarkGray|;,;,;,;,;," }, { ",;!;,;;!;;" }, { ";!;;;!;;;," }, { ",;,;,;,;,;<R>" } });
        private static Image _stone = new Image(new string[,] { { "<Gray>|DarkGray|:.:.\".:.::" }, { ".'..'.\"..\"" }, { ":..'.'.'.." }, { "'.:.:.:.\":<R>" } });
        private static Image _copper = new Image(new string[,] { { "<Yellow>|DarkGray|.:.:':.:.." }, { ":\"::\":'::'" }, { ".::\":\":\"::" }, { "':.:.:.:'.<R>" } });
        private static Image _water = new Image(new string[,] { { "<White>|DarkBlue|-  _ _ -- " }, { " --  _- _ " }, { " _ -- _  -" }, { " -  --  _ <R>" } });
        private static Random _r = new Random();

        public static string[,] GetChunk()
        {
            ChunksGenerated += 1;
            Log.Write($"Generating map chunk #{ChunksGenerated} ...");
            string[,] ret = new string[ChunkSize,ChunkSize];
            int[,] water = NoiseMap.GetNotSmoothedNoise(ChunkSize, ChunkSize, 100);
            water = water.MeanBlur(10).MeanBlur(10).MeanBlur(10);
            water = water.ChangeVariance(110);
            Log.Write("Generated water.");

            int[,] iron = NoiseMap.GetNotSmoothedNoise(ChunkSize, ChunkSize, 50);
            iron = iron.MedianBlur(10).MedianBlur(10).MeanBlur(10);
            iron = iron.ChangeVariance(110);
            Log.Write("Generated iron.");

            int[,] copper = NoiseMap.GetNotSmoothedNoise(ChunkSize, ChunkSize, 50);
            copper = copper.MedianBlur(10).MedianBlur(10).MeanBlur(10);
            copper = copper.ChangeVariance(110);
            Log.Write("Generated copper.");

            int[,] stone = NoiseMap.GetNotSmoothedNoise(ChunkSize, ChunkSize, 50);
            stone = stone.MeanBlur(10).MedianBlur(10).MedianBlur(10);
            stone = stone.ChangeVariance(110);
            Log.Write("Generated stone.");

            int[,] crystal = NoiseMap.GetNotSmoothedNoise(ChunkSize, ChunkSize, 50);
            crystal = crystal.MeanBlur(10).MedianBlur(10).MedianBlur(10);
            crystal = crystal.ChangeVariance(110);
            Log.Write("Generated crystal.");
            Log.Write("Writing chunk ...");
            for (int y = 0; y < ChunkSize; y++)
            {
                for(int x = 0; x < ChunkSize; x++)
                {
                    // make order:
                    // - stone
                    // - coal
                    // - iron/copper ( random if overlap )
                    // - crystal
                    // - water 

                    int w = water[y, x];
                    int i = iron[y, x];
                    int p = copper[y, x];
                    int s = stone[y, x];
                    int c = crystal[y, x];
                    string ps = $"{x},{y}";
                    int v = _r.Next(MinimumRichness*ChunksGenerated, MaximumRichness*ChunksGenerated);
                    if (w >= WaterLevel)
                    {
                        ret[y, x] = "|~~~|";
                        Program.AddMapResource(ps, new Resource("Normal Water", _water, ResourceType.Water, 9999999));
                    }
                    else if(c >= CrystalLevel)
                    {
                        ret[y, x] = "|ccc|";
                        Program.AddMapResource(ps, new Resource("Crystal Vein", _crystal, ResourceType.Crystal, v));
                    }
                    else if (i >= IronLevel && _priority == 0)
                    {
                        ret[y, x] = "|iii|";
                        Program.AddMapResource(ps, new Resource("Iron Vein", _iron, ResourceType.Iron, v));
                        _priority = 1;
                    }
                    else if (p >= CopperLevel && _priority == 1)
                    {
                        ret[y, x] = "|ppp|";
                        Program.AddMapResource(ps, new Resource("Copper Vein", _copper, ResourceType.Copper, v));
                        _priority = 0;
                    }
                    else if (p >= CopperLevel && _priority == 0)
                    {
                        ret[y, x] = "|ppp|";
                        Program.AddMapResource(ps, new Resource("Copper Vein", _copper, ResourceType.Copper, v));
                        _priority = 1;
                    }   
                    else if (i >= IronLevel && _priority == 1)
                    {
                        ret[y, x] = "|iii|";
                        Program.AddMapResource(ps, new Resource("Iron Vein", _iron, ResourceType.Iron, v));
                        _priority = 0;
                    }
                    else if(s >= StoneLevel)
                    {
                        ret[y, x] = "|sss|";
                        Program.AddMapResource(ps, new Resource("Stone Vein", _stone, ResourceType.Stone, v));
                    }
                    else
                    {
                        ret[y, x] = " ";
                    }
                }
            }
            Log.Write("Finished writing chunk.");
            return ret;
        }
    }
}
