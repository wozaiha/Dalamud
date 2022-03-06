using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

using Dalamud.Configuration.Internal;
using Dalamud.Game;
using Dalamud.Game.ClientState;
using Dalamud.Game.Gui;
using Dalamud.Interface.Animation.EasingFunctions;
using Dalamud.Interface.GameFonts;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using ImGuiScene;

namespace Dalamud.Interface.Internal.Windows
{
    /// <summary>
    /// Class responsible for drawing the main plugin window.
    /// </summary>
    internal class TitleScreenMenuWindow : Window, IDisposable
    {
        private const float TargetFontSize = 16.2f;
        private readonly TextureWrap shadeTexture;

        private readonly Dictionary<Guid, InOutCubic> shadeEasings = new();
        private readonly Dictionary<Guid, InOutQuint> moveEasings = new();
        private readonly Dictionary<Guid, InOutCubic> logoEasings = new();

        private InOutCubic? fadeOutEasing;

        private GameFontHandle? axisFontHandle;

        private State state = State.Hide;

        /// <summary>
        /// Initializes a new instance of the <see cref="TitleScreenMenuWindow"/> class.
        /// </summary>
        public TitleScreenMenuWindow()
            : base(
                "TitleScreenMenuOverlay",
                ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoScrollbar |
                   ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoFocusOnAppearing | ImGuiWindowFlags.NoNavFocus)
        {
            this.IsOpen = true;

            this.ForceMainWindow = true;

            this.Position = new Vector2(0, 200);
            this.PositionCondition = ImGuiCond.Always;
            this.RespectCloseHotkey = false;

            var dalamud = Service<Dalamud>.Get();
            var interfaceManager = Service<InterfaceManager>.Get();

            var shadeTex =
                interfaceManager.LoadImage(Path.Combine(dalamud.AssetDirectory.FullName, "UIRes", "tsmShade.png"));
            this.shadeTexture = shadeTex ?? throw new Exception("Could not load TSM background texture.");

            var framework = Service<Framework>.Get();
            framework.Update += this.FrameworkOnUpdate;
        }

        private enum State
        {
            Hide,
            Show,
            FadeOut,
        }

