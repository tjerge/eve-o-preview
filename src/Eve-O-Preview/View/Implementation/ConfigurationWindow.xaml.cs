using System;
using System.Windows;
using EveOPreview.Configuration;
using EveOPreview.Services;

namespace EveOPreview.View
{
	/// <summary>
	/// Interaction logic for ConfigurationWindow.xaml
	/// </summary>
	public partial class ConfigurationWindow : Window
	{
		private readonly ConfigurationViewModel _viewModel;
		private readonly IConfigurationStorage _configurationStorage;
		private readonly Action _onConfigurationSaved;
		private bool _isApplied = false;

		public ConfigurationWindow(IThumbnailConfiguration config, IThumbnailManager thumbnailManager = null, IConfigurationStorage configurationStorage = null, Action onConfigurationSaved = null)
		{
			InitializeComponent();

			_configurationStorage = configurationStorage;
			_onConfigurationSaved = onConfigurationSaved;

			_viewModel = new ConfigurationViewModel(
				config,
				thumbnailManager,
				onSave: OnSave,
				onCancel: OnCancel,
				onApply: OnApply
			);

			DataContext = _viewModel;
		}

		public bool IsApplied => _isApplied;

		private void OnSave()
		{
			_viewModel.SaveConfigurationValues();
			_configurationStorage?.Save();
			_onConfigurationSaved?.Invoke();
			_isApplied = true;
			DialogResult = true;
			Close();
		}

		private void OnCancel()
		{
			DialogResult = false;
			Close();
		}

		private void OnApply()
		{
			_viewModel.SaveConfigurationValues();
			_configurationStorage?.Save();
			_onConfigurationSaved?.Invoke();
			_isApplied = true;
		}

		private void RemovePerClientHotkey_Click(object sender, RoutedEventArgs e)
		{
			if (sender is System.Windows.Controls.Button button && button.Tag is PerClientHotkeyItem item)
			{
				_viewModel.RemovePerClientHotkey(item);
			}
		}

		private void RemovePerClientHighlightColor_Click(object sender, RoutedEventArgs e)
		{
			if (sender is System.Windows.Controls.Button button && button.Tag is PerClientHighlightColorItem item)
			{
				_viewModel.RemovePerClientHighlightColor(item);
			}
		}

		private void SelectPerClientHighlightColor_Click(object sender, RoutedEventArgs e)
		{
			if (sender is System.Windows.Controls.Button button && button.Tag is PerClientHighlightColorItem item)
			{
				var dialog = new System.Windows.Forms.ColorDialog();
				dialog.Color = System.Drawing.Color.FromArgb(item.HighlightColor.A, item.HighlightColor.R, item.HighlightColor.G, item.HighlightColor.B);
				
				if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
				{
					item.HighlightColor = System.Windows.Media.Color.FromArgb(dialog.Color.A, dialog.Color.R, dialog.Color.G, dialog.Color.B);
				}
			}
		}

		private void SetAspectRatio16_9_Click(object sender, RoutedEventArgs e)
		{
			int width = _viewModel.ThumbnailWidth;
			int height = (int)(width / 16.0 * 9.0);
			_viewModel.ThumbnailHeight = height;
		}

		private void SetAspectRatio21_9_Click(object sender, RoutedEventArgs e)
		{
			int width = _viewModel.ThumbnailWidth;
			int height = (int)(width / 21.0 * 9.0);
			_viewModel.ThumbnailHeight = height;
		}

		private void SetAspectRatio4_3_Click(object sender, RoutedEventArgs e)
		{
			int width = _viewModel.ThumbnailWidth;
			int height = (int)(width / 4.0 * 3.0);
			_viewModel.ThumbnailHeight = height;
		}
	}
}
