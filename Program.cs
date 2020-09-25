using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;

namespace UnixShell
{
    class Program
    {
        static void Main(string[] args)
        {
            if (!Shell.IsLoaderAvailable())
            {
                Console.WriteLine("The UnixAPI API isn't available in your wine currently, you need install it.");
                Console.WriteLine("Do you Wish to install it? [Y/N]");

                if (Console.ReadKey().Key == ConsoleKey.Y) {
                    Console.WriteLine();
                    Console.WriteLine("If you Install the UnixAPI you will expose to all programs running under your wine the native linux system functions, allowing any program interact with your operating system with the same previleges of the wine");
                    Console.WriteLine();
                    Console.WriteLine("An \"unixapi.dll.so\" has been created in the UnixShell directory.");
                    Console.WriteLine("You will need move that files to your wine directory.");
                    Console.WriteLine();
                    Console.WriteLine("Example on WINE:");
                    Console.WriteLine("unixapi.dll.so  => /opt/wine-???/lib/wine");
                    Console.WriteLine();
                    Console.WriteLine("Example on Proton:");
                    Console.WriteLine("unixapi.dll.so  => ~/.steam/root/steamapps/common/Proton ???/dist/lib/wine");
                    Console.WriteLine();
                    Console.WriteLine("Example on Custom Proton:");
                    Console.WriteLine("unixapi.dll.so  => ~/.steam/root/compatibilitytools.d/Proton-????/dist/lib/wine");
                    SelfExtract();
                }
                return;
            }

            if (args.Length == 0)
            {
                Console.WriteLine("UnixShell - By Marcussacana");
                Console.WriteLine("Escapes Wine and runs a command line on the operating system");
                Console.WriteLine("PS: Any valid Windows file path is automatically converted to the real unix path");
                Console.WriteLine();
                Console.WriteLine("Usage:");
                Console.WriteLine("UnixShell notify-send --icon=wine \"C:\\Windows\\explorer.exe\"");
                Console.WriteLine("Results: notify-send --icon=wine \"~/.wine/drive_c/Windows/explorer.exe\"");
                Console.WriteLine();
                Console.WriteLine("UnixShell -raw \"echo My raw command line that don\\'t will be pre-processed\"");
                Console.WriteLine("Results: echo My raw command line that don\\'t will be pre-processed");
                return;
            }

            string Command = "";
            if (args.Length == 2 && args[0].ToLower().TrimStart('-', '/', '\\') == "raw")
            {
                Command = args[1];
            }
            else
            {
                for (int i = 0; i < args.Length; i++)
                {
                    var arg = args[i];
                    bool Quoted = arg.StartsWith("\"") && arg.EndsWith("\"");
                    bool IsLast = i + 1 >= args.Length;

                    if (Quoted)
                        arg = arg.Substring(1, arg.Length - 2);

                    if (File.Exists(arg) || Directory.Exists(arg))
                        arg = Shell.ConvertToUnixPath(arg);

                    if (Quoted || arg.Contains(" "))
                        arg = $"\"{arg}\"";

                    Command += arg;

                    if (!IsLast)
                        Command += ' ';
                }
            }

            Shell.system(Command);
        }

        static void SelfExtract() {
            var UnixShell = Assembly.GetExecutingAssembly();
            using (var Reader = UnixShell.GetManifestResourceStream("UnixShell.unixapi.dll.so"))
            using (var Writer = File.Create("unixapi.dll.so"))
            {
                Reader.CopyTo(Writer);
                Writer.Flush();
            }
        }
    }
}
