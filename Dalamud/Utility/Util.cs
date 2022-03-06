using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Dalamud.Configuration;
using Dalamud.Configuration.Internal;
using Dalamud.Game;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using ImGuiNET;
using Microsoft.Win32;
using Serilog;

namespace Dalamud.Utility
{
    /// <summary>
    /// Class providing various helper methods for use in Dalamud and plugins.
    /// </summary>
    public static class Util
    {
        private static string gitHashInternal;
        private static List<FuckGFWSettings> fuckGFWList;

        /// <summary>
        /// Gets an httpclient for usage.
        /// Do NOT await this.
        /// </summary>
        public static HttpClient HttpClient { get; } = new();

        /// <summary>
        /// Gets the assembly version of Dalamud.
        /// </summary>
        public static string AssemblyVersion { get; } = Assembly.GetAssembly(typeof(ChatHandlers)).GetName().Version.ToString();

        /// <summary>
        /// Gets the git hash value from the assembly
        /// or null if it cannot be found.
        /// </summary>
        /// <returns>The git hash of the assembly.</returns>
        public static string GetGitHash()
        {
            if (gitHashInternal != null)
                return gitHashInternal;

            var asm = typeof(Util).Assembly;
            var attrs = asm.GetCustomAttributes<AssemblyMetadataAttribute>();

            gitHashInternal = attrs.FirstOrDefault(a => a.Key == "GitHash")?.Value;

            return gitHashInternal;
        }

        /// <summary>
        /// Read memory from an offset and hexdump them via Serilog.
        /// </summary>
        /// <param name="offset">The offset to read from.</param>
        /// <param name="len">The length to read.</param>
        public static void DumpMemory(IntPtr offset, int len = 512)
        {
            try
            {
                SafeMemory.ReadBytes(offset, len, out var data);
                Log.Information(ByteArrayToHex(data));
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Read failed");
            }
        }

        /// <summary>
        /// Create a hexdump of the provided bytes.
        /// </summary>
        /// <param name="bytes">The bytes to hexdump.</param>
        /// <param name="offset">The offset in the byte array to start at.</param>
        /// <param name="bytesPerLine">The amount of bytes to display per line.</param>
        /// <returns>The generated hexdump in string form.</returns>
        public static string ByteArrayToHex(byte[] bytes, int offset = 0, int bytesPerLine = 16)
        {
            if (bytes == null) return string.Empty;

            var hexChars = "0123456789ABCDEF".ToCharArray();

            var offsetBlock = 8 + 3;
            var byteBlock = offsetBlock + (bytesPerLine * 3) + ((bytesPerLine - 1) / 8) + 2;
            var lineLength = byteBlock + bytesPerLine + Environment.NewLine.Length;

            var line = (new string(' ', lineLength - Environment.NewLine.Length) + Environment.NewLine).ToCharArray();
            var numLines = (bytes.Length + bytesPerLine - 1) / bytesPerLine;

            var sb = new StringBuilder(numLines * lineLength);

            for (var i = 0; i < bytes.Length; i += bytesPerLine)
            {
                var h = i + offset;

                line[0] = hexChars[(h >> 28) & 0xF];
                line[1] = hexChars[(h >> 24) & 0xF];
                line[2] = hexChars[(h >> 20) & 0xF];
                line[3] = hexChars[(h >> 16) & 0xF];
                line[4] = hexChars[(h >> 12) & 0xF];
                line[5] = hexChars[(h >> 8) & 0xF];
                line[6] = hexChars[(h >> 4) & 0xF];
                line[7] = hexChars[(h >> 0) & 0xF];

                var hexColumn = offsetBlock;
                var charColumn = byteBlock;

                for (var j = 0; j < bytesPerLine; j++)
                {
                    if (j > 0 && (j & 7) == 0) hexColumn++;

                    if (i + j >= bytes.Length)
                    {
                        line[hexColumn] = ' ';
                        line[hexColumn + 1] = ' ';
                        line[charColumn] = ' ';
                    }
                    else
                    {
                        var by = bytes[i + j];
                        line[hexColumn] = hexChars[(by >> 4) & 0xF];
                        line[hexColumn + 1] = hexChars[by & 0xF];
                        line[charColumn] = by < 32 ? '.' : (char)by;
                    }

                    hexColumn += 3;
                    charColumn++;
                }

                sb.Append(line);
            }

            return sb.ToString().TrimEnd(Environment.NewLine.ToCharArray());
        }

