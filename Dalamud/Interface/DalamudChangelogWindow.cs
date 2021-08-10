using System.Diagnostics;
using System.Numerics;

using Dalamud.Interface.Windowing;
using ImGuiNET;

namespace Dalamud.Interface
{
    /// <summary>
    /// For major updates, an in-game Changelog window.
    /// </summary>
    internal class DalamudChangelogWindow : Window
    {
        /// <summary>
        /// Whether the latest update warrants a changelog window.
        /// </summary>
        public const bool WarrantsChangelog = false;

        private const string ChangeLog =
            @"* API更新以支持最新版插件，增加稳定性
* 更快的启动速度

如果您遇到了任何问题或者需要帮助，请加入我们的 Discord 或 QQ群 反馈问题。
如果您在咸鱼小店等处付费获取卫月框架及其插件，请默念奸商司马。";

        private readonly Dalamud dalamud;
        private string assemblyVersion = Util.AssemblyVersion;

        /// <summary>
        /// Initializes a new instance of the <see cref="DalamudChangelogWindow"/> class.
        /// </summary>
        /// <param name="dalamud">The Dalamud instance.</param>
        public DalamudChangelogWindow(Dalamud dalamud)
            : base("更新了什么？", ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoResize)
        {
            this.dalamud = dalamud;

            this.Namespace = "DalamudChangelogWindow";

            this.IsOpen = WarrantsChangelog;
        }

        /// <inheritdoc/>
        public override void Draw()
        {
            ImGui.Text($"卫月框架更新到了版本 D{this.assemblyVersion}。");

            ImGui.Dummy(new Vector2(10, 10) * ImGui.GetIO().FontGlobalScale);

            ImGui.Text("包含以下更新内容：");
            ImGui.Text(ChangeLog);

            ImGui.Dummy(new Vector2(10, 10) * ImGui.GetIO().FontGlobalScale);

            ImGui.Text("感谢使用我们的工具！");

            ImGui.Dummy(new Vector2(10, 10) * ImGui.GetIO().FontGlobalScale);

            ImGui.PushFont(InterfaceManager.IconFont);

            if (ImGui.Button(FontAwesomeIcon.Download.ToIconString()))
                this.dalamud.DalamudUi.OpenPluginInstaller();

            if (ImGui.IsItemHovered())
            {
                ImGui.PopFont();
                ImGui.SetTooltip("打开插件管理器");
                ImGui.PushFont(InterfaceManager.IconFont);
            }

            ImGui.SameLine();

            if (ImGui.Button(FontAwesomeIcon.LaughBeam.ToIconString()))
                Process.Start("https://discord.gg/3NMcUV5");

            if (ImGui.IsItemHovered())
            {
                ImGui.PopFont();
                ImGui.SetTooltip("加入我们的 Discord 服务器");
                ImGui.PushFont(InterfaceManager.IconFont);
            }

            ImGui.SameLine();

            if (ImGui.Button(FontAwesomeIcon.LaughBeam.ToIconString()))
                Process.Start("https://jq.qq.com/?_wv=1027&k=FQO1as0Y");

            if (ImGui.IsItemHovered())
            {
                ImGui.PopFont();
                ImGui.SetTooltip("加入QQ群");
                ImGui.PushFont(InterfaceManager.IconFont);
            }

            ImGui.SameLine();

            if (ImGui.Button(FontAwesomeIcon.Globe.ToIconString()))
                Process.Start("https://github.com/Bluefissure/Dalamud");

            if (ImGui.IsItemHovered())
            {
                ImGui.PopFont();
                ImGui.SetTooltip("查看 GitHub 仓库");
                ImGui.PushFont(InterfaceManager.IconFont);
            }

            ImGui.PopFont();

            ImGui.SameLine();
            ImGui.Dummy(new Vector2(20, 0) * ImGui.GetIO().FontGlobalScale);
            ImGui.SameLine();

            if (ImGui.Button("关闭"))
            {
                this.IsOpen = false;
            }
        }
    }
}
