using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace EveOPreview.View
{
	/// <summary>
	/// A control for a single per-client hotkey entry with client name and hotkey capture
	/// </summary>
	public class PerClientHotkeyEntry : StackPanel
	{
		private TextBox _clientNameTextBox;
		private MultiHotkeyControl _hotkeyControl;
		private Button _removeButton;

		public event EventHandler RemoveRequested;

		public static readonly DependencyProperty ClientNameProperty =
			DependencyProperty.Register(
				nameof(ClientName),
				typeof(string),
				typeof(PerClientHotkeyEntry),
				new FrameworkPropertyMetadata(
					string.Empty,
					FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

		public static readonly DependencyProperty HotkeyProperty =
			DependencyProperty.Register(
				nameof(Hotkey),
				typeof(string),
				typeof(PerClientHotkeyEntry),
				new FrameworkPropertyMetadata(
					string.Empty,
					FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

		public string ClientName
		{
			get => (string)GetValue(ClientNameProperty);
			set => SetValue(ClientNameProperty, value);
		}

		public string Hotkey
		{
			get => (string)GetValue(HotkeyProperty);
			set => SetValue(HotkeyProperty, value);
		}

		public PerClientHotkeyEntry()
		{
			Orientation = Orientation.Horizontal;
			Margin = new Thickness(0, 4, 0, 4);

			// Client Name TextBox
			var clientLabel = new TextBlock
			{
				Text = "Client:",
				VerticalAlignment = VerticalAlignment.Center,
				Margin = new Thickness(0, 0, 8, 0),
				Width = 50
			};

			_clientNameTextBox = new TextBox
			{
				Width = 250,
				VerticalContentAlignment = VerticalAlignment.Center,
				Margin = new Thickness(0, 0, 8, 0),
				ToolTip = "Enter the exact client window name (e.g., 'EVE - Main Character')"
			};
			_clientNameTextBox.SetBinding(TextBox.TextProperty, new System.Windows.Data.Binding(nameof(ClientName))
			{
				Source = this,
				Mode = System.Windows.Data.BindingMode.TwoWay,
				UpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged
			});

			// Hotkey Label
			var hotkeyLabel = new TextBlock
			{
				Text = "Hotkey:",
				VerticalAlignment = VerticalAlignment.Center,
				Margin = new Thickness(0, 0, 8, 0),
				Width = 50
			};

			// Hotkey Control - but we want single hotkey only
			_hotkeyControl = new MultiHotkeyControl();
			_hotkeyControl.SetBinding(MultiHotkeyControl.HotkeysProperty, new System.Windows.Data.Binding(nameof(Hotkey))
			{
				Source = this,
				Mode = System.Windows.Data.BindingMode.TwoWay,
				UpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged
			});

			// Remove Button
			_removeButton = new Button
			{
				Content = "Remove",
				Padding = new Thickness(12, 4, 12, 4),
				Margin = new Thickness(8, 0, 0, 0)
			};
			_removeButton.Click += RemoveButton_Click;

			Children.Add(clientLabel);
			Children.Add(_clientNameTextBox);
			Children.Add(hotkeyLabel);
			Children.Add(_hotkeyControl);
			Children.Add(_removeButton);
		}

		private void RemoveButton_Click(object sender, RoutedEventArgs e)
		{
			RemoveRequested?.Invoke(this, EventArgs.Empty);
		}
	}
}
