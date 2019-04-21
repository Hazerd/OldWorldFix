using System;
using Substrate;
using Substrate.Core;

namespace OldWorldFix
{
    class MainClass
    {
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
        }
    }
}