        /// <inheritdoc/>
        public override void PreDraw()
        {
            this.SetAxisFonts();
            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(0, 0));
            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(0, 0));
            if (this.axisFontHandle?.Available ?? false)
                ImGui.PushFont(this.axisFontHandle.ImFont);
            base.PreDraw();
        }

        /// <inheritdoc/>
        public override void PostDraw()
        {
            if (this.axisFontHandle?.Available ?? false)
                ImGui.PopFont();
            ImGui.PopStyleVar(2);
            base.PostDraw();
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            this.shadeTexture.Dispose();
            var framework = Service<Framework>.Get();
            framework.Update -= this.FrameworkOnUpdate;
        }

        /// <inheritdoc/>
        public override void Draw()
        {
            ImGui.SetWindowFontScale(TargetFontSize / ImGui.GetFont().FontSize * 4 / 3);

            var tsm = Service<TitleScreenMenu>.Get();

            switch (this.state)
            {
                case State.Show:
                    {
                        for (var i = 0; i < tsm.Entries.Count; i++)
                        {
                            var entry = tsm.Entries[i];

                            if (!this.moveEasings.TryGetValue(entry.Id, out var moveEasing))
                            {
                                moveEasing = new InOutQuint(TimeSpan.FromMilliseconds(400));
                                this.moveEasings.Add(entry.Id, moveEasing);
                            }

                            if (!moveEasing.IsRunning && !moveEasing.IsDone)
                            {
                                moveEasing.Restart();
                            }

                            if (moveEasing.IsDone)
                            {
                                moveEasing.Stop();
                            }

                            moveEasing.Update();

                            var finalPos = (i + 1) * this.shadeTexture.Height;
                            var pos = moveEasing.Value * finalPos;

                            // FIXME(goat): Sometimes, easings can overshoot and bring things out of alignment.
                            if (moveEasing.IsDone)
                            {
                                pos = finalPos;
                            }

                            this.DrawEntry(entry, moveEasing.IsRunning && i != 0, true, i == 0, true);

                            var cursor = ImGui.GetCursorPos();
                            cursor.Y = (float)pos;
                            ImGui.SetCursorPos(cursor);
                        }

                        if (!ImGui.IsWindowHovered(ImGuiHoveredFlags.RootAndChildWindows |
                                                   ImGuiHoveredFlags.AllowWhenOverlapped |
                                                   ImGuiHoveredFlags.AllowWhenBlockedByActiveItem))
                        {
                            this.state = State.FadeOut;
                        }

                        break;
                    }

                case State.FadeOut:
                    {
                        this.fadeOutEasing ??= new InOutCubic(TimeSpan.FromMilliseconds(400))
                        {
                            IsInverse = true,
                        };

                        if (!this.fadeOutEasing.IsRunning && !this.fadeOutEasing.IsDone)
                        {
                            this.fadeOutEasing.Restart();
                        }

                        if (this.fadeOutEasing.IsDone)
                        {
                            this.fadeOutEasing.Stop();
                        }

                        this.fadeOutEasing.Update();

                        ImGui.PushStyleVar(ImGuiStyleVar.Alpha, (float)this.fadeOutEasing.Value);

                        for (var i = 0; i < tsm.Entries.Count; i++)
                        {
                            var entry = tsm.Entries[i];

                            var finalPos = (i + 1) * this.shadeTexture.Height;

                            this.DrawEntry(entry, i != 0, true, i == 0, false);

                            var cursor = ImGui.GetCursorPos();
                            cursor.Y = finalPos;
                            ImGui.SetCursorPos(cursor);
                        }

                        ImGui.PopStyleVar();

                        var isHover = ImGui.IsWindowHovered(ImGuiHoveredFlags.RootAndChildWindows |
                                                            ImGuiHoveredFlags.AllowWhenOverlapped |
                                                            ImGuiHoveredFlags.AllowWhenBlockedByActiveItem);

                        if (!isHover && this.fadeOutEasing!.IsDone)
                        {
                            this.state = State.Hide;
                            this.fadeOutEasing = null;
                        }
                        else if (isHover)
                        {
                            this.state = State.Show;
                            this.fadeOutEasing = null;
                        }

                        break;
                    }

                case State.Hide:
                    {
                        if (this.DrawEntry(tsm.Entries[0], true, false, true, true))
                        {
                            this.state = State.Show;
                        }

                        this.moveEasings.Clear();
                        this.logoEasings.Clear();
                        this.shadeEasings.Clear();
                        break;
                    }
            }
        }

        private void SetAxisFonts()
        {
            var configuration = Service<DalamudConfiguration>.Get();
            if (configuration.UseAxisFontsFromGame)
            {
                if (this.axisFontHandle == null)
                    this.axisFontHandle = Service<GameFontManager>.Get().NewFontRef(new(GameFontFamily.Axis, TargetFontSize));
            }
            else
            {
                this.axisFontHandle?.Dispose();
                this.axisFontHandle = null;
            }
        }

        private bool DrawEntry(
            TitleScreenMenu.TitleScreenMenuEntry entry, bool inhibitFadeout, bool showText, bool isFirst, bool overrideAlpha)
        {
            if (!this.shadeEasings.TryGetValue(entry.Id, out var shadeEasing))
            {
                shadeEasing = new InOutCubic(TimeSpan.FromMilliseconds(350));
                this.shadeEasings.Add(entry.Id, shadeEasing);
            }

            var initialCursor = ImGui.GetCursorPos();

            ImGui.PushStyleVar(ImGuiStyleVar.Alpha, (float)shadeEasing.Value);
            ImGui.Image(this.shadeTexture.ImGuiHandle, new Vector2(this.shadeTexture.Width, this.shadeTexture.Height));
            ImGui.PopStyleVar();

            var isHover = ImGui.IsItemHovered();
            if (isHover && (!shadeEasing.IsRunning || (shadeEasing.IsDone && shadeEasing.IsInverse)) && !inhibitFadeout)
            {
                shadeEasing.IsInverse = false;
                shadeEasing.Restart();
            }
            else if (!isHover && !shadeEasing.IsInverse && shadeEasing.IsRunning && !inhibitFadeout)
            {
                shadeEasing.IsInverse = true;
                shadeEasing.Restart();
            }

            var isClick = ImGui.IsItemClicked();
            if (isClick)
            {
                entry.Trigger();
            }

            shadeEasing.Update();

            if (!this.logoEasings.TryGetValue(entry.Id, out var logoEasing))
            {
                logoEasing = new InOutCubic(TimeSpan.FromMilliseconds(350));
                this.logoEasings.Add(entry.Id, logoEasing);
            }

            if (!logoEasing.IsRunning && !logoEasing.IsDone)
            {
                logoEasing.Restart();
            }

            if (logoEasing.IsDone)
            {
                logoEasing.Stop();
            }

            logoEasing.Update();

            ImGui.SetCursorPos(initialCursor);
            ImGuiHelpers.ScaledDummy(5);
            ImGui.SameLine();

            if (overrideAlpha)
            {
                ImGui.PushStyleVar(ImGuiStyleVar.Alpha, isFirst ? 1f : (float)logoEasing.Value);
            }
            else if (isFirst)
            {
                ImGui.PushStyleVar(ImGuiStyleVar.Alpha, 1f);
            }

            ImGui.Image(entry.Texture.ImGuiHandle, new Vector2(TitleScreenMenu.TextureSize));
            if (overrideAlpha || isFirst)
            {
                ImGui.PopStyleVar();
            }

            ImGui.SameLine();

            ImGuiHelpers.ScaledDummy(10);
            ImGui.SameLine();

            var textHeight = ImGui.GetTextLineHeightWithSpacing();
            var cursor = ImGui.GetCursorPos();

            cursor.Y += (entry.Texture.Height / 2) - (textHeight / 2);
            ImGui.SetCursorPos(cursor);

            if (overrideAlpha)
            {
                ImGui.PushStyleVar(ImGuiStyleVar.Alpha, showText ? (float)logoEasing.Value : 0f);
            }

            ImGui.Text(entry.Name);
            if (overrideAlpha)
            {
                ImGui.PopStyleVar();
            }

            initialCursor.Y += entry.Texture.Height;
            ImGui.SetCursorPos(initialCursor);

            return isHover;
        }

        private void FrameworkOnUpdate(Framework framework)
        {
            var clientState = Service<ClientState>.Get();
            this.IsOpen = !clientState.IsLoggedIn;

            var configuration = Service<DalamudConfiguration>.Get();
            if (!configuration.ShowTsm)
                this.IsOpen = false;

            var gameGui = Service<GameGui>.Get();
            var charaSelect = gameGui.GetAddonByName("CharaSelect", 1);
            var charaMake = gameGui.GetAddonByName("CharaMake", 1);
            var titleDcWorldMap = gameGui.GetAddonByName("TitleDCWorldMap", 1);
            if (charaMake != IntPtr.Zero || charaSelect != IntPtr.Zero || titleDcWorldMap != IntPtr.Zero)
                this.IsOpen = false;
        }
    }
}
