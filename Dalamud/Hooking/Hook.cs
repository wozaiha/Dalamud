using System;
using System.Reflection;

using Dalamud.Configuration.Internal;
using Dalamud.Hooking.Internal;
using Dalamud.Memory;
using Reloaded.Hooks;
using Serilog;

namespace Dalamud.Hooking
{
    /// <summary>
    /// Manages a hook which can be used to intercept a call to native function.
    /// This class is basically a thin wrapper around the LocalHook type to provide helper functions.
    /// </summary>
    /// <typeparam name="T">Delegate type to represents a function prototype. This must be the same prototype as original function do.</typeparam>
    public sealed class Hook<T> : IDisposable, IDalamudHook where T : Delegate
    {
        private readonly IntPtr address;
        private readonly Reloaded.Hooks.Definitions.IHook<T> hookImpl;
        private readonly CoreHook.IHook<T> coreHookImpl;
        private readonly bool isCoreHook;

        /// <summary>
        /// Initializes a new instance of the <see cref="Hook{T}"/> class.
        /// Hook is not activated until Enable() method is called.
        /// </summary>
        /// <param name="address">A memory address to install a hook.</param>
        /// <param name="detour">Callback function. Delegate must have a same original function prototype.</param>
        public Hook(IntPtr address, T detour)
            : this(address, detour, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Hook{T}"/> class.
        /// Hook is not activated until Enable() method is called.
        /// Please do not use CoreHook unless you have thoroughly troubleshot why Reloaded does not work.
        /// </summary>
        /// <param name="address">A memory address to install a hook.</param>
        /// <param name="detour">Callback function. Delegate must have a same original function prototype.</param>
        /// <param name="useCoreHook">Use the corehook hooking library instead of Reloaded.</param>
        public Hook(IntPtr address, T detour, bool useCoreHook)
        {
            address = HookManager.FollowJmp(address);
            this.isCoreHook = useCoreHook || EnvironmentConfiguration.DalamudForceCoreHook;

            var hasOtherHooks = HookManager.Originals.ContainsKey(address);
            if (!hasOtherHooks)
            {
                MemoryHelper.ReadRaw(address, 0x32, out var original);
                HookManager.Originals[address] = original;
            }

            this.address = address;
            if (this.isCoreHook)
            {
                try
                {
                    this.coreHookImpl = CoreHook.HookFactory.CreateHook<T>(address, detour);
                }
                catch (Exception ex)
                {
                    this.isCoreHook = false;
                    Log.Error(ex, "CoreHook is having a bad day, defaulting to Reloaded");
                    this.hookImpl = ReloadedHooks.Instance.CreateHook<T>(detour, address.ToInt64());
                }
            }
            else
            {
                this.hookImpl = ReloadedHooks.Instance.CreateHook<T>(detour, address.ToInt64());
            }

            HookManager.TrackedHooks.Add(new HookInfo(this, detour, Assembly.GetCallingAssembly()));
        }

        /// <summary>
        /// Gets a memory address of the target function.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Hook is already disposed.</exception>
        public IntPtr Address
        {
            get
            {
                this.CheckDisposed();
                return this.address;
            }
        }

        /// <summary>
        /// Gets a delegate function that can be used to call the actual function as if function is not hooked yet.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Hook is already disposed.</exception>
        public T Original
        {
            get
            {
                this.CheckDisposed();
                if (this.isCoreHook)
                {
                    return this.coreHookImpl.Original;
                }
                else
                {
                    return this.hookImpl.OriginalFunction;
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether or not the hook is enabled.
        /// </summary>
        public bool IsEnabled
        {
            get
            {
                this.CheckDisposed();
                if (this.isCoreHook)
                {
                    return this.coreHookImpl.Enabled;
                }
                else
                {
                    return this.hookImpl.IsHookEnabled;
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether or not the hook has been disposed.
        /// </summary>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Creates a hook. Hooking address is inferred by calling to GetProcAddress() function.
        /// The hook is not activated until Enable() method is called.
        /// </summary>
        /// <param name="moduleName">A name of the module currently loaded in the memory. (e.g. ws2_32.dll).</param>
        /// <param name="exportName">A name of the exported function name (e.g. send).</param>
        /// <param name="detour">Callback function. Delegate must have a same original function prototype.</param>
        /// <returns>The hook with the supplied parameters.</returns>
        public static Hook<T> FromSymbol(string moduleName, string exportName, T detour)
            => FromSymbol(moduleName, exportName, detour, false);

        /// <summary>
        /// Creates a hook. Hooking address is inferred by calling to GetProcAddress() function.
        /// The hook is not activated until Enable() method is called.
        /// Please do not use CoreHook unless you have thoroughly troubleshot why Reloaded does not work.
        /// </summary>
        /// <param name="moduleName">A name of the module currently loaded in the memory. (e.g. ws2_32.dll).</param>
        /// <param name="exportName">A name of the exported function name (e.g. send).</param>
        /// <param name="detour">Callback function. Delegate must have a same original function prototype.</param>
        /// <param name="useCoreHook">Use the corehook hooking library instead of Reloaded.</param>
        /// <returns>The hook with the supplied parameters.</returns>
        public static Hook<T> FromSymbol(string moduleName, string exportName, T detour, bool useCoreHook)
        {
            var moduleHandle = NativeFunctions.GetModuleHandleW(moduleName);
            if (moduleHandle == IntPtr.Zero)
                throw new Exception($"Could not get a handle to module {moduleName}");

            var procAddress = NativeFunctions.GetProcAddress(moduleHandle, exportName);
            if (procAddress == IntPtr.Zero)
                throw new Exception($"Could not get the address of {moduleName}::{exportName}");

            return new Hook<T>(procAddress, detour, useCoreHook);
        }

        /// <summary>
        /// Remove a hook from the current process.
        /// </summary>
        public void Dispose()
        {
            if (this.IsDisposed)
                return;

            if (this.isCoreHook)
            {
                this.Disable();
                // Disposing CoreHook causes an APPCRASH on game exit.
                // We already overwrite the original hook code, so there shouldn't be any real risk with not disposing here.
                // this.coreHookImpl.Dispose();
            }
            else
            {
                this.Disable();
            }

            this.IsDisposed = true;
        }

        /// <summary>
        /// Starts intercepting a call to the function.
        /// </summary>
        public void Enable()
        {
            this.CheckDisposed();

            if (this.isCoreHook)
            {
                if (!this.coreHookImpl.Enabled)
                    this.coreHookImpl.Enabled = true;
            }
            else
            {
                if (!this.hookImpl.IsHookActivated)
                    this.hookImpl.Activate();

                if (!this.hookImpl.IsHookEnabled)
                    this.hookImpl.Enable();
            }
        }

        /// <summary>
        /// Stops intercepting a call to the function.
        /// </summary>
        public void Disable()
        {
            this.CheckDisposed();

            if (this.isCoreHook)
            {
                if (this.coreHookImpl.Enabled)
                    this.coreHookImpl.Enabled = false;
            }
            else
            {
                if (!this.hookImpl.IsHookActivated)
                    return;

                if (this.hookImpl.IsHookEnabled)
                    this.hookImpl.Disable();
            }
        }

        /// <summary>
        /// Check if this object has been disposed already.
        /// </summary>
        private void CheckDisposed()
        {
            if (this.IsDisposed)
            {
                throw new ObjectDisposedException(message: "Hook is already disposed", null);
            }
        }
    }
}
