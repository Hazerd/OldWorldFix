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

        private static bool RequestConfirmation(bool enter, string trueMessage, string falseMessage)
        {
            while (true)
            {
                Console.Write("Would you like to apply this change? ({0}): ", enter ? "Y,n" : "y,N");
                switch (Console.ReadKey().Key)
                {
                    case ConsoleKey.Y:
                        Console.WriteLine("\r\n{0}", trueMessage);
                        return true;
                    case ConsoleKey.N:
                        Console.WriteLine("\r\n{0}", falseMessage);
                        return false;
                    case ConsoleKey.Enter:
                        Console.WriteLine("\r\n{0}", enter ? trueMessage : falseMessage);
                        return enter;
                    default:
                        Console.WriteLine("\r\nPlease enter 'Y' or 'N'");
                        break;
                }
            }
        }

        internal static bool FixPotionsConfirmation()
        {
            Console.WriteLine("In 1.9, the way potions determine their effects was changed.\r\n" +
            "In versions 1.9-1.12 any old potions will work fine. and any potions 'seen' by a player will be converted to new potions.\r\n" +
            "However, if the world is loaded in version 1.13 or later, any 'old' potions will be converted into water bottles.\r\n" +
            "This can be fixed, but the world will become incompatible with versions prior to 1.9.\r\n" +
            "If you plan to play the map in version 1.13+, press 'Y' to apply the potion fix.\r\n" +
            "If you plan to play the map in versions prior to 1.9, press 'N' to leave potions the way they are.\r\n");
            return RequestConfirmation(false, "Thank you. Potions WILL be fixed. This world will no longer be compatible with versions prior to 1.9.", "Thank you. Potions WILL NOT be fixed. This world will remain compatible with version prior to 1.9, but potions will automatically turn into water bottles if loaded in 1.13 or later.");
        }
    }
}
