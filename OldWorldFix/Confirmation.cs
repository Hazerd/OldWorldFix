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
    }
}
