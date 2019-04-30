using System;
using System.Collections.Generic;
using Substrate;
using Substrate.Nbt;
using Substrate.Core;
using Substrate.TileEntities;
using Substrate.Entities;

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
        private static bool fixSpawners;
        private static readonly Dictionary<string, int> fixedIssues = new Dictionary<string, int>
        {
            {"chest", 0},
            {"potion", 0},
            {"spawner", 0},
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
            TileEntityFactory.Register(TileEntityMobSpawner.TypeId, typeof(TileEntityMobSpawner));

            string path = args.Length == 0 ? "." : string.Join(" ", args);
            NbtWorld world = NbtWorld.Open(path);
            if (world == null)
            {
                Dialog.WorldInvalidDialog();
                return;
            }
            Console.WriteLine("Working with world: {0}", world.Level.LevelName);
            fixPotions = Dialog.FixPotionsDialog();
            fixSpawners = Dialog.FixSpawnersDialog();
            DateTime startTime = DateTime.Now;
            LoopChunks(world, Dimension.DEFAULT);
            LoopChunks(world, Dimension.NETHER);
            LoopChunks(world, Dimension.THE_END);
            TimeSpan time = DateTime.Now.Subtract(startTime);
            Console.Write("Finished searching {0} in {1}. Fixed:", world.Level.LevelName, time.ToString(@"h\:mm\:ss"));
            Console.Write(" [{0} chest{1}]", fixedIssues["chest"], fixedIssues["chest"] == 1 ? "" : "s");
            Console.Write(" [{0} potion{1}]", fixedIssues["potion"], fixedIssues["potion"] == 1 ? "" : "s");
            Console.Write(" [{0} spawner{1}]", fixedIssues["spawner"], fixedIssues["spawner"] == 1 ? "" : "s");
            Console.WriteLine();
            Dialog.NewDialog();
        }

        private static void LoopChunks(NbtWorld world, int dim)
        {
            DateTime startTime = DateTime.Now;
            Console.WriteLine("Searching {0}", dimensions[dim]);
            int fixedChests = 0;
            int fixedPotions = 0;
            int fixedSpawners = 0;
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
                                case BlockType.MONSTER_SPAWNER:
                                    if (fixSpawners && FixSpawner(chunk, x, y, z))
                                    {
                                        Console.WriteLine("Fixed Spawner");
                                        fixedSpawners++;
                                    }
                                    break;
                            }
                        }
                    }
                }
                chunks.Save();
            }
            TimeSpan time = DateTime.Now.Subtract(startTime);
            Console.Write("Finished searching {0} in {1}. Fixed:", dimensions[dim], time.ToString(@"h\:mm\:ss"));
            Console.Write(" [{0} chest{1}]", fixedChests, fixedChests == 1 ? "" : "s");
            Console.Write(" [{0} potion{1}]", fixedPotions, fixedPotions == 1 ? "" : "s");
            Console.Write(" [{0} potion spawner{1}]", fixedSpawners, fixedSpawners == 1 ? "" : "s");
            Console.WriteLine();
            fixedIssues["chest"] += fixedChests;
            fixedIssues["potion"] += fixedPotions;
            fixedIssues["spawner"] += fixedSpawners;
        }

        #region Fix Chests
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
        #endregion

        #region Fix Potions
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
                    fixedPotions++;
                }
            }
            return fixedPotions;
        }

        private static bool FixPotion(ref Item item)
        {
            if (item.Source.ContainsKey("tag") && item.Source["tag"].ToTagCompound().ContainsKey("CustomPotionEffects"))
                return false;
            if (string.IsNullOrEmpty(item.Tag.Potion))
            {
                if (item.Damage == 0)
                    return false;
                string potionEffect = PotionEffects[item.Damage & 15];
                string potionPrefix = "";
                if ((item.Damage & 32) == 32)
                    potionPrefix = "strong_";
                else if ((item.Damage & 64) == 64)
                    potionPrefix = "long_";
                string potionId = ItemInfo.Potion.NameID;
                if ((item.Damage & 8192) == 8192)
                    potionId = ItemInfo.Potion.NameID;
                else if ((item.Damage & 16384) == 16384)
                    potionId = ItemInfo.SplashPotion.NameID;
                if (potionPrefix == "strong_" && (potionEffect == "fire_resistance" || potionEffect == "night_vision" || potionEffect == "weakness" || potionEffect == "slowness" || potionEffect == "water_breating" || potionEffect == "invisibility"))
                    item.Tag.Potion = "minecraft:" + potionEffect;
                else if (potionPrefix == "long_" && (potionEffect == "healing" || potionEffect == "harming"))
                    item.Tag.Potion = "minecraft:" + potionEffect;
                else
                    item.Tag.Potion = "minecraft:" + potionPrefix + potionEffect;
                item.Damage = 0;
                item.ID = potionId;
                Console.WriteLine("Fixed {0} of {1}", item.ID.Substring(10), item.Tag.Potion.Substring(10));
                return true;
            }
            return false;
        }

        #endregion

        #region Fix Spawners
        private static bool FixSpawner(ChunkRef chunk, int x, int y, int z)
        {
            if (chunk.Blocks.GetTileEntity(x, y, z) is TileEntityMobSpawner spawner && spawner.SpawnData != null)
            {
                if (FixSpawnData(spawner.SpawnData))
                {
                    if (spawner.Source.ContainsKey("SpawnPotentials"))
                        spawner.Source.Remove("SpawnPotentials");
                    if (spawner.Source.ContainsKey("EntityId"))
                        spawner.Source.Remove("EntityId");
                    spawner.EntityID = null;
                    chunk.Blocks.SetTileEntity(x, y, z, spawner);
                    return true;
                }
            }
            return false;
        }

        private static bool FixSpawnData(TagNodeCompound spawnData)
        {
            if (!spawnData.ContainsKey("potionValue") && !spawnData.ContainsKey("Potion"))
                return false;
            int damage = 0;
            spawnData["id"] = new TagNodeString("minecraft:potion");
            if (spawnData.ContainsKey("potionValue"))
            {
                damage = spawnData["potionValue"].ToTagInt();
                spawnData.Remove("potionValue");
            }
            if (!spawnData.ContainsKey("Potion"))
                spawnData["Potion"] = new TagNodeCompound();
            TagNodeCompound potion = spawnData["Potion"].ToTagCompound();
            if (!potion.ContainsKey("tag"))
                potion["tag"] = new TagNodeCompound();
            TagNodeCompound tag = potion["tag"].ToTagCompound();
            if (!potion.ContainsKey("Count"))
                potion["Count"] = new TagNodeByte(1);
            if (potion.ContainsKey("Damage"))
            {
                if (damage == 0)
                    damage = potion["Damage"].ToTagInt();
                potion.Remove("Damage");
            }
            potion["id"] = new TagNodeString(ItemInfo.SplashPotion.NameID);
            if (damage != 0)
            {
                string potionEffect = PotionEffects[damage & 15];
                string potionPrefix = "";
                if ((damage & 32) == 32)
                    potionPrefix = "strong_";
                else if ((damage & 64) == 64)
                    potionPrefix = "long_";
                if (potionPrefix == "strong_" && (potionEffect == "fire_resistance" || potionEffect == "night_vision" || potionEffect == "weakness" || potionEffect == "slowness" || potionEffect == "water_breating" || potionEffect == "invisibility"))
                    tag["Potion"] = new TagNodeString("minecraft:" + potionEffect);
                else if (potionPrefix == "long_" && (potionEffect == "healing" || potionEffect == "harming"))
                    tag["Potion"] = new TagNodeString("minecraft:" + potionEffect);
                else
                    tag["Potion"] = new TagNodeString("minecraft:" + potionPrefix + potionEffect);
            }
            return true;
        }

        #endregion
    }
}
