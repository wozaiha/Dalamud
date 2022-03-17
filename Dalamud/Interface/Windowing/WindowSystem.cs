using System;
using System.Collections.Generic;
using System.Linq;

using Dalamud.Interface.Internal.ManagedAsserts;
using ImGuiNET;
using Serilog;

namespace Dalamud.Interface.Windowing
{
    /// <summary>
    /// Class running a WindowSystem using <see cref="Window"/> implementations to simplify ImGui windowing.
    /// </summary>
    public class WindowSystem
    {
        private static DateTimeOffset lastAnyFocus;

        private readonly List<Window> windows = new();

        private string lastFocusedWindowName = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="WindowSystem"/> class.
        /// </summary>
        /// <param name="imNamespace">The name/ID-space of this <see cref="WindowSystem"/>.</param>
        public WindowSystem(string? imNamespace = null)
        {
            this.Namespace = imNamespace;
        }

        /// <summary>
        /// Gets a value indicating whether any <see cref="WindowSystem"/> contains any <see cref="Window"/>
        /// that has focus and is not marked to be excluded from consideration.
        /// </summary>
        public static bool HasAnyWindowSystemFocus { get; internal set; } = false;

        /// <summary>
        /// Gets the name of the currently focused window system that is redirecting normal escape functionality.
        /// </summary>
        public static string FocusedWindowSystemNamespace { get; internal set; } = string.Empty;

        /// <summary>
        /// Gets the timespan since the last time any window was focused.
        /// </summary>
        public static TimeSpan TimeSinceLastAnyFocus => DateTimeOffset.Now - lastAnyFocus;

        /// <summary>
        /// Gets a value indicating whether any window in this <see cref="WindowSystem"/> has focus and is
        /// not marked to be excluded from consideration.
        /// </summary>
        public bool HasAnyFocus { get; private set; }

        /// <summary>
        /// Gets or sets the name/ID-space of this <see cref="WindowSystem"/>.
        /// </summary>
        public string? Namespace { get; set; }

        /// <summary>
        /// Add a window to this <see cref="WindowSystem"/>.
        /// </summary>
        /// <param name="window">The window to add.</param>
        public void AddWindow(Window window)
        {
            if (this.windows.Any(x => x.WindowName == window.WindowName))
                throw new ArgumentException("A window with this name/ID already exists.");

            this.windows.Add(window);
        }

        /// <summary>
        /// Remove a window from this <see cref="WindowSystem"/>.
        /// </summary>
        /// <param name="window">The window to remove.</param>
        public void RemoveWindow(Window window)
        {
            if (!this.windows.Contains(window))
                throw new ArgumentException("This window is not registered on this WindowSystem.");

            this.windows.Remove(window);
        }

        /// <summary>
        /// Remove all windows from this <see cref="WindowSystem"/>.
        /// </summary>
        public void RemoveAllWindows() => this.windows.Clear();

        /// <summary>
        /// Get a window by name.
        /// </summary>
        /// <param name="windowName">The name of the <see cref="Window"/></param>
        /// <returns>The <see cref="Window"/> object with matching name or null.</returns>
        public Window? GetWindow(string windowName) => this.windows.FirstOrDefault(w => w.WindowName == windowName);

        /// <summary>
        /// Draw all registered windows using ImGui.
        /// </summary>
        public void Draw()
        {
            var hasNamespace = !string.IsNullOrEmpty(this.Namespace);

            if (hasNamespace)
                ImGui.PushID(this.Namespace);

            foreach (var window in this.windows)
            {
#if DEBUG
                // Log.Verbose($"[WS{(hasNamespace ? "/" + this.Namespace : string.Empty)}] Drawing {window.WindowName}");
#endif
                var snapshot = ImGuiManagedAsserts.GetSnapshot();

                window.DrawInternal();

                var source = ($"{this.Namespace}::" ?? string.Empty) + window.WindowName;
                ImGuiManagedAsserts.ReportProblems(source, snapshot);
            }

            var focusedWindow = this.windows.FirstOrDefault(window => window.IsFocused && window.RespectCloseHotkey);
            this.HasAnyFocus = focusedWindow != default;

            if (this.HasAnyFocus)
            {
                if (this.lastFocusedWindowName != focusedWindow.WindowName)
                {
                    Log.Verbose($"WindowSystem \"{this.Namespace}\" Window \"{focusedWindow.WindowName}\" has focus now");
                    this.lastFocusedWindowName = focusedWindow.WindowName;
                }

                HasAnyWindowSystemFocus = true;
                FocusedWindowSystemNamespace = this.Namespace;

                lastAnyFocus = DateTimeOffset.Now;
            }
            else
            {
                if (this.lastFocusedWindowName != string.Empty)
                {
                    Log.Verbose($"WindowSystem \"{this.Namespace}\" Window \"{this.lastFocusedWindowName}\" lost focus");
                    this.lastFocusedWindowName = string.Empty;
                }
            }

            if (hasNamespace)
                ImGui.PopID();
        }
    }
}
