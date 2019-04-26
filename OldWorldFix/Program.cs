using System;
using System.Collections.Generic;
using Substrate;
using Substrate.Nbt;
using Substrate.Core;
using Substrate.TileEntities;

namespace OldWorldFix
{
    class MainClass
    {
        private static readonly string[] PotionEffects = new string[]
        {
            "water", // 0
            "regeneration", // 1
            "swiftness", // 2
            "fire_resistance", // 3
            "poison", // 4
            "healing", // 5
            "night_vision", // 6
            "unused", // 7
            "weakness", // 8
            "strength", // 9
            "slowness", // 10
            "leaping", // 11
            "harming", // 12
            "water_breathing", // 13
            "invisibility", // 14
        };
        private static bool fixPotions;
        private static readonly Dictionary<string, int> fixedIssues = new Dictionary<string, int>
        {
            {"chest", 0},
            {"potion", 0},
        };
        private static readonly Dictionary<int, string> dimensions = new Dictionary<int, string>
        {
            {Dimension.NETHER, "The Nether"}, {Dimension.DEFAULT, "The Overworld"}, {Dimension.THE_END, "The End"}
        };
        public static void Main(string[] args)
        {
            TileEntityFactory.Register(TileEntityBrewingStand.TypeId, typeof(TileEntityBrewingStand));
            TileEntityFactory.Register(TileEntityChest.TypeId, typeof(TileEntityChest));
            TileEntityFactory.Register(TileEntityTrap.TypeId, typeof(TileEntityTrap));
            string path = args.Length == 0 ? "." : string.Join(" ", args);
            NbtWorld world = NbtWorld.Open(path);
            if (world == null)
            {
                Confirmation.WorldInvalidConfirmation();
                return;
            }
            Console.WriteLine("Working with world: {0}", world.Level.LevelName);
            fixPotions = Confirmation.FixPotionsConfirmation();
            LoopChunks(world, Dimension.DEFAULT);
            LoopChunks(world, Dimension.NETHER);
            LoopChunks(world, Dimension.THE_END);
            Console.Write("Finished searching {0}. Fixed:", world.Level.LevelName);
            Console.Write(" [{0} chest{1}]", fixedIssues["chest"], fixedIssues["chest"] == 1 ? "" : "s");
            Console.Write(" [{0} potion{1}]", fixedIssues["potion"], fixedIssues["potion"] == 1 ? "" : "s");
            Console.WriteLine();
        }

