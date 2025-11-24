using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using EveOPreview.Configuration;
using EveOPreview.Services;

namespace EveOPreview.View
{
	/// <summary>
	/// Helper class to launch the WPF Configuration Window from WinForms
	/// </summary>
	public static class ConfigurationWindowLauncher
	{
		private static bool _isWpfInitialized = false;

		public static bool? ShowDialog(IThumbnailConfiguration config, IThumbnailManager thumbnailManager = null, IConfigurationStorage configurationStorage = null, Action onConfigurationSaved = null)
		{
			try
			{
				LogDebug("=== ConfigurationWindowLauncher.ShowDialog called ===");
				
				// Initialize WPF if needed (only once)
				if (!_isWpfInitialized)
				{
					LogDebug("Initializing WPF...");
					InitializeWpf();
					_isWpfInitialized = true;
					LogDebug("WPF initialized successfully");
				}
			else
			{
				LogDebug("WPF already initialized");
			}

			LogDebug("Creating ConfigurationWindow...");
			var window = new ConfigurationWindow(config, thumbnailManager, configurationStorage, onConfigurationSaved);
			LogDebug("ConfigurationWindow created, showing dialog...");
			
			var result = window.ShowDialog();
			LogDebug($"Dialog closed with result: {result}");				return result;
			}
			catch (Exception ex)
			{
				LogError("EXCEPTION in ShowDialog", ex);
				
				// Write to file for sure
				WriteErrorToFile(ex);
				
				// Log the actual error for debugging
				System.Windows.Forms.MessageBox.Show(
					$"Failed to open WPF configuration window:\n\n{ex.Message}\n\nInner: {ex.InnerException?.Message}\n\nSee log file for details.",
					"WPF Configuration Error",
					System.Windows.Forms.MessageBoxButtons.OK,
					System.Windows.Forms.MessageBoxIcon.Error);
				return null;
			}
		}

		private static void InitializeWpf()
		{
			try
			{
				LogDebug("InitializeWpf: Starting...");
				
				if (Application.Current == null)
				{
					LogDebug("InitializeWpf: Creating Application instance...");
					// Create a new WPF Application instance
					var app = new Application();
					app.ShutdownMode = ShutdownMode.OnExplicitShutdown;
					LogDebug("InitializeWpf: Application instance created");

				// Load MaterialDesign resources at the application level
				LogDebug("InitializeWpf: Loading MaterialDesign resources...");
				
				// Note: We'll let the window load its own resources from XAML
				// Just set up a basic resource dictionary
				app.Resources = new ResourceDictionary();
				LogDebug("InitializeWpf: Basic resources initialized");
			}
			else
			{
				LogDebug("InitializeWpf: Application.Current already exists");
			}
		}
		catch (Exception ex)
		{
			LogError("InitializeWpf: Fatal error", ex);
			throw;
		}
	}		private static void LogDebug(string message)
		{
			var fullMessage = $"[WPF-Config] {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} - {message}";
			Debug.WriteLine(fullMessage);
			Console.WriteLine(fullMessage);
			
			try
			{
				var logPath = Path.Combine(Path.GetTempPath(), "eve-o-preview-wpf-debug.log");
				File.AppendAllText(logPath, fullMessage + Environment.NewLine);
			}
			catch
			{
				// Ignore file write errors
			}
		}

		private static void LogError(string message, Exception ex)
		{
			var fullMessage = $"[WPF-Config-ERROR] {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} - {message}\n" +
			                  $"Exception: {ex.GetType().Name}\n" +
			                  $"Message: {ex.Message}\n" +
			                  $"StackTrace: {ex.StackTrace}\n";
			
			if (ex.InnerException != null)
			{
				fullMessage += $"InnerException: {ex.InnerException.GetType().Name}\n" +
				              $"InnerMessage: {ex.InnerException.Message}\n" +
				              $"InnerStackTrace: {ex.InnerException.StackTrace}\n";
			}
			
			Debug.WriteLine(fullMessage);
			Console.WriteLine(fullMessage);
			
			try
			{
				var logPath = Path.Combine(Path.GetTempPath(), "eve-o-preview-wpf-debug.log");
				File.AppendAllText(logPath, fullMessage + Environment.NewLine);
			}
			catch
			{
				// Ignore file write errors
			}
		}

		private static void WriteErrorToFile(Exception ex)
		{
			try
			{
				var errorPath = Path.Combine(Path.GetTempPath(), "eve-o-preview-wpf-error.txt");
				var errorText = $"EVE-O-Preview WPF Configuration Error\n" +
				               $"Time: {DateTime.Now}\n" +
				               $"Exception: {ex.GetType().FullName}\n" +
				               $"Message: {ex.Message}\n\n" +
				               $"StackTrace:\n{ex.StackTrace}\n\n";
				
				if (ex.InnerException != null)
				{
					errorText += $"InnerException: {ex.InnerException.GetType().FullName}\n" +
					            $"InnerMessage: {ex.InnerException.Message}\n" +
					            $"InnerStackTrace:\n{ex.InnerException.StackTrace}\n";
				}
				
				File.WriteAllText(errorPath, errorText);
				LogDebug($"Error details written to: {errorPath}");
			}
			catch (Exception writeEx)
			{
				Debug.WriteLine($"Failed to write error file: {writeEx.Message}");
			}
		}
	}
}
