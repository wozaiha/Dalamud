using System;
using System.Diagnostics;
using System.Numerics;

using Dalamud.Interface.Colors;
using Dalamud.Interface.Windowing;
using Dalamud.Utility;
using ImGuiNET;
using Serilog;

namespace Dalamud.Interface.Internal.Windows
{
    /// <summary>
    /// For major updates, an in-game Changelog window.
    /// </summary>
    internal sealed class ChangelogWindow : Window
    {
        /// <summary>
        /// Whether the latest update warrants a changelog window.
        /// </summary>
        public const string WarrantsChangelogForMajorMinor = "6.0.";

        private const string ChangeLog =
            @"这是迄今为止卫月插件框架的最大更新。
我们重做了大部分底层系统，为您提供更好的插件运行和浏览体验，包括更好的性能，以及更好的API和更舒适的开发环境。

我们还添加了一些新功能：
• 重新设计的插件安装程序，具有图标、屏幕截图和可过滤的类别
• 卫月窗口的新外观和样式编辑器，可让您根据自己的喜好调整颜色和其他变量
• 在游戏中按下 ESC 将关闭激活的卫月窗口并保持游戏窗口打开，直到所有窗口关闭，从而统一游戏窗口的行为（您可以在设置中禁用此功能）
• 提供了卫月窗口的输入法

如果您发现任何问题或需要帮助，请务必在我们的 Discord 服务器或 QQ 群里询问。";

        private const string UpdatePluginsInfo =
            @"• 由于此更新，您的所有插件都已自动禁用。 这是正常的。
• 打开插件安装程序，然后单击“更新插件”。 更新的插件应该更新然后重新启用自己。
    => 请记住，并非您的所有插件都已针对新版本进行了更新。
    => 如果某些插件在“已安装插件”选项卡中显示为红色叉号，则它们可能尚不可用。";

        private readonly string assemblyVersion = Util.AssemblyVersion;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChangelogWindow"/> class.
        /// </summary>
        public ChangelogWindow()
            : base("有啥新功能？？")
        {
            this.Namespace = "DalamudChangelogWindow";

            this.Size = new Vector2(885, 463);
            this.SizeCondition = ImGuiCond.Appearing;
        }

        /// <inheritdoc/>
        public override void Draw()
        {
            ImGui.Text($"卫月框架更新到了版本 D{this.assemblyVersion}。");

            ImGuiHelpers.ScaledDummy(10);

            ImGui.Text("包含了以下更新:");
            ImGui.TextWrapped(ChangeLog);

            ImGuiHelpers.ScaledDummy(5);

            ImGui.TextColored(ImGuiColors.DalamudRed, " !!! 注意 !!!");

            ImGui.TextWrapped(UpdatePluginsInfo);

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
                try
                {
                    Process.Start(new ProcessStartInfo()
                    {
                        FileName = "https://github.com/goatcorp/FFXIVQuickLauncher",
                        UseShellExecute = true,
                    });
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Could not open github repo url");
                }
            }

            if (ImGui.IsItemHovered())
            {
                ImGui.PopFont();
                ImGui.SetTooltip("See our GitHub repository");
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
        }
    }
}
