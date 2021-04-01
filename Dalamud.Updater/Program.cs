using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Trinet.Core.IO.Ntfs;

namespace Dalamud.Updater
{
    class Program
    {
        static void Main(string[] args)
        {
            CheckDalamudVersion();
            Console.WriteLine($"Update finish.");
            Console.ReadLine();
        }


        private static string Get(string uri)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        private static void CheckDalamudVersion()
        {
            Console.WriteLine($"Checking version......");

            // File check
            var libPath = Path.GetFullPath("Dalamud.dll");
            string version = "unknown";
            if (!File.Exists(libPath))
            {
                Console.WriteLine($"Can't find Dalamud.dll on {libPath}");
            }
            else
            {
                var versionInfo = FileVersionInfo.GetVersionInfo(libPath);
                version = versionInfo.FileVersion;
            }

            Console.WriteLine($"Dalamud version: {version}");
            string baseUrl = "https://dalamud-1253720819.cos.ap-nanjing.myqcloud.com/";
            string remoteVersion = Get(baseUrl + "dalamud.ver");
            Console.WriteLine($"Dalamud remote version: {remoteVersion}");
            if (remoteVersion != version)
            {
                string fileName = $"dalamud_{remoteVersion}.zip";
                string zipPath = Path.Combine(Path.GetTempPath(), fileName);
                using (var client = new WebClient())
                {
                    Console.WriteLine($"Downloading dalamud v{remoteVersion} to {zipPath}");
                    client.DownloadFile(baseUrl + fileName, zipPath);
                }
                FileInfo file = new FileInfo(zipPath);

                // Read the "Zone.Identifier" stream, if it exists:
                if (file.AlternateDataStreamExists("Zone.Identifier"))
                {
                    // Console.WriteLine("Found zone identifier stream:");
                    Console.WriteLine("Unblocking file......");

                    AlternateDataStreamInfo s =
                       file.GetAlternateDataStream("Zone.Identifier",
                                                   FileMode.Open);
                    using (TextReader reader = s.OpenText())
                    {
                        Console.WriteLine(reader.ReadToEnd());
                    }

                    // Delete the stream:
                    s.Delete();
                }
                else
                {
                    // Console.WriteLine("No zone identifier stream found.");
                }

                Console.WriteLine($"Extracting {fileName} to {Directory.GetCurrentDirectory()}");
                ZipFile.ExtractToDirectory(zipPath, Directory.GetCurrentDirectory());
            }

        }
    }
}
