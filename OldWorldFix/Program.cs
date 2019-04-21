using System;
using System.Collections.Generic;
using Substrate;
using Substrate.Core;
using Substrate.Nbt;
using Substrate.Entities;
using Substrate.TileEntities;

namespace OldWorldFix
{
    class MainClass
    {
        private static bool fixPaintings;
        private static readonly Dictionary<string, int> fixedIssues = new Dictionary<string, int>
        {
            {"chest", 0}, {"painting", 0}
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
            // Explain the problem with paintings, and ask if they should be fixed.
            Console.WriteLine("Starting in 1.8, paintings changed how their positions are determined.\r\n" +
            "This causes old paintings to 'pop off' the wall. This behavior can be fixed.\r\n" +
            "However, if the world is loaded in a version prior to 1.8, the paintings will 'pop off'.\r\n");
            ConsoleKeyInfo cki;
            do
            {
                Console.Write("Would you like to apply this change? (y,N): ");
                cki = Console.ReadKey();
                Console.WriteLine();
                switch (cki.Key)
                {
                    case ConsoleKey.Y:
                        Console.WriteLine("Thank you. Paintings WILL be fixed.");
                        fixPaintings = true;
                        break;
                    case ConsoleKey.N:
                    case ConsoleKey.Enter:
                        Console.WriteLine("Thank you. Paintings WILL NOT be fixed.");
                        fixPaintings = false;
                        break;
                    default:
                        Console.WriteLine("Please enter 'Y' or 'N'");
                        break;
                }
            } while (!(cki.Key == ConsoleKey.Y || cki.Key == ConsoleKey.N || cki.Key == ConsoleKey.Enter));

            LoopChunks(world, Dimension.DEFAULT);
            LoopChunks(world, Dimension.NETHER);
            LoopChunks(world, Dimension.THE_END);
            Console.WriteLine("Finished searching {0}. Fixed {1} chests and {2} paintings.", world.Level.LevelName, fixedIssues["chest"], fixedIssues["painting"]);
        }

        private static void LoopChunks(NbtWorld world, int dim)
        {
            Console.WriteLine("Searching {0}", dimensions[dim]);
            int fixedChests = 0;
            int fixedPaintings = 0;
            IChunkManager chunks = world.GetChunkManager(dim);
            foreach (ChunkRef chunk in chunks)
            {

                foreach (TypedEntity entity in chunk.Entities)
                {
                    if (entity.ID == "Painting" && fixPaintings && FixPainting(new EntityPainting(entity)))
                    {
                        fixedPaintings++;
                    }
                }

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
            Console.WriteLine("Finished searching {0}. Fixed {1} chests and {2} paintings", dimensions[dim], fixedChests, fixedPaintings);
            fixedIssues["chest"] += fixedChests;
            fixedIssues["painting"] += fixedPaintings;
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

        private static bool FixPainting(EntityPainting painting)
        {
            Console.WriteLine("Found painting at {0},{1},{2}", painting.Position.X, painting.Position.Y, painting.Position.Z);
            switch (painting.Direction)
            {
                case EntityPainting.DirectionType.NORTH:
                    if (painting.TileZ > painting.Position.Z)
                        painting.TileZ -= 1;
                    else
                        return false;
                    break;
                case EntityPainting.DirectionType.WEST:
                    if (painting.TileX > painting.Position.X)
                        painting.TileX -= 1;
                    else
                        return false;
                    break;
                case EntityPainting.DirectionType.SOUTH:
                    if (painting.TileZ > painting.Position.Z)
                        painting.TileZ += 1;
                    else
                        return false;
                    break;
                case EntityPainting.DirectionType.EAST:
                    if (painting.TileX > painting.Position.X)
                        painting.TileX += 1;
                    else
                        return false;
                    break;
                default:
                    Console.WriteLine("Found painting with invalid Dir tag: {0} at {1},{2},{3}", painting.Direction, painting.Position.X, painting.Position.Y, painting.Position.Z);
                    return false;
            }
            Console.WriteLine("Fixed painting at {0},{1},{2}", painting.Position.X, painting.Position.Y, painting.Position.Z);
            return true;
        }
    }
}