        /// <summary>
        /// Show all properties and fields of the provided object via ImGui.
        /// </summary>
        /// <param name="obj">The object to show.</param>
        public static void ShowObject(object obj)
        {
            var type = obj.GetType();

            ImGui.Text($"Object Dump({type.Name}) for {obj}({obj.GetHashCode()})");

            ImGuiHelpers.ScaledDummy(5);

            ImGui.TextColored(ImGuiColors.DalamudOrange, "-> Properties:");

            ImGui.Indent();

            foreach (var propertyInfo in type.GetProperties())
            {
                ImGui.TextColored(ImGuiColors.DalamudOrange, $"    {propertyInfo.Name}: {propertyInfo.GetValue(obj)}");
            }

            ImGui.Unindent();

            ImGuiHelpers.ScaledDummy(5);

            ImGui.TextColored(ImGuiColors.HealerGreen, "-> Fields:");

            ImGui.Indent();

            foreach (var fieldInfo in type.GetFields())
            {
                ImGui.TextColored(ImGuiColors.HealerGreen, $"    {fieldInfo.Name}: {fieldInfo.GetValue(obj)}");
            }

            ImGui.Unindent();
        }

        /// <summary>
        /// Display an error MessageBox and exit the current process.
        /// </summary>
        /// <param name="message">MessageBox body.</param>
        /// <param name="caption">MessageBox caption (title).</param>
        public static void Fatal(string message, string caption)
        {
            var flags = NativeFunctions.MessageBoxType.Ok | NativeFunctions.MessageBoxType.IconError;
            _ = NativeFunctions.MessageBoxW(Process.GetCurrentProcess().MainWindowHandle, message, caption, flags);

            Environment.Exit(-1);
        }

        /// <summary>
        /// Retrieve a UTF8 string from a null terminated byte array.
        /// </summary>
        /// <param name="array">A null terminated UTF8 byte array.</param>
        /// <returns>A UTF8 encoded string.</returns>
        public static string GetUTF8String(byte[] array)
        {
            var count = 0;
            for (; count < array.Length; count++)
            {
                if (array[count] == 0)
                    break;
            }

            string text;
            if (count == array.Length)
            {
                text = Encoding.UTF8.GetString(array);
                Log.Warning($"Warning: text exceeds underlying array length ({text})");
            }
            else
            {
                text = Encoding.UTF8.GetString(array, 0, count);
            }

            return text;
        }

        /// <summary>
        /// Set the list of fuck GFWs from configure.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static void SetFuckGFWFromConfig()
        {
            var configuration = Service<DalamudConfiguration>.Get();
            fuckGFWList = configuration.FuckGFWList;
        }

        /// <summary>
        /// This is a FUCK-GFW replacement of urls.
        /// </summary>
        /// <param name="url">A url to be fucked.</param>
        /// <returns>A fucked url.</returns>
        public static string FuckGFW(string url)
        {
            if (url == null) return null;
            if (fuckGFWList == null)
            {
                SetFuckGFWFromConfig();
            }

            var originalUrl = url;
            if (fuckGFWList != null)
            {
                foreach (var fuckGFW in fuckGFWList)
                {
                    if (fuckGFW.IsEnabled)
                    {
                        var oldUrl = url;
                        url = Regex.Replace(url, fuckGFW.UrlRegex, fuckGFW.ReplaceTo);
                        if (url != oldUrl)
                        {
                            Log.Debug($"GFW fucked by {fuckGFW.UrlRegex} -> {fuckGFW.ReplaceTo}:");
                            Log.Debug($"\t{url}");
                        }
                    }
                }
            }

            var startInfo = Service<DalamudStartInfo>.Get();
            if (startInfo.GlobalAccelerate)
            {
                var oldUrl = url;
                var urlRegex = @"cos\.ap-nanjing\.myqcloud\.com";
                var replaceTo = "cos.accelerate.myqcloud.com";
                url = Regex.Replace(url, urlRegex, replaceTo);
                if (url != oldUrl)
                {
                    Log.Debug($"GFW fucked by {urlRegex} -> {replaceTo}:");
                    Log.Debug($"\t{url}");
                }
            }

            if (originalUrl != url)
            {
                Log.Information($"Fucked GFW from {originalUrl} \n\tto {url}");
            }

            /*
            if (!startInfo.GlobalAccelerate)
            {
                url = Regex.Replace(url, @"^https:\/\/raw\.githubusercontent\.com", "https://raw.fastgit.org");
                url = Regex.Replace(url, @"^https:\/\/(?:gitee|github)\.com\/(.*)?\/(.*)?\/raw", "https://raw.fastgit.org/$1/$2");
                url = Regex.Replace(url, @"^https:\/\/github\.com\/(.*)?\/(.*)?\/releases\/download", "https://download.fastgit.org/$1/$2/releases/download/");
            }
            else
            {
                url = Regex.Replace(url, @"cos\.ap-nanjing\.myqcloud\.com", "cos.accelerate.myqcloud.com");
            }
            */

            return url;
        }

