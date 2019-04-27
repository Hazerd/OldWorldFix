using System;
using System.Collections.Generic;

namespace OldWorldFix
{
    public static class Dialog
    {
        #region NewDialog Variants

        // Wait until any key is pressed.
        internal static void NewDialog()
        {
            Console.Write("Please press any key to continue...");
            Console.ReadKey();
        }

        // Relay a message to the user and wait until any key is pressed.
        internal static void NewDialog(string[] message)
        {
            Console.WriteLine(string.Join("\r\n", message));
            Console.Write("Please press any key to continue...");
            Console.ReadKey();
        }

        // Relay a message to a user with a yes-or-no question.
        // 'message' is sent once, joined by newlines.
        // 'question' is sent after message, and after the user presses an invalid key.
        // 'trueMessage' is sent if the user answers yes.
        // 'falseMessage' is sent if the user answers no.
        // 'enterKey' sets whether pressing enter results in yes or no.
        internal static bool NewDialog(string[] message, string question, string trueMessage, string falseMessage, bool enterKey)
        {
            while (true)
            {
                Console.WriteLine(string.Join("\r\n", message));
                Console.Write("{0} ({1}): ", question, enterKey ? "Y,n" : "y,N");
                switch (Console.ReadKey().Key)
                {
                    case ConsoleKey.Y:
                        Console.WriteLine("\r\n{0}", trueMessage);
                        return true;
                    case ConsoleKey.N:
                        Console.WriteLine("\r\n{0}", falseMessage);
                        return false;
                    case ConsoleKey.Enter:
                        Console.WriteLine("\r\n{0}", enterKey ? trueMessage : falseMessage);
                        return enterKey;
                    default:
                        Console.WriteLine("\r\nPlease enter 'Y' or 'N'");
                        break;
                }
            }
        }

        // Relay a message to the user with a multiple-choice question.
        // 'T' is whatever type should be returned. bool, string, int, Item, etc.
        // 'message' is sent once.
        // 'question' is sent after message, and after the user presses an invalid key.
        // 'options' specifies which keys return which values, and the message sent to the user after selecting that option.
        // 'enterKey' is the key that should be evaluated if the 'Enter' key is pressed.
        internal static T NewDialog<T>(string[] message, string question, Dictionary<ConsoleKey, (T, string)> options, ConsoleKey enterKey = ConsoleKey.Enter)
        {
            Console.WriteLine(string.Join("\r\n", message));
            string keyOptions = "";
            foreach (ConsoleKey key in options.Keys)
            {
                if (keyOptions != "")
                    keyOptions += ",";
                keyOptions += (key == enterKey ? key.ToString().ToUpper() : key.ToString().ToLower());
            }
            while (true)
            {
                Console.Write("{0} ({1})", question, keyOptions);
                ConsoleKeyInfo cki = Console.ReadKey();
                Console.WriteLine();
                if (options.ContainsKey(cki.Key))
                {
                    Console.WriteLine(options[cki.Key].Item2);
                    return options[cki.Key].Item1;
                }
                Console.WriteLine("Please enter one of the following keys: ({0})", keyOptions);
            }
        }
        #endregion

        internal static void WorldInvalidDialog()
        {
            NewDialog(
                new string[]
                {
                "World not found.",
                "Usage: OldWorldFix.exe /path/to/world_directory",
                "'world_directory' is the folder containing 'level.dat'",
                "You can also copy 'OldWorldFix.exe' and 'Substrate.dll' into the world directory and just run 'OldWorldFix.exe'"
                }
            );
        }

        internal static bool FixPotionsDialog()
        {
            return NewDialog(
                new string[]
                {
                "In 1.9, the way potions determine their effects was changed.",
                "In versions 1.9-1.12 any old potions will work fine, and any potions 'seen' by a player will be converted to new potions.",
                "However, if the world is loaded in version 1.13 or later, any 'old' potions will be converted into water bottles.",
                "This can be fixed, but the world will become incompatible with versions prior to 1.9.",
                "If you plan to play the map in version 1.13+, press 'Y' to apply the potion fix.",
                "If you plan to play the map in versions prior to 1.9, press 'N' to leave potions the way they are."
                },
                "Would you like to fix potions?",
                "Thank you. Potions WILL be fixed. This world will no longer be compatible with versions prior to 1.9.",
                "Thank you. Potions WILL NOT be fixed. This world will remain compatible with version prior to 1.9, but potions will automatically turn into water bottles if loaded in 1.13 or later.",
                false
            );
        }
    }
}
