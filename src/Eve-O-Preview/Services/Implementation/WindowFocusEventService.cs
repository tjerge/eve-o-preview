using System;
using System.Runtime.InteropServices;
using EveOPreview.Services.Interop;

namespace EveOPreview.Services.Implementation
{
	public class WindowFocusEventService : IWindowFocusEventService, IDisposable
	{
#if !LINUX
		private IntPtr _hookHandle = IntPtr.Zero;
		private User32NativeMethods.WinEventDelegate _winEventDelegate;
#endif
		private bool _isStarted;

		public event Action<IntPtr> ForegroundWindowChanged;

		public WindowFocusEventService()
		{
#if !LINUX
			// Keep a reference to the delegate to prevent it from being garbage collected
			_winEventDelegate = new User32NativeMethods.WinEventDelegate(WinEventProc);
#endif
		}

		public void Start()
		{
			if (_isStarted)
			{
				return;
			}

#if !LINUX
			// Set up the hook for foreground window changes (Windows only)
			_hookHandle = User32NativeMethods.SetWinEventHook(
				User32NativeMethods.EVENT_SYSTEM_FOREGROUND,
				User32NativeMethods.EVENT_SYSTEM_FOREGROUND,
				IntPtr.Zero,
				_winEventDelegate,
				0,
				0,
				User32NativeMethods.WINEVENT_OUTOFCONTEXT);
#endif
			_isStarted = true;
		}

		public void Stop()
		{
			if (!_isStarted)
			{
				return;
			}

#if !LINUX
			if (_hookHandle != IntPtr.Zero)
			{
				User32NativeMethods.UnhookWinEvent(_hookHandle);
				_hookHandle = IntPtr.Zero;
			}
#endif
			_isStarted = false;
		}

#if !LINUX
		private void WinEventProc(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
		{
			// Only process foreground window change events
			if (eventType == User32NativeMethods.EVENT_SYSTEM_FOREGROUND)
			{
				// Raise the event on the UI thread if there are subscribers
				ForegroundWindowChanged?.Invoke(hwnd);
			}
		}
#endif

		public void Dispose()
		{
			Stop();
		}
	}
}
