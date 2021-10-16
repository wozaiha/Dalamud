using AutoUpdaterDotNET;
using Dalamud.Updater.Properties;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.IO.Compression;
using System.Configuration;
using Newtonsoft.Json.Linq;

namespace Dalamud.Updater
{
    public partial class FormMain : Form
    {
        private string updateUrl = "https://dalamud-1253720819.cos.ap-nanjing.myqcloud.com/update.xml";

        // private List<string> pidList = new List<string>();
        private bool isThreadRunning = true;
        private Version getVersion()
        {
            var rgx = new Regex(@"\d+\.\d+\.\d+\.\d+");
            var di = new DirectoryInfo(@".");
            var dirs = di.GetDirectories("*", SearchOption.TopDirectoryOnly).Where(dir => rgx.IsMatch(dir.Name)).ToArray();

            var version = new Version("0.0.0.0");
            foreach (var dir in dirs)
            {
                var newVersion = new Version(dir.Name);
                if (newVersion > version)
                {
                    version = newVersion;
                }
            }
            return version;
        }

        public FormMain()
        {
            InitializeComponent();
            InitializePIDCheck();
            labelVersion.Text = string.Format("当前版本 : {0}", getVersion());
        }

        private void InitializePIDCheck()
        {
            var thread = new Thread(() => {
                while (this.isThreadRunning)
                {
                    var newPidList = Process.GetProcessesByName("ffxiv_dx11").ToList()
                                    .ConvertAll(process => process.Id.ToString()).ToArray();
                    var newHash = String.Join(", ", newPidList).GetHashCode();
                    var oldPidList = this.comboBoxFFXIV.Items.Cast<Object>().Select(item => item.ToString()).ToArray();
                    var oldHash = String.Join(", ", oldPidList).GetHashCode();
                    if (oldHash != newHash)
                    {
                        this.comboBoxFFXIV.Invoke((MethodInvoker)delegate {
                            // Running on the UI thread
                            comboBoxFFXIV.Items.Clear();
                            comboBoxFFXIV.Items.AddRange(newPidList);
                            if (newPidList.Length > 0)
                            {
                                if (!comboBoxFFXIV.DroppedDown)
                                    this.comboBoxFFXIV.SelectedIndex = 0;
                                if (this.checkBoxAutoInject.Checked)
                                {
                                    foreach (var pidStr in newPidList)
                                    {
                                        var pid = int.Parse(pidStr);
                                        this.Inject(pid);
                                    }
                                }
                            }
                        });
                    }
                    Thread.Sleep(1000);
                }
            });
            thread.Start();
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            AutoUpdater.ApplicationExitEvent += AutoUpdater_ApplicationExitEvent;
            AutoUpdater.CheckForUpdateEvent += AutoUpdaterOnCheckForUpdateEvent;
            AutoUpdater.InstalledVersion = getVersion();
        }
        private void FormMain_Disposed(object sender, EventArgs e)
        {
            this.isThreadRunning = false;
        }

        private void AutoUpdater_ApplicationExitEvent()
        {
            Text = @"Closing application...";
            Thread.Sleep(5000);
            Application.Exit();
        }


        private void AutoUpdaterOnParseUpdateInfoEvent(ParseUpdateInfoEventArgs args)
        {
            dynamic json = JsonConvert.DeserializeObject(args.RemoteData);
            args.UpdateInfo = new UpdateInfoEventArgs
            {
                CurrentVersion = json.version,
                ChangelogURL = json.changelog,
                DownloadURL = json.url,
                Mandatory = new Mandatory
                {
                    Value = json.mandatory.value,
                    UpdateMode = json.mandatory.mode,
                    MinimumVersion = json.mandatory.minVersion
                },
                CheckSum = new CheckSum
                {
                    Value = json.checksum.value,
                    HashingAlgorithm = json.checksum.hashingAlgorithm
                }
            };
        }

