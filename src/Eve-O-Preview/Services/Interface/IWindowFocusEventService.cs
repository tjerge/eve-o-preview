using System;

namespace EveOPreview.Services
{
	public interface IWindowFocusEventService
	{
		/// <summary>
		/// Event raised when the foreground window changes
		/// </summary>
		event Action<IntPtr> ForegroundWindowChanged;

		/// <summary>
		/// Start monitoring foreground window changes
		/// </summary>
		void Start();

		/// <summary>
		/// Stop monitoring foreground window changes
		/// </summary>
		void Stop();
	}
}
