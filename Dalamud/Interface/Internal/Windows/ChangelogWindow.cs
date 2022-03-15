using System;
using System.Diagnostics;
using System.IO;
using System.Numerics;

using Dalamud.Interface.Colors;
using Dalamud.Interface.Windowing;
using Dalamud.Utility;
using ImGuiNET;
using ImGuiScene;

namespace Dalamud.Interface.Internal.Windows
{
    /// <summary>
    /// For major updates, an in-game Changelog window.
    /// </summary>
    internal sealed class ChangelogWindow : Window, IDisposable
    {
        /// <summary>
        /// Whether the latest update warrants a changelog window.
        /// </summary>
        public const string WarrantsChangelogForMajorMinor = "6.3.";

        private const string ChangeLog =
            @"• 在标题屏幕中添加了一个新菜单，允许您在登录前访问插件安装程序和各种其他插件。
    => 您可以在“外观”下的设置中禁用此菜单。
• 为插件添加了一种将信息添加到游戏服务器信息栏的方法（例如当前歌曲、ping 等）。
    => 如果有任何插件提供此功能，您可以在设置中禁用并重新排序这些信息。
• 将插件下载服务器切换到自托管解决方案而不是 GitHub，以规避 API 限制、国家/地区限制和不良 ISP 路由，同时添加了代理设置。
    => 请参阅 卫月框架 常见问题解答 (ottercorp.github.io/faq) 的“插件是否可以安全使用”部分，或者如果您对安全性有疑虑或想了解有关如何设置和运行的详细信息，请联系 Discord 或 QQ。
    => 游戏中的变更日志/插件安装程序现在也应该更常见，因为新服务从开发人员拉取请求描述中获取变更日志。
• 插件安装程序中的“可用插件”列表现在还显示已安装的插件，以减少拆分的混乱。 添加了过滤已安装插件的新过滤模式。
• 插件安装程序中添加了“更改日志”类别，该类别将列出您的插件的所有最新更改，以及 Dalamud 的最新更改。

如果您发现任何问题或需要帮助，请务必在我们的 Discord 服务器与 QQ群 内提问。
谢谢，玩得开心！";

        private const string UpdatePluginsInfo =
            @"• 由于此更新，您的所有插件都被自动禁用。 这个是正常的。
• 打开插件安装程序，然后单击“更新插件”。 更新的插件应该更新然后重新启用自己。
   => 请记住，并非所有插件都已针对新版本进行了更新。
   => 如果某些插件在“已安装插件”选项卡中显示为红色叉号，则它们可能尚不可用。

虽然我们用一小部分人对已发布的插件进行了相当大的测试，并相信它们是稳定的，但我们不能向您保证您不会遇到崩溃。

考虑到当前的排队时间，我们现在建议您只使用一组对您来说最重要的插件，这样您就可以继续玩游戏而不是无休止地排队。";

        private readonly string assemblyVersion = Util.AssemblyVersion;

        private readonly TextureWrap logoTexture;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChangelogWindow"/> class.
        /// </summary>
        public ChangelogWindow()
            : base("有啥新功能？？", ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoResize)
        {
            this.Namespace = "DalamudChangelogWindow";

            this.Size = new Vector2(885, 463);
            this.SizeCondition = ImGuiCond.Appearing;

            var interfaceManager = Service<InterfaceManager>.Get();
            var dalamud = Service<Dalamud>.Get();

            this.logoTexture =
                interfaceManager.LoadImage(Path.Combine(dalamud.AssetDirectory.FullName, "UIRes", "logo.png"))!;
        }

        /// <inheritdoc/>
        public override void Draw()
        {
            ImGui.Text($"卫月框架更新到了版本 D{this.assemblyVersion}。");

            ImGuiHelpers.ScaledDummy(10);

            ImGui.Text("包含了以下更新:");

            ImGui.SameLine();
            ImGuiHelpers.ScaledDummy(0);
            var imgCursor = ImGui.GetCursorPos();

            ImGui.TextWrapped(ChangeLog);

            /*
            ImGuiHelpers.ScaledDummy(5);

            ImGui.TextColored(ImGuiColors.DalamudRed, " !!! 注意 !!!");

            ImGui.TextWrapped(UpdatePluginsInfo);
            */

            ImGuiHelpers.ScaledDummy(10);

            ImGui.Text("感谢使用我们的工具！");

            ImGuiHelpers.ScaledDummy(10);

            ImGui.PushFont(UiBuilder.IconFont);

            if (ImGui.Button(FontAwesomeIcon.Download.ToIconString()))
            {
                Service<DalamudInterface>.Get().OpenPluginInstaller();
            }

            if (ImGui.IsItemHovered())
            {
                ImGui.PopFont();
                ImGui.SetTooltip("打开插件安装器");
                ImGui.PushFont(UiBuilder.IconFont);
            }

            ImGui.SameLine();

            if (ImGui.Button(FontAwesomeIcon.LaughBeam.ToIconString()))
            {
                try
                {
                    Process.Start(new ProcessStartInfo()
                    {
                        FileName = "https://discord.gg/3NMcUV5",
                        UseShellExecute = true,
                    });
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Could not open discord url");
                }
            }

            if (ImGui.IsItemHovered())
            {
                ImGui.PopFont();
                ImGui.SetTooltip("加入我们的 Discord 服务器");
                ImGui.PushFont(UiBuilder.IconFont);
            }

            ImGui.SameLine();

            if (ImGui.Button(FontAwesomeIcon.LaughSquint.ToIconString()))
            {
                try
                {
                    Process.Start(new ProcessStartInfo()
                    {
                        FileName = "https://jq.qq.com/?_wv=1027&k=3un8iHCo",
                        UseShellExecute = true,
                    });
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Could not open QQ url");
                }
            }

            if (ImGui.IsItemHovered())
            {
                ImGui.PopFont();
                ImGui.SetTooltip("加入我们的 QQ 群 827725124");
                ImGui.PushFont(UiBuilder.IconFont);
            }

            ImGui.SameLine();

            if (ImGui.Button(FontAwesomeIcon.Globe.ToIconString()))
            {
                Util.OpenLink("https://goatcorp.github.io/faq/");
            }

            if (ImGui.IsItemHovered())
            {
                ImGui.PopFont();
                ImGui.SetTooltip("查看 FAQ");
                ImGui.PushFont(UiBuilder.IconFont);
            }

            ImGui.SameLine();

            if (ImGui.Button(FontAwesomeIcon.Heart.ToIconString()))
            {
                Util.OpenLink("https://ottercorp.github.io/faq/support");
            }

            if (ImGui.IsItemHovered())
            {
                ImGui.PopFont();
                ImGui.SetTooltip("支持我们");
                ImGui.PushFont(UiBuilder.IconFont);
            }

            ImGui.PopFont();

            ImGui.SameLine();
            ImGuiHelpers.ScaledDummy(20, 0);
            ImGui.SameLine();

            if (ImGui.Button("关闭"))
            {
                this.IsOpen = false;
            }

            imgCursor.X += 750;
            imgCursor.Y -= 30;
            ImGui.SetCursorPos(imgCursor);

            ImGui.Image(this.logoTexture.ImGuiHandle, new Vector2(100));
        }

        /// <summary>
        /// Dispose this window.
        /// </summary>
        public void Dispose()
        {
            this.logoTexture.Dispose();
        }
    }
}
