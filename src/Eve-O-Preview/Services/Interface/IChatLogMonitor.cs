using System;

namespace EveOPreview.Services
{
	/// <summary>
	/// Interface for monitoring EVE Online chatlog files to track character locations
	/// </summary>
	public interface IChatlogMonitor
	{
		/// <summary>
		/// Event raised when a character's system location changes
		/// </summary>
		event Action<string, string> SystemChanged; // (characterName, systemName)

		/// <summary>
		/// Start monitoring chatlog files
		/// </summary>
		void Start();

		/// <summary>
		/// Stop monitoring chatlog files
		/// </summary>
		void Stop();

		/// <summary>
		/// Get the current system name for a character
		/// </summary>
		/// <param name="characterName">The character name</param>
		/// <returns>The current system name or null if unknown</returns>
		string GetCurrentSystem(string characterName);
	}
}
