using System;
using System.Collections.Generic;

namespace OldWorldFix
{
    public static class Confirmation
    {
        internal static bool WorldInvalidConfirmation()
        {
            Console.WriteLine("World not found.\r\n" +
            "Usage: OldWorldFix.exe /path/to/world_directory\r\n" +
            "'world_directory' is the folder containing 'level.dat'\r\n" +
            "You can also copy 'OldWorldFix.exe' and 'Substrate.dll' into the world directory and just run 'OldWorldFix.exe'\r\n" +
            "Please press any key to close the program" );
            Console.ReadKey();
            return true;
        }
    }
}
