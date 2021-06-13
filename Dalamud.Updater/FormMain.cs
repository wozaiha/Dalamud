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

namespace Dalamud.Updater
{
    public partial class FormMain : Form
    {
        private string updateUrl = "https://dalamud-1253720819.cos.ap-nanjing.myqcloud.com/update.xml";
        private Version getVersion()
        {
            var libPath = Path.GetFullPath("Dalamud.dll");
            var version = "0.0.0.0";
            if (File.Exists(libPath)) {
                var versionInfo = FileVersionInfo.GetVersionInfo(libPath);
                version = versionInfo.FileVersion;
            }
            return new Version(version);
        }

        public FormMain()
        {
            InitializeComponent();
            labelVersion.Text = string.Format("当前版本 : {0}", getVersion());
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            //Uncomment below lines to handle parsing logic of non XML AppCast file.

            //AutoUpdater.ParseUpdateInfoEvent += AutoUpdaterOnParseUpdateInfoEvent;
            //AutoUpdater.Start("https://rbsoft.org/updates/AutoUpdaterTest.json");

            //Uncomment below line to run update process using non administrator account.

            //AutoUpdater.RunUpdateAsAdmin = false;

            //Uncomment below line to see russian version

            //Thread.CurrentThread.CurrentCulture = Thread.CurrentThread.CurrentUICulture = CultureInfo.CreateSpecificCulture("ru");

            //If you want to open download page when user click on download button uncomment below line.

            //AutoUpdater.OpenDownloadPage = true;

            //Don't want user to select remind later time in AutoUpdater notification window then uncomment 3 lines below so default remind later time will be set to 2 days.

            //AutoUpdater.LetUserSelectRemindLater = false;
            //AutoUpdater.RemindLaterTimeSpan = RemindLaterFormat.Minutes;
            //AutoUpdater.RemindLaterAt = 1;

            //Don't want to show Skip button then uncomment below line.

            //AutoUpdater.ShowSkipButton = false;

            //Don't want to show Remind Later button then uncomment below line.

            //AutoUpdater.ShowRemindLaterButton = false;

            //Want to show custom application title then uncomment below line.

            //AutoUpdater.AppTitle = "My Custom Application Title";

            //Want to show errors then uncomment below line.

            //AutoUpdater.ReportErrors = true;

            //Want to handle how your application will exit when application finished downloading then uncomment below line.

            AutoUpdater.ApplicationExitEvent += AutoUpdater_ApplicationExitEvent;

            //Want to handle update logic yourself then uncomment below line.

            AutoUpdater.CheckForUpdateEvent += AutoUpdaterOnCheckForUpdateEvent;

            //Want to use XML and Update file served only through Proxy.

            //var proxy = new WebProxy("localproxyIP:8080", true) {Credentials = new NetworkCredential("domain\\user", "password")};

            //AutoUpdater.Proxy = proxy;

            //Want to check for updates frequently then uncomment following lines.

            //System.Timers.Timer timer = new System.Timers.Timer
            //{
            //    Interval = 2 * 60 * 1000,
            //    SynchronizingObject = this
            //};
            //timer.Elapsed += delegate
            //{
            //    AutoUpdater.Start("https://rbsoft.org/updates/AutoUpdaterTest.xml");
            //};
            //timer.Start();

            //Uncomment following lines to provide basic authentication credentials to use.

            //BasicAuthentication basicAuthentication = new BasicAuthentication("myUserName", "myPassword");
            //AutoUpdater.BasicAuthXML = AutoUpdater.BasicAuthDownload = basicAuthentication;

            //Uncomment following lines to enable forced updates.

            //AutoUpdater.Mandatory = true;
            //AutoUpdater.UpdateMode = Mode.Forced;

            //Want to change update form size then uncomment below line.

            //AutoUpdater.UpdateFormSize = new System.Drawing.Size(800, 600);

            //Uncomment following if you want to update using FTP.
            //AutoUpdater.Start("ftp://rbsoft.org/updates/AutoUpdaterTest.xml", new NetworkCredential("FtpUserName", "FtpPassword"));

            //Uncomment following lines if you want to persist Remind Later and Skip values in a json file.
            //string jsonPath = Path.Combine(Environment.CurrentDirectory, "settings.json");
            //AutoUpdater.PersistenceProvider = new JsonFilePersistenceProvider(jsonPath);

            //Uncomment following line if you want to set the zip extraction path.
            //var currentDirectory = new DirectoryInfo(Environment.CurrentDirectory);
            //if (currentDirectory.Parent != null)
            //{
            //    AutoUpdater.InstallationPath = currentDirectory.Parent.FullName;
            //}

            //Uncomment following line if you want to check for update synchronously.
            //AutoUpdater.Synchronous = true;

            AutoUpdater.InstalledVersion = getVersion();
            AutoUpdater.Start("https://dalamud-1253720819.cos.ap-nanjing.myqcloud.com/update.xml");
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

        private void ButtonCheckForUpdate_Click(object sender, EventArgs e)
        {
            //Uncomment below lines to select download path where update is saved.

            //FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            //if (folderBrowserDialog.ShowDialog().Equals(DialogResult.OK))
            //{
            //    AutoUpdater.DownloadPath = folderBrowserDialog.SelectedPath;
            //    AutoUpdater.Mandatory = true;
            //    AutoUpdater.Start("https://rbsoft.org/updates/AutoUpdaterTest.xml");
            //}

            AutoUpdater.Mandatory = true;
            AutoUpdater.InstalledVersion = getVersion();
            AutoUpdater.Start(updateUrl);
        }
    }
}
