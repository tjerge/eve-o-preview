using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using EveOPreview.Configuration;

namespace EveOPreview.Services.Implementation
{
	/// <summary>
	/// Monitors EVE Online chatlog files to track character locations
	/// </summary>
	class ChatlogMonitor : IChatlogMonitor
	{
		#region Private constants
		private const string LOCAL_CHANNEL_ID = "local";
		private static readonly Regex ListenerRegex = new Regex(@"Listener:\s+(.+)", RegexOptions.Compiled);
		private static readonly Regex SystemChangeRegex = new Regex(@"Channel changed to Local : (.+)", RegexOptions.Compiled);
		#endregion

	#region Private fields
private readonly IThumbnailConfiguration _configuration;
private readonly ConcurrentDictionary<string, string> _characterSystems;
private readonly ConcurrentDictionary<string, FileSystemWatcher> _watchers;
private readonly ConcurrentDictionary<string, string> _watcherToCharacter;
private readonly object _syncLock = new object();
private bool _isRunning;
private FileSystemWatcher _directoryWatcher;
private string _chatlogPath; // Cached chatlog directory path
	#endregion

	public event Action<string, string> SystemChanged;

	public ChatlogMonitor(IThumbnailConfiguration configuration)
		{
			_configuration = configuration;
			_characterSystems = new ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase);
			_watchers = new ConcurrentDictionary<string, FileSystemWatcher>();
			_watcherToCharacter = new ConcurrentDictionary<string, string>();
		}

	public void Start()
	{
		lock (_syncLock)
		{
			if (_isRunning)
				return;

			_isRunning = true;
			
			// Do initial scan
			ScanForChatlogs();
			
			// Set up directory watcher for new files
			SetupDirectoryWatcher();
		}
	}		public void Stop()
		{
			lock (_syncLock)
			{
				if (!_isRunning)
					return;

				_isRunning = false;

				// Stop all watchers
				foreach (var watcher in _watchers.Values)
				{
					try
					{
						watcher.EnableRaisingEvents = false;
						watcher.Dispose();
					}
					catch { }
				}

				_watchers.Clear();
				_watcherToCharacter.Clear();
			}
		}

		public string GetCurrentSystem(string characterName)
		{
			if (string.IsNullOrEmpty(characterName))
				return null;

			_characterSystems.TryGetValue(characterName, out string system);
			return system;
		}

	#region Private methods

	private void SetupDirectoryWatcher()
	{
		try
		{
			// Use cached path from ScanForChatlogs, or find it if not cached
			if (string.IsNullOrEmpty(_chatlogPath))
			{
				_chatlogPath = FindChatlogDirectory();
			}
			
			if (string.IsNullOrEmpty(_chatlogPath))
			{				return;
			}

			_directoryWatcher = new FileSystemWatcher
			{
				Path = _chatlogPath,
				Filter = "Local_*.txt",
				NotifyFilter = NotifyFilters.FileName | NotifyFilters.CreationTime
			};

			_directoryWatcher.Created += OnNewChatlogFileCreated;
			_directoryWatcher.EnableRaisingEvents = true;		}
		catch
		{		}
	}

	private void OnNewChatlogFileCreated(object sender, FileSystemEventArgs e)
	{
		try
		{			
			// Wait a moment for the file to be written
			Thread.Sleep(500);
			
			// Process this new file
			ProcessChatlogFile(e.FullPath);
		}
		catch
		{		}
	}

