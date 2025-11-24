using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace EveOPreview.View
{
	/// <summary>
	/// A control for capturing multiple hotkeys with inline key detection
	/// </summary>
	public class MultiHotkeyControl : StackPanel
	{
		private TextBox _captureTextBox;
		private Button _clearButton;
		private List<string> _hotkeys = new List<string>();
		private bool _isCapturing = false;

		public static readonly DependencyProperty HotkeysProperty =
			DependencyProperty.Register(
				nameof(Hotkeys),
				typeof(string),
				typeof(MultiHotkeyControl),
				new FrameworkPropertyMetadata(
					string.Empty,
					FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
					OnHotkeysChanged));

		public string Hotkeys
		{
			get => (string)GetValue(HotkeysProperty);
			set => SetValue(HotkeysProperty, value);
		}

		public MultiHotkeyControl()
		{
			Orientation = Orientation.Horizontal;

			_captureTextBox = new TextBox
			{
				IsReadOnly = true,
				Cursor = Cursors.Hand,
				ToolTip = "Click to add hotkey. While capturing: press Delete to remove last hotkey, ESC or OK to finish",
				MinWidth = 200,
				VerticalContentAlignment = VerticalAlignment.Center,
				Margin = new Thickness(0, 0, 8, 0)
			};

			_captureTextBox.PreviewMouseDown += OnPreviewMouseDown;
			_captureTextBox.PreviewKeyDown += OnPreviewKeyDown;
			_captureTextBox.LostFocus += OnLostFocus;

			var okButton = new Button
			{
				Content = "OK",
				Padding = new Thickness(12, 4, 12, 4),
				Margin = new Thickness(0, 0, 8, 0)
			};
			okButton.Click += OkButton_Click;

			_clearButton = new Button
			{
				Content = "Clear All",
				Padding = new Thickness(12, 4, 12, 4)
			};
			_clearButton.Click += ClearButton_Click;

			Children.Add(_captureTextBox);
			Children.Add(okButton);
			Children.Add(_clearButton);

			UpdateDisplay();
		}

		private static void OnHotkeysChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (d is MultiHotkeyControl control)
			{
				control.ParseHotkeys((string)e.NewValue);
			}
		}

		private void ParseHotkeys(string value)
		{
			_hotkeys.Clear();
			if (!string.IsNullOrWhiteSpace(value))
			{
				_hotkeys.AddRange(value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
					.Select(s => s.Trim())
					.Where(s => !string.IsNullOrEmpty(s)));
			}
			UpdateDisplay();
		}

		private void UpdateDisplay()
		{
			if (_isCapturing)
			{
				_captureTextBox.Foreground = new SolidColorBrush(Colors.Blue);
				_captureTextBox.Text = _hotkeys.Count > 0 
					? string.Join(", ", _hotkeys) + " | Press key to add..." 
					: "Press a key combination...";
			}
			else
			{
				_captureTextBox.Foreground = new SolidColorBrush(Colors.Black);
				_captureTextBox.Text = _hotkeys.Count > 0 
					? string.Join(", ", _hotkeys) 
					: "Click to add hotkey";
			}
		}

		private void OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
		{
			_isCapturing = true;
			UpdateDisplay();
			_captureTextBox.Focus();
			e.Handled = true;
		}

		private void OnPreviewKeyDown(object sender, KeyEventArgs e)
		{
			e.Handled = true;

			if (!_isCapturing)
				return;

			// Get the actual key - some keys like F13-F24 might come through as System keys
			var key = e.Key == Key.System ? e.SystemKey : e.Key;
			
			// If key is still None or DeadCharProcessed, try to get it from the actual key value
			if (key == Key.None || key == Key.DeadCharProcessed)
			{
				key = e.Key;
			}

			// ESC exits capture mode
			if (key == Key.Escape)
			{
				_isCapturing = false;
				UpdateDisplay();
				return;
			}

			// Delete/Backspace removes the last hotkey
			if (key == Key.Delete || key == Key.Back)
			{
				if (_hotkeys.Count > 0)
				{
					_hotkeys.RemoveAt(_hotkeys.Count - 1);
					Hotkeys = string.Join(", ", _hotkeys);
				}
				return;
			}

			// Ignore modifier-only presses
			if (key == Key.LeftCtrl || key == Key.RightCtrl ||
				key == Key.LeftAlt || key == Key.RightAlt ||
				key == Key.LeftShift || key == Key.RightShift ||
				key == Key.LWin || key == Key.RWin)
			{
				return;
			}

			// Build the hotkey string
			var modifiers = Keyboard.Modifiers;
			var hotkeyParts = new List<string>();

			if (modifiers.HasFlag(ModifierKeys.Control))
				hotkeyParts.Add("Control");
			if (modifiers.HasFlag(ModifierKeys.Alt))
				hotkeyParts.Add("Alt");
			if (modifiers.HasFlag(ModifierKeys.Shift))
				hotkeyParts.Add("Shift");

			hotkeyParts.Add(ConvertKeyToString(key));

			var hotkeyString = string.Join("+", hotkeyParts);

			// Add to list if not already present
			if (!_hotkeys.Contains(hotkeyString))
			{
				_hotkeys.Add(hotkeyString);
				Hotkeys = string.Join(", ", _hotkeys);
			}

			UpdateDisplay();
		}

		private void OnLostFocus(object sender, RoutedEventArgs e)
		{
			_isCapturing = false;
			UpdateDisplay();
		}

		private void OkButton_Click(object sender, RoutedEventArgs e)
		{
			_isCapturing = false;
			UpdateDisplay();
		}

		private void ClearButton_Click(object sender, RoutedEventArgs e)
		{
			_hotkeys.Clear();
			Hotkeys = string.Empty;
			_isCapturing = false;
			UpdateDisplay();
		}

		private string ConvertKeyToString(Key key)
		{
			// Handle F1-F24 explicitly
			if (key >= Key.F1 && key <= Key.F24)
				return key.ToString();

			// Handle number pad
			if (key >= Key.NumPad0 && key <= Key.NumPad9)
				return "NumPad" + (key - Key.NumPad0);

			// Handle regular numbers
			if (key >= Key.D0 && key <= Key.D9)
				return (key - Key.D0).ToString();

			// Handle special keys
			switch (key)
			{
				case Key.OemPlus: return "Plus";
				case Key.OemMinus: return "Minus";
				case Key.Enter: return "Enter";
				case Key.Space: return "Space";
				case Key.Tab: return "Tab";
				case Key.PageUp: return "PageUp";
				case Key.PageDown: return "PageDown";
				case Key.Home: return "Home";
				case Key.End: return "End";
				case Key.Insert: return "Insert";
				default: return key.ToString();
			}
		}
	}
}
