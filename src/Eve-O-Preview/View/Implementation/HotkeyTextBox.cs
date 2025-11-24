using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace EveOPreview.View
{
	/// <summary>
	/// A TextBox that captures keyboard input and formats it as a hotkey string
	/// </summary>
	public class HotkeyTextBox : TextBox
	{
		private bool _isCapturing = false;

		public HotkeyTextBox()
		{
			IsReadOnly = true;
			PreviewKeyDown += OnPreviewKeyDown;
			GotFocus += OnGotFocus;
			LostFocus += OnLostFocus;
			MouseDown += OnMouseDown;
		}

		private void OnMouseDown(object sender, MouseButtonEventArgs e)
		{
			// Start capturing on click
			_isCapturing = true;
			Text = "Press a key combination...";
			e.Handled = true;
		}

		private void OnGotFocus(object sender, RoutedEventArgs e)
		{
			_isCapturing = true;
			if (string.IsNullOrEmpty(Text))
			{
				Text = "Press a key combination...";
			}
		}

		private void OnLostFocus(object sender, RoutedEventArgs e)
		{
			_isCapturing = false;
			if (Text == "Press a key combination...")
			{
				Text = string.Empty;
			}
		}

		private void OnPreviewKeyDown(object sender, KeyEventArgs e)
		{
			if (!_isCapturing)
				return;

			e.Handled = true;

			// Get the actual key (not the system key)
			Key key = (e.Key == Key.System) ? e.SystemKey : e.Key;

			// Ignore modifier keys by themselves
			if (key == Key.LeftCtrl || key == Key.RightCtrl ||
				key == Key.LeftAlt || key == Key.RightAlt ||
				key == Key.LeftShift || key == Key.RightShift ||
				key == Key.LWin || key == Key.RWin)
			{
				return;
			}

			// Handle Escape to clear
			if (key == Key.Escape)
			{
				Text = string.Empty;
				_isCapturing = false;
				MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
				return;
			}

			// Build the hotkey string
			StringBuilder hotkeyBuilder = new StringBuilder();

			// Add modifiers
			if ((Keyboard.Modifiers & ModifierKeys.Control) != 0)
			{
				hotkeyBuilder.Append("Control+");
			}
			if ((Keyboard.Modifiers & ModifierKeys.Alt) != 0)
			{
				hotkeyBuilder.Append("Alt+");
			}
			if ((Keyboard.Modifiers & ModifierKeys.Shift) != 0)
			{
				hotkeyBuilder.Append("Shift+");
			}

			// Add the main key
			string keyString = ConvertKeyToString(key);
			hotkeyBuilder.Append(keyString);

			Text = hotkeyBuilder.ToString();
			_isCapturing = false;

			// Move focus to next control
			MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
		}

		private string ConvertKeyToString(Key key)
		{
			// Handle function keys F1-F24
			if (key >= Key.F1 && key <= Key.F24)
			{
				return key.ToString();
			}

			// Handle number pad
			if (key >= Key.NumPad0 && key <= Key.NumPad9)
			{
				return "NumPad" + (key - Key.NumPad0);
			}

			// Handle special keys
			switch (key)
			{
				case Key.D0: return "0";
				case Key.D1: return "1";
				case Key.D2: return "2";
				case Key.D3: return "3";
				case Key.D4: return "4";
				case Key.D5: return "5";
				case Key.D6: return "6";
				case Key.D7: return "7";
				case Key.D8: return "8";
				case Key.D9: return "9";
				case Key.OemPlus: return "Add";
				case Key.OemMinus: return "Subtract";
				case Key.OemPeriod: return "Decimal";
				case Key.Multiply: return "Multiply";
				case Key.Divide: return "Divide";
				case Key.Return: return "Enter";
				case Key.Back: return "Back";
				case Key.Space: return "Space";
				case Key.Prior: return "PageUp";
				case Key.Next: return "PageDown";
				default:
					return key.ToString();
			}
		}
	}
}