        ///     Compress a string using GZip.
        /// </summary>
        /// <param name="str">The input string.</param>
        /// <returns>The compressed output bytes.</returns>
        public static byte[] CompressString(string str)
        {
            var bytes = Encoding.UTF8.GetBytes(str);

            using var msi = new MemoryStream(bytes);
            using var mso = new MemoryStream();
            using var gs = new GZipStream(mso, CompressionMode.Compress);

            CopyTo(msi, gs);

            return mso.ToArray();
        }

        /// <summary>
        /// Decompress a string using GZip.
        /// </summary>
        /// <param name="bytes">The input bytes.</param>
        /// <returns>The compressed output string.</returns>
        public static string DecompressString(byte[] bytes)
        {
            using var msi = new MemoryStream(bytes);
            using var mso = new MemoryStream();
            using var gs = new GZipStream(msi, CompressionMode.Decompress);

            CopyTo(gs, mso);

            return Encoding.UTF8.GetString(mso.ToArray());
        }

        /// <summary>
        /// Copy one stream to another.
        /// </summary>
        /// <param name="src">The source stream.</param>
        /// <param name="dest">The destination stream.</param>
        /// <param name="len">The maximum length to copy.</param>
        public static void CopyTo(Stream src, Stream dest, int len = 4069)
        {
            var bytes = new byte[len];
            int cnt;

            while ((cnt = src.Read(bytes, 0, bytes.Length)) != 0) dest.Write(bytes, 0, cnt);
        }

        /// <summary>
        /// Heuristically determine if Dalamud is running on Linux/WINE.
        /// </summary>
        /// <returns>Whether or not Dalamud is running on Linux/WINE.</returns>
        public static bool IsLinux()
        {
            bool Check1()
            {
                return EnvironmentConfiguration.XlWineOnLinux;
            }

            bool Check2()
            {
                var hModule = NativeFunctions.GetModuleHandleW("ntdll.dll");
                var proc1 = NativeFunctions.GetProcAddress(hModule, "wine_get_version");
                var proc2 = NativeFunctions.GetProcAddress(hModule, "wine_get_build_id");

                return proc1 != IntPtr.Zero || proc2 != IntPtr.Zero;
            }

            bool Check3()
            {
                return Registry.CurrentUser.OpenSubKey(@"Software\Wine") != null ||
                       Registry.LocalMachine.OpenSubKey(@"Software\Wine") != null;
            }

            return Check1() || Check2() || Check3();
        }

        /// <summary>
        /// Set the proxy.
        /// </summary>
        /// <param name="useSystemProxy">Use system proxy</param>
        /// <param name="proxyHost">The proxy host.</param>
        /// <param name="proxyPort">The proxy port.</param>
        public static void SetProxy(bool useSystemProxy, string proxyHost = "", int proxyPort = 0) {
            var proxy = useSystemProxy ? WebRequest.GetSystemWebProxy() : new WebProxy(proxyHost, proxyPort);
            if (useSystemProxy)
                Log.Information($"Current proxy is default proxy of system.");
            else {
                Log.Information($"Current proxy is {proxyHost}:{proxyPort}.");
            }
            WebRequest.DefaultWebProxy = proxy;
            HttpClient.DefaultProxy = proxy;
            }
    }
}
