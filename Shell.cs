using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace UnixShell
{
    static class Shell
    {
        public static bool IsLoaderAvailable() {
            return LoadLibraryW("UnixAPI.dll") != IntPtr.Zero;
        }
        public static string ConvertToUnixPath(string Path) {
            var PrefixDir = GetPrefixPath();

            Path = System.IO.Path.GetFullPath(Path);

            var Drive = Path.ToLowerInvariant().First();

            Path = Path.Substring(2);//C:
            Path = Path.Replace("\\", "/");

            if (Drive == 'z')
                return Path;

            Path = PrefixDir + "drive_"  + Drive + Path;

            return Path;
        }
        public static string GetPrefixPath()
        {
            var Prefix = getenv("WINEPREFIX");

            if (Prefix == null || string.IsNullOrWhiteSpace(Prefix))
                Prefix = "~/.wine";

            if (!Prefix.EndsWith("/"))
                Prefix += '/';

            return Prefix;
        }
        public static string getenv(string Name)
        {
            if (hGetEnv == IntPtr.Zero)
            {
                var hModule = dlopen("libc.so.6", RTLD_NOW);
                hGetEnv = dlsym(hModule, "getenv");

                dGetEnv = (getEnvDel)Marshal.GetDelegateForFunctionPointer(hGetEnv, typeof(getEnvDel));
            }

            return dGetEnv(Name);
        }

        public static void system(string Command)
        {
            if (hSystem == IntPtr.Zero)
            {
                var hModule = dlopen("libc.so.6", RTLD_NOW);
                hSystem = dlsym(hModule, "system");

                dSystem = (systemDel)Marshal.GetDelegateForFunctionPointer(hSystem, typeof(systemDel));
            }

            dSystem(Command);
        }


        static IntPtr hSystem;
        static systemDel dSystem;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        delegate void systemDel(string Command);


        static IntPtr hGetEnv;
        static getEnvDel dGetEnv;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        delegate string getEnvDel(string Command);


        const int RTLD_NOW = 0x002;


        [DllImport("UnixAPI", EntryPoint = "wine_dlopen", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        static extern IntPtr dlopen(string Filename, int Mode);

        [DllImport("UnixAPI", EntryPoint = "wine_dlsym", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        static extern IntPtr dlsym(IntPtr Handle, string Symbol);


        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        static extern IntPtr LoadLibraryW(string Library);
    }
}