	private void ScanForChatlogs()
	{
		try
		{
			// Find and cache the chatlog path
			_chatlogPath = FindChatlogDirectory();
			
			if (string.IsNullOrEmpty(_chatlogPath))
			{				return;
			}

			// Look for Local chat logs
			var localChatlogs = Directory.GetFiles(_chatlogPath, "Local_*.txt");
			// Group by character ID (extracted from filename) and get only the most recent file for each
			// Filename format: Local_YYYYMMDD_HHMMSS_CharacterID.txt
			var characterIdFiles = new Dictionary<string, (string FilePath, DateTime Timestamp)>();

			foreach (var logFile in localChatlogs)
			{
				if (!_isRunning)
					break;

				try
				{
					// Extract character ID from filename: Local_20251123_022507_94687812.txt
					string filename = Path.GetFileNameWithoutExtension(logFile);
					string[] parts = filename.Split('_');
					
					if (parts.Length != 4)
						continue;

					string characterId = parts[3]; // The character ID
					string dateStr = parts[1];     // YYYYMMDD
					string timeStr = parts[2];     // HHMMSS

					// Parse the timestamp from the filename
					DateTime timestamp;
					try
					{
						int year = int.Parse(dateStr.Substring(0, 4));
						int month = int.Parse(dateStr.Substring(4, 2));
						int day = int.Parse(dateStr.Substring(6, 2));
						int hour = int.Parse(timeStr.Substring(0, 2));
						int minute = int.Parse(timeStr.Substring(2, 2));
						int second = int.Parse(timeStr.Substring(4, 2));
						timestamp = new DateTime(year, month, day, hour, minute, second);
					}
					catch
					{
						// If we can't parse the timestamp, fall back to file write time
						timestamp = File.GetLastWriteTime(logFile);
					}

					// Keep only the most recent file for each character ID
					if (!characterIdFiles.ContainsKey(characterId) || timestamp > characterIdFiles[characterId].Timestamp)
					{
						characterIdFiles[characterId] = (logFile, timestamp);
					}
				}
				catch
				{				}
			}
			// Now process only the latest file for each character ID
			foreach (var kvp in characterIdFiles)
			{
				if (!_isRunning)
					break;

				ProcessChatlogFile(kvp.Value.FilePath);
			}
		}
		catch
		{		}
	}

	private void ProcessChatlogFile(string logFile)
	{
		try
		{
			// Read the file header to get the character name
			string characterName = ExtractCharacterName(logFile);
			if (string.IsNullOrEmpty(characterName))
				return;

			// Check if we're already watching this file
			if (_watchers.ContainsKey(logFile))
				return;

			// If we're watching an old file for this character, stop watching it
			var oldWatcher = _watcherToCharacter.FirstOrDefault(x => x.Value == characterName);
			if (!string.IsNullOrEmpty(oldWatcher.Key) && oldWatcher.Key != logFile)
			{
				if (_watchers.TryRemove(oldWatcher.Key, out var watcher))
				{
					watcher.EnableRaisingEvents = false;
					watcher.Dispose();				}
				_watcherToCharacter.TryRemove(oldWatcher.Key, out _);
			}

			// Get the current system from the file
			string currentSystem = ExtractCurrentSystem(logFile);
			if (!string.IsNullOrEmpty(currentSystem))
			{
				UpdateCharacterSystem(characterName, currentSystem);
			}

			// Set up a file watcher for this chatlog
			SetupFileWatcher(logFile, characterName);
		}
		catch
		{		}
	}

	private string FindChatlogDirectory()
	{
		// If a path is configured and exists, use it
		if (!string.IsNullOrEmpty(_configuration.ChatlogPath) && Directory.Exists(_configuration.ChatlogPath))
		{			return _configuration.ChatlogPath;
		}

		// Common EVE Online chatlog locations to check
		var locationsToCheck = new List<string>();

		// Standard Windows location
		string myDocs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
		locationsToCheck.Add(Path.Combine(myDocs, @"EVE\logs\Chatlogs"));

		// Local AppData (sometimes used)
		string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
		locationsToCheck.Add(Path.Combine(localAppData, @"CCP\EVE\logs\Chatlogs"));

		// Roaming AppData
		string roamingAppData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
		locationsToCheck.Add(Path.Combine(roamingAppData, @"CCP\EVE\logs\Chatlogs"));

		// Common custom install locations
		locationsToCheck.Add(@"C:\EVE\logs\Chatlogs");
		locationsToCheck.Add(@"C:\Games\EVE\logs\Chatlogs");
		locationsToCheck.Add(@"D:\EVE\logs\Chatlogs");
		locationsToCheck.Add(@"D:\Games\EVE\logs\Chatlogs");
		// Check each location
		foreach (var location in locationsToCheck)
		{			
			if (Directory.Exists(location))
			{				
			// Verify it actually contains chatlog files
			try
			{
				var files = Directory.GetFiles(location, "Local_*.txt");					if (files.Length > 0)
					{						return location;
					}
				}
				catch
				{				}
			}
			else
			{			}
		}		System.Diagnostics.Debug.WriteLine($"[ChatlogMonitor] Typically located at: {Path.Combine(myDocs, @"EVE\logs\Chatlogs")}");
		return null;
	}