        private static void LoopChunks(NbtWorld world, int dim)
        {
            Console.WriteLine("Searching {0}", dimensions[dim]);
            int fixedChests = 0;
            int fixedPotions = 0;
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
                                        Console.WriteLine("Fixed chest");
                                        fixedChests++;
                                    }
                                    if (fixPotions)
                                    {
                                        fixedPotions += FixPotions(chunk, x, y, z);
                                    }
                                    break;
                                case BlockType.DISPENSER:
                                    if (fixPotions)
                                    {
                                        fixedPotions += FixPotions(chunk, x, y, z);
                                    }
                                    break;
                                case BlockType.BREWING_STAND:
                                    if (fixPotions)
                                    {
                                        fixedPotions += FixPotions(chunk, x, y, z);
                                    }
                                    break;
                            }
                        }
                    }
                }
                chunks.Save();
            }
            Console.Write("Finished searching {0}. Fixed:", dimensions[dim]);
            Console.Write(" [{0} chest{1}]", fixedChests, fixedChests == 1 ? "" : "s");
            Console.Write(" [{0} potion{1}]", fixedPotions, fixedPotions == 1 ? "" : "s");
            Console.WriteLine();
            fixedIssues["chest"] += fixedChests;
            fixedIssues["potion"] += fixedPotions;
        }

        private static bool FixChest(ChunkRef chunk, int x, int y, int z)
        {
            int id = chunk.Blocks.GetID(x, y, z);
            int data = chunk.Blocks.GetData(x, y, z);
            // Check if chest is a valid facing. Chests use the same facing values as ladders.
            if (id == BlockType.CHEST && !Enum.IsDefined(typeof(LadderOrientation), data))
            {
                chunk.Blocks.SetData(x, y, z, (int)LadderOrientation.SOUTH);
                return true;
            }
            return false;
        }

        private static int FixPotions(ChunkRef chunk, int x, int y, int z)
        {
            int fixedPotions = 0;
            TileEntity te = chunk.Blocks.GetTileEntity(x, y, z);
            switch (te)
            {
                case TileEntityChest chest:
                    fixedPotions = FixPotions(ref chest);
                    chunk.Blocks.SetTileEntity(x, y, z, chest);
                    return fixedPotions;
                case TileEntityTrap dispenser:
                    fixedPotions = FixPotions(ref dispenser);
                    chunk.Blocks.SetTileEntity(x, y, z, dispenser);
                    return fixedPotions;
                case TileEntityBrewingStand brewingStand:
                    fixedPotions = FixPotions(ref brewingStand);
                    chunk.Blocks.SetTileEntity(x, y, z, brewingStand);
                    return fixedPotions;
                default:
                    Console.WriteLine("UNIDENTIFIED TILE ENTITY: {0} at {1},{2},{3}", te.GetType(), te.X, te.Y, te.Z);
                    return 0;
            }
        }

        private static int FixPotions(ref TileEntityChest chest)
        {
            int fixedPotions = 0;
            for (int slot = 0; slot <= chest.Items.Capacity; slot++)
            {
                Item item = chest.Items[slot];
                if (item != null && item.ID == ItemInfo.Potion.NameID && FixPotion(ref item))
                {
                    Console.WriteLine("Fixed {0} of {1}", item.ID, item.Tag.Potion);
                    fixedPotions++;
                }
            }
            return fixedPotions;
        }

        private static int FixPotions(ref TileEntityTrap dispenser)
        {
            int fixedPotions = 0;
            for (int slot = 0; slot <= dispenser.Items.Capacity; slot++)
            {
                Item item = dispenser.Items[slot];
                if (item != null && item.ID == ItemInfo.Potion.NameID && FixPotion(ref item))
                {
                    Console.WriteLine("Fixed {0} of {1}", item.ID, item.Tag.Potion);
                    fixedPotions++;
                }
            }
            return fixedPotions;
        }

        private static int FixPotions(ref TileEntityBrewingStand brewingStand)
        {
            int fixedPotions = 0;
            for (int slot = 0; slot <= brewingStand.Items.Capacity; slot++)
            {
                Item item = brewingStand.Items[slot];
                if (item != null && item.ID == ItemInfo.Potion.NameID && FixPotion(ref item))
                {
                    Console.WriteLine("Fixed {0} of {1}", item.ID, item.Tag.Potion);
                    fixedPotions++;
                }
            }
            return fixedPotions;
        }

        private static bool FixPotion(ref Item item)
        {
            int slot = item.Slot.GetValueOrDefault();
            int damage = item.Damage;
            string potionEffect = PotionEffects[damage & 15];
            string potionPrefix = "";
            if ((damage & 32) == 32)
                potionPrefix = "strong_";
            else if ((damage & 64) == 64)
                potionPrefix = "long_";
            string potionId = ItemInfo.Potion.NameID;
            if ((damage & 8192) == 8192)
                potionId = ItemInfo.Potion.NameID;
            else if ((damage & 16384) == 16384)
                potionId = ItemInfo.SplashPotion.NameID;
            if (potionPrefix == "strong_" && (potionEffect == "fire_resistance" || potionEffect == "night_vision" || potionEffect == "weakness" || potionEffect == "slowness" || potionEffect == "water_breating" || potionEffect == "invisibility"))
                item.Tag.Potion = "minecraft:" + potionEffect;
            else if (potionPrefix == "long_" && (potionEffect == "healing" || potionEffect == "harming"))
                item.Tag.Potion = "minecraft:" + potionEffect;
            else
                item.Tag.Potion = "minecraft:" + potionPrefix + potionEffect;
            item.Damage = 0;
            item.ID = potionId;
            return true;
        }
    }
}