        private void AutoUpdaterOnCheckForUpdateEvent(UpdateInfoEventArgs args)
        {
            if (args.Error == null)
            {
                if (args.IsUpdateAvailable)
                {
                    DialogResult dialogResult;
                    if (args.Mandatory.Value)
                    {
                        dialogResult =
                            MessageBox.Show(
                                $@"卫月框架 {args.CurrentVersion} 版本可用。当前版本为 {
                                        args.InstalledVersion
                                    }。这是一个强制更新，请点击 OK 来更新卫月框架。",
                                @"更新可用",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information);
                    }
                    else
                    {
                        dialogResult =
                            MessageBox.Show(
                                $@"卫月框架 {args.CurrentVersion} 版本可用。当前版本为 {
                                        args.InstalledVersion
                                    }。您想要开始更新吗？", @"更新可用",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Information);
                    }


                    if (dialogResult.Equals(DialogResult.Yes) || dialogResult.Equals(DialogResult.OK))
                    {
                        try
                        {
                            //You can use Download Update dialog used by AutoUpdater.NET to download the update.

                            if (AutoUpdater.DownloadUpdate(args))
                            {
                                Application.Exit();
                            }
                        }
                        catch (Exception exception)
                        {
                            MessageBox.Show(exception.Message, exception.GetType().ToString(), MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                        }
                    }
                }
                else
                {
                    MessageBox.Show(@"没有可用更新，请稍后查看。", @"更新不可用",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else
            {
                if (args.Error is WebException)
                {
                    MessageBox.Show(
                        @"访问更新服务器出错，请检查您的互联网连接后重试。",
                        @"更新检查失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    MessageBox.Show(args.Error.Message,
                        args.Error.GetType().ToString(), MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
        }

        private void TryDownloadRuntime(DirectoryInfo runtimePath, string RuntimeVersion)
        {
            new Thread(() =>
            {
                try
                {
                    DownloadRuntime(runtimePath, RuntimeVersion);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("运行库下载失败 :(", "下载运行库",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                    return;
                }
                MessageBox.Show("运行库已下载 :)", "下载运行库",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
            }).Start();
        }

        private void DownloadRuntime(DirectoryInfo runtimePath, string version)
        {
            // Ensure directory exists
            if (!runtimePath.Exists)
            {
                runtimePath.Create();
            }
            else
            {
                runtimePath.Delete(true);
                runtimePath.Create();
            }

            var client = new WebClient();
            var baseDotnetRuntimeUrl = this.checkBoxAcce.Checked ? "https://dotnetcli.azureedge.net" : "https://dotnetcli.blob.core.windows.net";
            var dotnetUrl = $"{baseDotnetRuntimeUrl}/dotnet/Runtime/{version}/dotnet-runtime-{version}-win-x64.zip";
            var desktopUrl = $"{baseDotnetRuntimeUrl}/dotnet/WindowsDesktop/{version}/windowsdesktop-runtime-{version}-win-x64.zip";

            var downloadPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

            if (File.Exists(downloadPath))
                File.Delete(downloadPath);

            client.DownloadFile(dotnetUrl, downloadPath);
            ZipFile.ExtractToDirectory(downloadPath, runtimePath.FullName);

            client.DownloadFile(desktopUrl, downloadPath);
            ZipFile.ExtractToDirectory(downloadPath, runtimePath.FullName);

            File.Delete(downloadPath);
        }

        private void ButtonCheckRuntime_Click(object sender, EventArgs e)
        {
            var RuntimeVersion = "5.0.6";
            var runtimePath = new DirectoryInfo(Path.Combine(Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName, "XIVLauncher", "runtime"));
            var runtimePaths = new DirectoryInfo[]
            {
                new DirectoryInfo(Path.Combine(runtimePath.FullName, "host", "fxr", RuntimeVersion)),
                new DirectoryInfo(Path.Combine(runtimePath.FullName, "shared", "Microsoft.NETCore.App", RuntimeVersion)),
                new DirectoryInfo(Path.Combine(runtimePath.FullName, "shared", "Microsoft.WindowsDesktop.App", RuntimeVersion)),
            };
            if (runtimePaths.Any(p => !p.Exists))
            {
                var choice = MessageBox.Show("运行卫月需要下载所需运行库，是否下载？", "下载运行库",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Information);
                if (choice == DialogResult.Yes)
                    TryDownloadRuntime(runtimePath, RuntimeVersion);
                else
                    return;
            } else
            {
                var choice = MessageBox.Show("运行库已存在，是否强制下载？", "下载运行库",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Information);
                if (choice == DialogResult.Yes)
                    TryDownloadRuntime(runtimePath, RuntimeVersion);
            }
        }

        private void ButtonCheckForUpdate_Click(object sender, EventArgs e)
        {
            if (this.comboBoxFFXIV.SelectedItem != null)
            {
                var choice = MessageBox.Show("经检测存在 ffxiv_dx11.exe 进程，更新卫月需要关闭游戏，需要帮您代劳吗？", "关闭游戏",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Information);
                if (choice == DialogResult.Yes)
                {
                    var pid = int.Parse((string)this.comboBoxFFXIV.SelectedItem);
                    Process.GetProcessById(pid).Kill();
                } else
                {
                    return;
                }
            }
            AutoUpdater.Mandatory = true;
            AutoUpdater.InstalledVersion = getVersion();
            var OverwriteUpdate = ConfigurationManager.AppSettings["OverwriteUpdate"];
            if (OverwriteUpdate != null)
                updateUrl = OverwriteUpdate;
            if (this.checkBoxAcce.Checked)
                updateUrl = updateUrl.Replace("/update", "/acce_update").Replace("ap-nanjing", "accelerate");
            AutoUpdater.Start(updateUrl);
        }

        private void comboBoxFFXIV_Clicked(object sender, EventArgs e)
        {
            /*
            foreach(var pid in this.pidList)
                if (!this.comboBoxFFXIV.Items.Contains(pid))
                    this.comboBoxFFXIV.Items.Add(pid);
            var toDel = new List<string>();
            foreach (string pid in this.comboBoxFFXIV.Items)
                if (!this.pidList.Contains(pid))
                    toDel.Add(pid);
            foreach (var pid in toDel)
                if (!this.pidList.Contains(pid))
                    this.comboBoxFFXIV.Items.Remove(pid);
            */

        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://jq.qq.com/?_wv=1027&k=agTNLSBJ");
        }
        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://afdian.net/@bluefissure");
        }

        private string GeneratingDalamudStartInfo(Process process)
        {
            var ffxivDir = Path.GetDirectoryName(process.MainModule.FileName);
            var appDataDir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
            var xivlauncherDir = Path.Combine(appDataDir, "XIVLauncher");

            var gameVerStr = File.ReadAllText(Path.Combine(ffxivDir, "ffxivgame.ver"));

            var startInfo = JObject.FromObject(new
            {
                ConfigurationPath = Path.Combine(xivlauncherDir, "dalamudConfig.json"),
                PluginDirectory = Path.Combine(xivlauncherDir, "installedPlugins"),
                DefaultPluginDirectory = Path.Combine(xivlauncherDir, "devPlugins"),
                AssetDirectory = Path.Combine(xivlauncherDir, "dalamudAssets"),
                GameVersion = gameVerStr,
                Language = "ChineseSimplified",
                OptOutMbCollection = false,
                GlobalAccelerate = this.checkBoxAcce.Checked,
            });

            return startInfo.ToString();
        }

        private bool Inject(int pid)
        {
            var process = Process.GetProcessById(pid);
            bool injected = false;
            for (var j = 0; j < process.Modules.Count; j++)
            {
                if (process.Modules[j].ModuleName == "Dalamud.dll")
                {
                    injected = true;
                    break;
                }
            }
            if (injected)
            {
                return false;
            }
            var version = getVersion();
            var dalamudPath = new DirectoryInfo(Path.Combine(Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName, $"{version}"));
            var injectorFile = Path.Combine(dalamudPath.FullName, "Dalamud.Injector.exe");
            var dalamudStartInfo = Convert.ToBase64String(Encoding.UTF8.GetBytes(GeneratingDalamudStartInfo(process)));
            var startInfo = new ProcessStartInfo(injectorFile, $"{pid} {dalamudStartInfo}");
            startInfo.WorkingDirectory = dalamudPath.FullName;
            Process.Start(startInfo);
            return true;
        }

        private void ButtonInject_Click(object sender, EventArgs e)
        {
            if(this.comboBoxFFXIV.SelectedItem != null)
            {
                var pid = this.comboBoxFFXIV.SelectedItem.ToString();
                Inject(int.Parse(pid));
            }
            else
            {
                MessageBox.Show("未选择游戏进程", "找不到游戏",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {

        }
    }
}