	private void SetupFileWatcher(string filePath, string characterName)
		{
			try
			{
				var watcher = new FileSystemWatcher
				{
					Path = Path.GetDirectoryName(filePath),
					Filter = Path.GetFileName(filePath),
					NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size
				};

				watcher.Changed += (sender, e) => OnChatlogFileChanged(e.FullPath);
				watcher.EnableRaisingEvents = true;

				_watchers[filePath] = watcher;
				_watcherToCharacter[filePath] = characterName;			}
			catch
			{			}
		}

		private void OnChatlogFileChanged(string filePath)
		{
			try
			{
				if (!_watcherToCharacter.TryGetValue(filePath, out string characterName))
					return;

				// Read the last few lines to check for system changes
				string lastSystemChange = ReadLastSystemChange(filePath);
				
				if (!string.IsNullOrEmpty(lastSystemChange))
				{
					UpdateCharacterSystem(characterName, lastSystemChange);
				}
			}
			catch
			{			}
		}

		private string ExtractCharacterName(string filePath)
		{
			try
			{
				// Read first 20 lines to find the Listener field
				using (var reader = new StreamReader(new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
				{
					for (int i = 0; i < 20 && !reader.EndOfStream; i++)
					{
						string line = reader.ReadLine();
						var match = ListenerRegex.Match(line);
						if (match.Success)
						{
							return match.Groups[1].Value.Trim();
						}
					}
				}
			}
			catch
			{			}

			return null;
		}

		private string ExtractCurrentSystem(string filePath)
		{
			try
			{
				// Read the file backwards to find the most recent system change
				return ReadLastSystemChange(filePath);
			}
			catch
			{				return null;
			}
		}

		private string ReadLastSystemChange(string filePath)
		{
			try
			{
				// Read the last portion of the file to find system changes
				using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
				{
					// If file is small, read all
					if (stream.Length < 10000)
					{
						stream.Seek(0, SeekOrigin.Begin);
					}
					else
					{
						// Read last 10KB
						stream.Seek(-10000, SeekOrigin.End);
					}

					using (var reader = new StreamReader(stream))
					{
						var lines = new List<string>();
						while (!reader.EndOfStream)
						{
							lines.Add(reader.ReadLine());
						}

						// Search backwards for system change
						for (int i = lines.Count - 1; i >= 0; i--)
						{
							var match = SystemChangeRegex.Match(lines[i]);
							if (match.Success)
							{
								return match.Groups[1].Value.Trim();
							}
						}
					}
				}
			}
			catch
			{			}

			return null;
		}

	private void UpdateCharacterSystem(string characterName, string systemName)
	{
		bool changed = false;
		if (_characterSystems.TryGetValue(characterName, out string currentSystem))
		{
			if (currentSystem != systemName)
			{
				_characterSystems[characterName] = systemName;
				changed = true;
			}
		}
		else
		{
			_characterSystems[characterName] = systemName;
			changed = true;
		}

		if (changed)
		{		SystemChanged?.Invoke(characterName, systemName);
	}
}

#endregion
}
}