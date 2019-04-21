using System;
using System.Collections.Generic;
using Substrate;
using Substrate.Core;

namespace OldWorldFix
{
    class MainClass
    {
        private static readonly Dictionary<string, int> fixedIssues = new Dictionary<string, int>
        {
            {"chest", 0}
        };
        private static readonly Dictionary<int, string> dimensions = new Dictionary<int, string>
        {
            {Dimension.NETHER, "The Nether"}, {Dimension.DEFAULT, "The Overworld"}, {Dimension.THE_END, "The End"}
        };
        public static void Main(string[] args)
        {
            string path = args.Length == 0 ? "." : string.Join(" ", args);
            NbtWorld world = NbtWorld.Open(path);
            if (world == null)
            {
                Console.WriteLine("World not found.\r\n" +
                "Usage: OldWorldFix.exe /path/to/world_directory\r\n" +
                "'world_directory' is the folder containing 'level.dat'\r\n" +
                "You can also copy 'OldWorldFix.exe' and 'Substrate.dll' into the world directory and just run 'OldWorldFix.exe'");
                return;
            }
            Console.WriteLine("Working with world: {0}", world.Level.LevelName);
            LoopChunks(world, Dimension.DEFAULT);
            LoopChunks(world, Dimension.NETHER);
            LoopChunks(world, Dimension.THE_END);
            Console.WriteLine("Finished searching {0}. Fixed {1} chests.", world.Level.LevelName, fixedIssues["chest"]);
        }

        private static void LoopChunks(NbtWorld world, int dim)
        {
            Console.WriteLine("Searching {0}", dimensions[dim]);
            int fixedChests = 0;
            IChunkManager chunks = world.GetChunkManager(dim);
            foreach (ChunkRef chunk in chunks)
            {
                for (int x = 0; x < chunk.Blocks.XDim; x++)
                {
                    for (int z = 0; z < chunk.Blocks.ZDim; z++)
                    {
                        for (int y = 0; y < chunk.Blocks.YDim; y++)
                        {
                            switch (chunk.Blocks.GetID(x, y, z))
                            {
                                case BlockType.CHEST:
                                    if (FixChest(chunk, x, y, z))
                                    {
                                        fixedChests++;
                                    }
                                    break;
                            }
                        }
                    }
                }
                chunks.Save();
            }
            Console.WriteLine("Finished searching {0}. Fixed {1} chests.", dimensions[dim], fixedChests);
            fixedIssues["chest"] += fixedChests;
        }

        private static bool FixChest(ChunkRef chunk, int x, int y, int z)
        {
            int id = chunk.Blocks.GetID(x, y, z);
            int data = chunk.Blocks.GetData(x, y, z);
            // Check if chest is a valid facing. Chests use the same facing values as ladders.
            if (id == BlockType.CHEST && !Enum.IsDefined(typeof(LadderOrientation), data))
            {
                Console.WriteLine("Found invalid chest at {0},{1},{2}", chunk.X * 16 + x, y, chunk.Z * 16 + z);
                chunk.Blocks.SetData(x, y, z, (int)LadderOrientation.SOUTH);
                return true;
            }
            return false;
        }
    }
}
