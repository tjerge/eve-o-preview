using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Media;
using EveOPreview.Configuration;
using EveOPreview.Services;

namespace EveOPreview.View
{
	public class ConfigurationViewModel : INotifyPropertyChanged
	{
		private readonly IThumbnailConfiguration _config;
		private readonly IThumbnailManager _thumbnailManager;
		private readonly Action _onSave;
		private readonly Action _onCancel;
		private readonly Action _onApply;

		public ConfigurationViewModel(IThumbnailConfiguration config, IThumbnailManager thumbnailManager, Action onSave, Action onCancel, Action onApply)
		{
			this._config = config;
			this._thumbnailManager = thumbnailManager;
			this._onSave = onSave;
			this._onCancel = onCancel;
			this._onApply = onApply;

			// Initialize commands
			SaveCommand = new RelayCommand(_ => _onSave());
			CancelCommand = new RelayCommand(_ => _onCancel());
			ApplyCommand = new RelayCommand(_ => _onApply());
			SelectOverlayLabelColorCommand = new RelayCommand(_ => SelectOverlayLabelColor());
			SelectSystemNameColorCommand = new RelayCommand(_ => SelectSystemNameColor());
			SelectHighlightColorCommand = new RelayCommand(_ => SelectHighlightColor());
			OpenDocumentationCommand = new RelayCommand(_ => OpenDocumentation());
			AddPerClientHotkeyCommand = new RelayCommand(_ => AddPerClientHotkey());
			PopulateFromOpenClientsCommand = new RelayCommand(_ => PopulateFromOpenClients(), _ => _thumbnailManager != null);
			AddPerClientHighlightColorCommand = new RelayCommand(_ => AddPerClientHighlightColor());
			PopulateHighlightColorsFromOpenClientsCommand = new RelayCommand(_ => PopulateHighlightColorsFromOpenClients(), _ => _thumbnailManager != null);
			BrowseChatlogPathCommand = new RelayCommand(_ => BrowseChatlogPath());

			// Initialize animation styles
			AnimationStyles = Enum.GetValues(typeof(AnimationStyle)).Cast<AnimationStyle>().ToList();
			
			// Initialize per-client hotkeys collection
			PerClientHotkeys = new ObservableCollection<PerClientHotkeyItem>();

			// Initialize per-client highlight colors collection
			PerClientHighlightColors = new ObservableCollection<PerClientHighlightColorItem>();

			LoadConfigurationValues();
		}

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		#region Commands
		public ICommand SaveCommand { get; }
		public ICommand CancelCommand { get; }
		public ICommand ApplyCommand { get; }
		public ICommand SelectOverlayLabelColorCommand { get; }
		public ICommand SelectSystemNameColorCommand { get; }
		public ICommand SelectHighlightColorCommand { get; }
		public ICommand OpenDocumentationCommand { get; }
		public ICommand AddPerClientHotkeyCommand { get; }
		public ICommand PopulateFromOpenClientsCommand { get; }
		public ICommand AddPerClientHighlightColorCommand { get; }
		public ICommand PopulateHighlightColorsFromOpenClientsCommand { get; }
		public ICommand BrowseChatlogPathCommand { get; }
		#endregion

		#region Properties - About
		public string VersionText => System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "Unknown";
		#endregion

		#region Properties - General
		private bool _minimizeToTray;
		public bool MinimizeToTray
		{
			get => _minimizeToTray;
			set { _minimizeToTray = value; OnPropertyChanged(); }
		}

		private bool _enableClientLayoutTracking;
		public bool EnableClientLayoutTracking
		{
			get => _enableClientLayoutTracking;
			set { _enableClientLayoutTracking = value; OnPropertyChanged(); }
		}

		private bool _hideActiveClientThumbnail;
		public bool HideActiveClientThumbnail
		{
			get => _hideActiveClientThumbnail;
			set { _hideActiveClientThumbnail = value; OnPropertyChanged(); }
		}

		private bool _minimizeInactiveClients;
		public bool MinimizeInactiveClients
		{
			get => _minimizeInactiveClients;
			set { _minimizeInactiveClients = value; OnPropertyChanged(); }
		}

		private bool _showThumbnailsAlwaysOnTop;
		public bool ShowThumbnailsAlwaysOnTop
		{
			get => _showThumbnailsAlwaysOnTop;
			set { _showThumbnailsAlwaysOnTop = value; OnPropertyChanged(); }
		}

		private bool _hideThumbnailsOnLostFocus;
		public bool HideThumbnailsOnLostFocus
		{
			get => _hideThumbnailsOnLostFocus;
			set { _hideThumbnailsOnLostFocus = value; OnPropertyChanged(); }
		}

		private bool _enablePerClientThumbnailLayouts;
		public bool EnablePerClientThumbnailLayouts
		{
			get => _enablePerClientThumbnailLayouts;
			set { _enablePerClientThumbnailLayouts = value; OnPropertyChanged(); }
		}

		private bool _hideLoginClientThumbnail;
		public bool HideLoginClientThumbnail
		{
			get => _hideLoginClientThumbnail;
			set { _hideLoginClientThumbnail = value; OnPropertyChanged(); }
		}

		private int _hideThumbnailsDelay;
		public int HideThumbnailsDelay
		{
			get => _hideThumbnailsDelay;
			set { _hideThumbnailsDelay = value; OnPropertyChanged(); }
		}

		private int _thumbnailRefreshPeriod;
		public int ThumbnailRefreshPeriod
		{
			get => _thumbnailRefreshPeriod;
			set { _thumbnailRefreshPeriod = value; OnPropertyChanged(); }
		}

		private int _thumbnailResizeTimeoutPeriod;
		public int ThumbnailResizeTimeoutPeriod
		{
			get => _thumbnailResizeTimeoutPeriod;
			set { _thumbnailResizeTimeoutPeriod = value; OnPropertyChanged(); }
		}

		private bool _enableWineCompatibilityMode;
		public bool EnableWineCompatibilityMode
		{
			get => _enableWineCompatibilityMode;
			set { _enableWineCompatibilityMode = value; OnPropertyChanged(); }
		}

		private string _chatlogPath;
		public string ChatlogPath
		{
			get => _chatlogPath;
			set { _chatlogPath = value; OnPropertyChanged(); }
		}

		public List<AnimationStyle> AnimationStyles { get; }

		private AnimationStyle _selectedAnimationStyle;
		public AnimationStyle SelectedAnimationStyle
		{
			get => _selectedAnimationStyle;
			set { _selectedAnimationStyle = value; OnPropertyChanged(); }
		}
		#endregion

		#region Properties - Thumbnail
		private int _thumbnailWidth;
		public int ThumbnailWidth
		{
			get => _thumbnailWidth;
			set { _thumbnailWidth = value; OnPropertyChanged(); }
		}

		private int _thumbnailHeight;
		public int ThumbnailHeight
		{
			get => _thumbnailHeight;
			set { _thumbnailHeight = value; OnPropertyChanged(); }
		}

		private int _thumbnailMinimumWidth;
		public int ThumbnailMinimumWidth
		{
			get => _thumbnailMinimumWidth;
			set { _thumbnailMinimumWidth = value; OnPropertyChanged(); }
		}

		private int _thumbnailMinimumHeight;
		public int ThumbnailMinimumHeight
		{
			get => _thumbnailMinimumHeight;
			set { _thumbnailMinimumHeight = value; OnPropertyChanged(); }
		}

		private int _thumbnailMaximumWidth;
		public int ThumbnailMaximumWidth
		{
			get => _thumbnailMaximumWidth;
			set { _thumbnailMaximumWidth = value; OnPropertyChanged(); }
		}

		private int _thumbnailMaximumHeight;
		public int ThumbnailMaximumHeight
		{
			get => _thumbnailMaximumHeight;
			set { _thumbnailMaximumHeight = value; OnPropertyChanged(); }
		}

		private bool _enableThumbnailSnap;
		public bool EnableThumbnailSnap
		{
			get => _enableThumbnailSnap;
			set { _enableThumbnailSnap = value; OnPropertyChanged(); }
		}

		private double _thumbnailOpacity;
		public double ThumbnailOpacity
		{
			get => _thumbnailOpacity;
			set { _thumbnailOpacity = value; OnPropertyChanged(); }
		}

		private bool _showThumbnailOverlays;
		public bool ShowThumbnailOverlays
		{
			get => _showThumbnailOverlays;
			set { _showThumbnailOverlays = value; OnPropertyChanged(); }
		}

		private bool _showThumbnailFrames;
		public bool ShowThumbnailFrames
		{
			get => _showThumbnailFrames;
			set { _showThumbnailFrames = value; OnPropertyChanged(); }
		}

		private bool _lockThumbnailLocation;
		public bool LockThumbnailLocation
		{
			get => _lockThumbnailLocation;
			set { _lockThumbnailLocation = value; OnPropertyChanged(); }
		}

		private bool _thumbnailSnapToGrid;
		public bool ThumbnailSnapToGrid
		{
			get => _thumbnailSnapToGrid;
			set { _thumbnailSnapToGrid = value; OnPropertyChanged(); }
		}

		private int _snapToGridSizeX;
		public int SnapToGridSizeX
		{
			get => _snapToGridSizeX;
			set { _snapToGridSizeX = value; OnPropertyChanged(); }
		}

		private int _snapToGridSizeY;
		public int SnapToGridSizeY
		{
			get => _snapToGridSizeY;
			set { _snapToGridSizeY = value; OnPropertyChanged(); }
		}
		#endregion

		#region Properties - Zoom
		private bool _enableThumbnailZoom;
		public bool EnableThumbnailZoom
		{
			get => _enableThumbnailZoom;
			set { _enableThumbnailZoom = value; OnPropertyChanged(); }
		}

		private int _thumbnailZoomFactor;
		public int ThumbnailZoomFactor
		{
			get => _thumbnailZoomFactor;
			set { _thumbnailZoomFactor = value; OnPropertyChanged(); }
		}

		// Zoom Anchor properties
		public bool IsZoomAnchorNW
		{
			get => _zoomAnchor == ZoomAnchor.NW;
			set { if (value) SetZoomAnchor(ZoomAnchor.NW); }
		}
		public bool IsZoomAnchorN
		{
			get => _zoomAnchor == ZoomAnchor.N;
			set { if (value) SetZoomAnchor(ZoomAnchor.N); }
		}
		public bool IsZoomAnchorNE
		{
			get => _zoomAnchor == ZoomAnchor.NE;
			set { if (value) SetZoomAnchor(ZoomAnchor.NE); }
		}
		public bool IsZoomAnchorW
		{
			get => _zoomAnchor == ZoomAnchor.W;
			set { if (value) SetZoomAnchor(ZoomAnchor.W); }
		}
		public bool IsZoomAnchorC
		{
			get => _zoomAnchor == ZoomAnchor.C;
			set { if (value) SetZoomAnchor(ZoomAnchor.C); }
		}
		public bool IsZoomAnchorE
		{
			get => _zoomAnchor == ZoomAnchor.E;
			set { if (value) SetZoomAnchor(ZoomAnchor.E); }
		}
		public bool IsZoomAnchorSW
		{
			get => _zoomAnchor == ZoomAnchor.SW;
			set { if (value) SetZoomAnchor(ZoomAnchor.SW); }
		}
		public bool IsZoomAnchorS
		{
			get => _zoomAnchor == ZoomAnchor.S;
			set { if (value) SetZoomAnchor(ZoomAnchor.S); }
		}
		public bool IsZoomAnchorSE
		{
			get => _zoomAnchor == ZoomAnchor.SE;
			set { if (value) SetZoomAnchor(ZoomAnchor.SE); }
		}

		private ZoomAnchor _zoomAnchor;
		private void SetZoomAnchor(ZoomAnchor anchor)
		{
			_zoomAnchor = anchor;
			OnPropertyChanged(nameof(IsZoomAnchorNW));
			OnPropertyChanged(nameof(IsZoomAnchorN));
			OnPropertyChanged(nameof(IsZoomAnchorNE));
			OnPropertyChanged(nameof(IsZoomAnchorW));
			OnPropertyChanged(nameof(IsZoomAnchorC));
			OnPropertyChanged(nameof(IsZoomAnchorE));
			OnPropertyChanged(nameof(IsZoomAnchorSW));
			OnPropertyChanged(nameof(IsZoomAnchorS));
			OnPropertyChanged(nameof(IsZoomAnchorSE));
		}
		#endregion

		#region Properties - Overlay
		private bool _enableSystemNameDisplay;
		public bool EnableSystemNameDisplay
		{
			get => _enableSystemNameDisplay;
			set { _enableSystemNameDisplay = value; OnPropertyChanged(); }
		}

		private Color _overlayLabelColor;
		public Color OverlayLabelColor
		{
			get => _overlayLabelColor;
			set { _overlayLabelColor = value; OnPropertyChanged(); }
		}

		private Color _systemNameColor;
		public Color SystemNameColor
		{
			get => _systemNameColor;
			set { _systemNameColor = value; OnPropertyChanged(); }
		}

		private int _overlayLabelSize;
		public int OverlayLabelSize
		{
			get => _overlayLabelSize;
			set { _overlayLabelSize = value; OnPropertyChanged(); }
		}

		// Overlay Anchor properties
		public bool IsOverlayAnchorNW
		{
			get => _overlayAnchor == ZoomAnchor.NW;
			set { if (value) SetOverlayAnchor(ZoomAnchor.NW); }
		}
		public bool IsOverlayAnchorN
		{
			get => _overlayAnchor == ZoomAnchor.N;
			set { if (value) SetOverlayAnchor(ZoomAnchor.N); }
		}
		public bool IsOverlayAnchorNE
		{
			get => _overlayAnchor == ZoomAnchor.NE;
			set { if (value) SetOverlayAnchor(ZoomAnchor.NE); }
		}
		public bool IsOverlayAnchorW
		{
			get => _overlayAnchor == ZoomAnchor.W;
			set { if (value) SetOverlayAnchor(ZoomAnchor.W); }
		}
		public bool IsOverlayAnchorC
		{
			get => _overlayAnchor == ZoomAnchor.C;
			set { if (value) SetOverlayAnchor(ZoomAnchor.C); }
		}
		public bool IsOverlayAnchorE
		{
			get => _overlayAnchor == ZoomAnchor.E;
			set { if (value) SetOverlayAnchor(ZoomAnchor.E); }
		}
		public bool IsOverlayAnchorSW
		{
			get => _overlayAnchor == ZoomAnchor.SW;
			set { if (value) SetOverlayAnchor(ZoomAnchor.SW); }
		}
		public bool IsOverlayAnchorS
		{
			get => _overlayAnchor == ZoomAnchor.S;
			set { if (value) SetOverlayAnchor(ZoomAnchor.S); }
		}
		public bool IsOverlayAnchorSE
		{
			get => _overlayAnchor == ZoomAnchor.SE;
			set { if (value) SetOverlayAnchor(ZoomAnchor.SE); }
		}

		private ZoomAnchor _overlayAnchor;
		private void SetOverlayAnchor(ZoomAnchor anchor)
		{
			_overlayAnchor = anchor;
			OnPropertyChanged(nameof(IsOverlayAnchorNW));
			OnPropertyChanged(nameof(IsOverlayAnchorN));
			OnPropertyChanged(nameof(IsOverlayAnchorNE));
			OnPropertyChanged(nameof(IsOverlayAnchorW));
			OnPropertyChanged(nameof(IsOverlayAnchorC));
			OnPropertyChanged(nameof(IsOverlayAnchorE));
			OnPropertyChanged(nameof(IsOverlayAnchorSW));
			OnPropertyChanged(nameof(IsOverlayAnchorS));
			OnPropertyChanged(nameof(IsOverlayAnchorSE));
		}
		#endregion

		#region Properties - Highlight
		private bool _enableActiveClientHighlight;
		public bool EnableActiveClientHighlight
		{
			get => _enableActiveClientHighlight;
			set { _enableActiveClientHighlight = value; OnPropertyChanged(); }
		}

		private Color _activeClientHighlightColor;
		public Color ActiveClientHighlightColor
		{
			get => _activeClientHighlightColor;
			set { _activeClientHighlightColor = value; OnPropertyChanged(); }
		}

		private int _activeClientHighlightThickness;
		public int ActiveClientHighlightThickness
		{
			get => _activeClientHighlightThickness;
			set { _activeClientHighlightThickness = value; OnPropertyChanged(); }
		}
		#endregion

		#region Properties - Hotkeys
		private string _cycleGroup1ForwardHotkeys;
		public string CycleGroup1ForwardHotkeys
		{
			get => _cycleGroup1ForwardHotkeys;
			set { _cycleGroup1ForwardHotkeys = value; OnPropertyChanged(); }
		}

		private string _cycleGroup1BackwardHotkeys;
		public string CycleGroup1BackwardHotkeys
		{
			get => _cycleGroup1BackwardHotkeys;
			set { _cycleGroup1BackwardHotkeys = value; OnPropertyChanged(); }
		}

		private string _cycleGroup1ClientsOrder;
		public string CycleGroup1ClientsOrder
		{
			get => _cycleGroup1ClientsOrder;
			set { _cycleGroup1ClientsOrder = value; OnPropertyChanged(); }
		}

		private string _cycleGroup2ForwardHotkeys;
		public string CycleGroup2ForwardHotkeys
		{
			get => _cycleGroup2ForwardHotkeys;
			set { _cycleGroup2ForwardHotkeys = value; OnPropertyChanged(); }
		}

		private string _cycleGroup2BackwardHotkeys;
		public string CycleGroup2BackwardHotkeys
		{
			get => _cycleGroup2BackwardHotkeys;
			set { _cycleGroup2BackwardHotkeys = value; OnPropertyChanged(); }
		}

		private string _cycleGroup2ClientsOrder;
		public string CycleGroup2ClientsOrder
		{
			get => _cycleGroup2ClientsOrder;
			set { _cycleGroup2ClientsOrder = value; OnPropertyChanged(); }
		}

		private string _cycleGroup3ForwardHotkeys;
		public string CycleGroup3ForwardHotkeys
		{
			get => _cycleGroup3ForwardHotkeys;
			set { _cycleGroup3ForwardHotkeys = value; OnPropertyChanged(); }
		}

		private string _cycleGroup3BackwardHotkeys;
		public string CycleGroup3BackwardHotkeys
		{
			get => _cycleGroup3BackwardHotkeys;
			set { _cycleGroup3BackwardHotkeys = value; OnPropertyChanged(); }
		}

		private string _cycleGroup3ClientsOrder;
		public string CycleGroup3ClientsOrder
		{
			get => _cycleGroup3ClientsOrder;
			set { _cycleGroup3ClientsOrder = value; OnPropertyChanged(); }
		}

		private string _cycleGroup4ForwardHotkeys;
		public string CycleGroup4ForwardHotkeys
		{
			get => _cycleGroup4ForwardHotkeys;
			set { _cycleGroup4ForwardHotkeys = value; OnPropertyChanged(); }
		}

		private string _cycleGroup4BackwardHotkeys;
		public string CycleGroup4BackwardHotkeys
		{
			get => _cycleGroup4BackwardHotkeys;
			set { _cycleGroup4BackwardHotkeys = value; OnPropertyChanged(); }
		}

		private string _cycleGroup4ClientsOrder;
		public string CycleGroup4ClientsOrder
		{
			get => _cycleGroup4ClientsOrder;
			set { _cycleGroup4ClientsOrder = value; OnPropertyChanged(); }
		}

		private string _cycleGroup5ForwardHotkeys;
		public string CycleGroup5ForwardHotkeys
		{
			get => _cycleGroup5ForwardHotkeys;
			set { _cycleGroup5ForwardHotkeys = value; OnPropertyChanged(); }
		}

		private string _cycleGroup5BackwardHotkeys;
		public string CycleGroup5BackwardHotkeys
		{
			get => _cycleGroup5BackwardHotkeys;
			set { _cycleGroup5BackwardHotkeys = value; OnPropertyChanged(); }
		}

		private string _cycleGroup5ClientsOrder;
		public string CycleGroup5ClientsOrder
		{
			get => _cycleGroup5ClientsOrder;
			set { _cycleGroup5ClientsOrder = value; OnPropertyChanged(); }
		}

		// Per-Client Hotkeys
		public ObservableCollection<PerClientHotkeyItem> PerClientHotkeys { get; set; }

		// Per-Client Highlight Colors
		public ObservableCollection<PerClientHighlightColorItem> PerClientHighlightColors { get; set; }
		#endregion

		#region Methods
		private void LoadConfigurationValues()
		{
			// General
			MinimizeToTray = _config.MinimizeToTray;
			EnableClientLayoutTracking = _config.EnableClientLayoutTracking;
			HideActiveClientThumbnail = _config.HideActiveClientThumbnail;
			HideLoginClientThumbnail = _config.HideLoginClientThumbnail;
			MinimizeInactiveClients = _config.MinimizeInactiveClients;
			ShowThumbnailsAlwaysOnTop = _config.ShowThumbnailsAlwaysOnTop;
			HideThumbnailsOnLostFocus = _config.HideThumbnailsOnLostFocus;
			HideThumbnailsDelay = _config.HideThumbnailsDelay;
			EnablePerClientThumbnailLayouts = _config.EnablePerClientThumbnailLayouts;
			SelectedAnimationStyle = _config.WindowsAnimationStyle;
			ThumbnailRefreshPeriod = _config.ThumbnailRefreshPeriod;
			ThumbnailResizeTimeoutPeriod = _config.ThumbnailResizeTimeoutPeriod;
			EnableWineCompatibilityMode = _config.EnableWineCompatibilityMode;
			
			// Auto-detect chatlog path if empty
			string configChatlogPath = _config.ChatlogPath ?? "";
			if (string.IsNullOrEmpty(configChatlogPath))
			{
				configChatlogPath = AutoDetectChatlogPath();
			}
			ChatlogPath = configChatlogPath;

			// Thumbnail
			ThumbnailWidth = _config.ThumbnailSize.Width;
			ThumbnailHeight = _config.ThumbnailSize.Height;
			ThumbnailMinimumWidth = _config.ThumbnailMinimumSize.Width;
			ThumbnailMinimumHeight = _config.ThumbnailMinimumSize.Height;
			ThumbnailMaximumWidth = _config.ThumbnailMaximumSize.Width;
			ThumbnailMaximumHeight = _config.ThumbnailMaximumSize.Height;
			EnableThumbnailSnap = _config.EnableThumbnailSnap;
			ThumbnailOpacity = _config.ThumbnailOpacity;
			ShowThumbnailOverlays = _config.ShowThumbnailOverlays;
			ShowThumbnailFrames = _config.ShowThumbnailFrames;
			LockThumbnailLocation = _config.LockThumbnailLocation;
			ThumbnailSnapToGrid = _config.ThumbnailSnapToGrid;
			SnapToGridSizeX = _config.ThumbnailSnapToGridSizeX;
			SnapToGridSizeY = _config.ThumbnailSnapToGridSizeY;

			// Zoom
			EnableThumbnailZoom = _config.ThumbnailZoomEnabled;
			ThumbnailZoomFactor = _config.ThumbnailZoomFactor;
			SetZoomAnchor(_config.ThumbnailZoomAnchor);

			// Overlay
			EnableSystemNameDisplay = _config.EnableSystemNameDisplay;
			OverlayLabelColor = ColorFromDrawingColor(_config.OverlayLabelColor);
			SystemNameColor = ColorFromDrawingColor(_config.SystemNameColor);
			OverlayLabelSize = _config.OverlayLabelSize;
			SetOverlayAnchor(_config.OverlayLabelAnchor);

			// Highlight
			EnableActiveClientHighlight = _config.EnableActiveClientHighlight;
			ActiveClientHighlightColor = ColorFromDrawingColor(_config.ActiveClientHighlightColor);
			ActiveClientHighlightThickness = _config.ActiveClientHighlightThickness;

			// Hotkeys
			CycleGroup1ForwardHotkeys = string.Join(", ", _config.CycleGroup1ForwardHotkeys ?? new List<string>());
			CycleGroup1BackwardHotkeys = string.Join(", ", _config.CycleGroup1BackwardHotkeys ?? new List<string>());
			CycleGroup1ClientsOrder = FormatClientOrder(_config.CycleGroup1ClientsOrder);

			CycleGroup2ForwardHotkeys = string.Join(", ", _config.CycleGroup2ForwardHotkeys ?? new List<string>());
			CycleGroup2BackwardHotkeys = string.Join(", ", _config.CycleGroup2BackwardHotkeys ?? new List<string>());
			CycleGroup2ClientsOrder = FormatClientOrder(_config.CycleGroup2ClientsOrder);

			CycleGroup3ForwardHotkeys = string.Join(", ", _config.CycleGroup3ForwardHotkeys ?? new List<string>());
			CycleGroup3BackwardHotkeys = string.Join(", ", _config.CycleGroup3BackwardHotkeys ?? new List<string>());
			CycleGroup3ClientsOrder = FormatClientOrder(_config.CycleGroup3ClientsOrder);

			CycleGroup4ForwardHotkeys = string.Join(", ", _config.CycleGroup4ForwardHotkeys ?? new List<string>());
			CycleGroup4BackwardHotkeys = string.Join(", ", _config.CycleGroup4BackwardHotkeys ?? new List<string>());
			CycleGroup4ClientsOrder = FormatClientOrder(_config.CycleGroup4ClientsOrder);

			CycleGroup5ForwardHotkeys = string.Join(", ", _config.CycleGroup5ForwardHotkeys ?? new List<string>());
			CycleGroup5BackwardHotkeys = string.Join(", ", _config.CycleGroup5BackwardHotkeys ?? new List<string>());
			CycleGroup5ClientsOrder = FormatClientOrder(_config.CycleGroup5ClientsOrder);

			// Per-Client Hotkeys
			LoadPerClientHotkeys(_config.GetAllClientHotkeys());

			// Per-Client Highlight Colors
			LoadPerClientHighlightColors(_config.PerClientActiveClientHighlightColor);
		}

		public void SaveConfigurationValues()
		{
			// General
			_config.MinimizeToTray = MinimizeToTray;
			_config.EnableClientLayoutTracking = EnableClientLayoutTracking;
			_config.HideActiveClientThumbnail = HideActiveClientThumbnail;
			_config.HideLoginClientThumbnail = HideLoginClientThumbnail;
			_config.MinimizeInactiveClients = MinimizeInactiveClients;
			_config.ShowThumbnailsAlwaysOnTop = ShowThumbnailsAlwaysOnTop;
			_config.HideThumbnailsOnLostFocus = HideThumbnailsOnLostFocus;
			_config.HideThumbnailsDelay = HideThumbnailsDelay;
			_config.EnablePerClientThumbnailLayouts = EnablePerClientThumbnailLayouts;
			_config.WindowsAnimationStyle = SelectedAnimationStyle;
			_config.ThumbnailRefreshPeriod = ThumbnailRefreshPeriod;
			_config.ThumbnailResizeTimeoutPeriod = ThumbnailResizeTimeoutPeriod;
			_config.EnableWineCompatibilityMode = EnableWineCompatibilityMode;
			_config.ChatlogPath = ChatlogPath;

			// Thumbnail
			_config.ThumbnailSize = new System.Drawing.Size(ThumbnailWidth, ThumbnailHeight);
			_config.ThumbnailMinimumSize = new System.Drawing.Size(ThumbnailMinimumWidth, ThumbnailMinimumHeight);
			_config.ThumbnailMaximumSize = new System.Drawing.Size(ThumbnailMaximumWidth, ThumbnailMaximumHeight);
			_config.EnableThumbnailSnap = EnableThumbnailSnap;
			_config.ThumbnailOpacity = ThumbnailOpacity;
			_config.ShowThumbnailOverlays = ShowThumbnailOverlays;
			_config.ShowThumbnailFrames = ShowThumbnailFrames;
			_config.LockThumbnailLocation = LockThumbnailLocation;
			_config.ThumbnailSnapToGrid = ThumbnailSnapToGrid;
			_config.ThumbnailSnapToGridSizeX = SnapToGridSizeX;
			_config.ThumbnailSnapToGridSizeY = SnapToGridSizeY;

			// Zoom
			_config.ThumbnailZoomEnabled = EnableThumbnailZoom;
			_config.ThumbnailZoomFactor = ThumbnailZoomFactor;
			_config.ThumbnailZoomAnchor = _zoomAnchor;

			// Overlay
			_config.EnableSystemNameDisplay = EnableSystemNameDisplay;
			_config.OverlayLabelColor = ColorToDrawingColor(OverlayLabelColor);
			_config.SystemNameColor = ColorToDrawingColor(SystemNameColor);
			_config.OverlayLabelSize = OverlayLabelSize;
			_config.OverlayLabelAnchor = _overlayAnchor;

			// Highlight
			_config.EnableActiveClientHighlight = EnableActiveClientHighlight;
			_config.ActiveClientHighlightColor = ColorToDrawingColor(ActiveClientHighlightColor);
			_config.ActiveClientHighlightThickness = ActiveClientHighlightThickness;

			// Hotkeys
			_config.CycleGroup1ForwardHotkeys = ParseHotkeyList(CycleGroup1ForwardHotkeys);
			_config.CycleGroup1BackwardHotkeys = ParseHotkeyList(CycleGroup1BackwardHotkeys);
			_config.CycleGroup1ClientsOrder = ParseClientOrder(CycleGroup1ClientsOrder);

			_config.CycleGroup2ForwardHotkeys = ParseHotkeyList(CycleGroup2ForwardHotkeys);
			_config.CycleGroup2BackwardHotkeys = ParseHotkeyList(CycleGroup2BackwardHotkeys);
			_config.CycleGroup2ClientsOrder = ParseClientOrder(CycleGroup2ClientsOrder);

			_config.CycleGroup3ForwardHotkeys = ParseHotkeyList(CycleGroup3ForwardHotkeys);
			_config.CycleGroup3BackwardHotkeys = ParseHotkeyList(CycleGroup3BackwardHotkeys);
			_config.CycleGroup3ClientsOrder = ParseClientOrder(CycleGroup3ClientsOrder);

			_config.CycleGroup4ForwardHotkeys = ParseHotkeyList(CycleGroup4ForwardHotkeys);
			_config.CycleGroup4BackwardHotkeys = ParseHotkeyList(CycleGroup4BackwardHotkeys);
			_config.CycleGroup4ClientsOrder = ParseClientOrder(CycleGroup4ClientsOrder);

			_config.CycleGroup5ForwardHotkeys = ParseHotkeyList(CycleGroup5ForwardHotkeys);
			_config.CycleGroup5BackwardHotkeys = ParseHotkeyList(CycleGroup5BackwardHotkeys);
			_config.CycleGroup5ClientsOrder = ParseClientOrder(CycleGroup5ClientsOrder);

			// Per-Client Hotkeys
			_config.SetAllClientHotkeys(SavePerClientHotkeys());

			// Per-Client Highlight Colors
			_config.PerClientActiveClientHighlightColor = SavePerClientHighlightColors();
		}

	private void SelectOverlayLabelColor()
	{
		var color = ShowColorDialog(OverlayLabelColor);
		if (color.HasValue)
		{
			OverlayLabelColor = color.Value;
		}
	}

	private void SelectSystemNameColor()
	{
		var color = ShowColorDialog(SystemNameColor);
		if (color.HasValue)
		{
			SystemNameColor = color.Value;
		}
	}

	private void SelectHighlightColor()
	{
		var color = ShowColorDialog(ActiveClientHighlightColor);
		if (color.HasValue)
		{
			ActiveClientHighlightColor = color.Value;
		}
	}		private Color? ShowColorDialog(Color initialColor)
		{
			var dialog = new System.Windows.Forms.ColorDialog
			{
				Color = ColorToDrawingColor(initialColor),
				FullOpen = true
			};

			if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
			{
				return ColorFromDrawingColor(dialog.Color);
			}

			return null;
		}

		private void OpenDocumentation()
		{
			try
			{
				System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
				{
					FileName = "https://github.com/Phrynohyas/eve-o-preview",
					UseShellExecute = true
				});
			}
			catch
			{
				// Ignore errors when opening browser
			}
		}

		private void BrowseChatlogPath()
		{
			using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
			{
				dialog.Description = "Select EVE Online Chatlog Directory";
				dialog.ShowNewFolderButton = false;
				
				// Set initial directory if ChatlogPath is valid
				if (!string.IsNullOrEmpty(ChatlogPath) && System.IO.Directory.Exists(ChatlogPath))
				{
					dialog.SelectedPath = ChatlogPath;
				}
				else
				{
					// Try to set to the auto-detected path
					string autoPath = AutoDetectChatlogPath();
					if (!string.IsNullOrEmpty(autoPath))
					{
						dialog.SelectedPath = autoPath;
					}
				}
				
				if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
				{
					ChatlogPath = dialog.SelectedPath;
				}
			}
		}

		private string AutoDetectChatlogPath()
		{
			try
			{
				string myDocs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
				string eveChatlogPath = System.IO.Path.Combine(myDocs, "EVE", "logs", "Chatlogs");
				
				if (System.IO.Directory.Exists(eveChatlogPath))
				{
					return eveChatlogPath;
				}
			}
			catch
			{
				// Ignore errors during auto-detection
			}
			
			return string.Empty;
		}

		private static Color ColorFromDrawingColor(System.Drawing.Color color)
		{
			return Color.FromArgb(color.A, color.R, color.G, color.B);
		}

		private static System.Drawing.Color ColorToDrawingColor(Color color)
		{
			return System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B);
		}

		private string FormatClientOrder(Dictionary<string, int> clientOrder)
		{
			if (clientOrder == null || clientOrder.Count == 0)
				return string.Empty;

			// Display just the character names in order
			return string.Join(Environment.NewLine, 
				clientOrder.OrderBy(kvp => kvp.Value)
				.Select(kvp => kvp.Key));
		}

		private List<string> ParseHotkeyList(string input)
		{
			if (string.IsNullOrWhiteSpace(input))
				return new List<string>();

			return input.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
				.Select(s => s.Trim())
				.Where(s => !string.IsNullOrEmpty(s))
				.ToList();
		}

		private Dictionary<string, int> ParseClientOrder(string input)
		{
			var result = new Dictionary<string, int>();
			
			if (string.IsNullOrWhiteSpace(input))
				return result;

			// Each line is a character name, order is determined by line position (1-based)
			var lines = input.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
			int order = 1;
			foreach (var line in lines)
			{
				var clientName = line.Trim();
				if (!string.IsNullOrEmpty(clientName))
				{
					result[clientName] = order++;
				}
			}

			return result;
		}

		private void LoadPerClientHotkeys(Dictionary<string, string> hotkeys)
		{
			PerClientHotkeys.Clear();
			if (hotkeys != null)
			{
				foreach (var kvp in hotkeys)
				{
					PerClientHotkeys.Add(new PerClientHotkeyItem
					{
						ClientName = kvp.Key,
						Hotkey = kvp.Value
					});
				}
			}
		}

		private Dictionary<string, string> SavePerClientHotkeys()
		{
			var result = new Dictionary<string, string>();
			foreach (var item in PerClientHotkeys)
			{
				if (!string.IsNullOrWhiteSpace(item.ClientName) && !string.IsNullOrWhiteSpace(item.Hotkey))
				{
					result[item.ClientName] = item.Hotkey;
				}
			}
			return result;
		}

		private void AddPerClientHotkey()
		{
			PerClientHotkeys.Add(new PerClientHotkeyItem
			{
				ClientName = "",
				Hotkey = ""
			});
		}

		private void PopulateFromOpenClients()
		{
			if (_thumbnailManager == null)
				return;

			var clientTitles = _thumbnailManager.GetAllClientTitles().ToList();

			// Add any client titles that don't already exist in the list
			foreach (var title in clientTitles)
			{
				if (!PerClientHotkeys.Any(h => h.ClientName == title))
				{
					PerClientHotkeys.Add(new PerClientHotkeyItem
					{
						ClientName = title,
						Hotkey = ""
					});
				}
			}
		}

		public void RemovePerClientHotkey(PerClientHotkeyItem item)
		{
			PerClientHotkeys.Remove(item);
		}

		private void LoadPerClientHighlightColors(Dictionary<string, System.Drawing.Color> colors)
		{
			PerClientHighlightColors.Clear();
			if (colors != null)
			{
				foreach (var kvp in colors)
				{
					PerClientHighlightColors.Add(new PerClientHighlightColorItem
					{
						ClientName = kvp.Key,
						HighlightColor = ColorFromDrawingColor(kvp.Value)
					});
				}
			}
		}

		private Dictionary<string, System.Drawing.Color> SavePerClientHighlightColors()
		{
			var result = new Dictionary<string, System.Drawing.Color>();
			foreach (var item in PerClientHighlightColors)
			{
				if (!string.IsNullOrWhiteSpace(item.ClientName))
				{
					result[item.ClientName] = ColorToDrawingColor(item.HighlightColor);
				}
			}
			return result;
		}

		private void AddPerClientHighlightColor()
		{
			PerClientHighlightColors.Add(new PerClientHighlightColorItem
			{
				ClientName = "",
				HighlightColor = System.Windows.Media.Colors.Red
			});
		}

		private void PopulateHighlightColorsFromOpenClients()
		{
			if (_thumbnailManager == null)
				return;

			var clientTitles = _thumbnailManager.GetAllClientTitles().ToList();

			// Add any client titles that don't already exist in the list
			foreach (var title in clientTitles)
			{
				if (!PerClientHighlightColors.Any(h => h.ClientName == title))
				{
					PerClientHighlightColors.Add(new PerClientHighlightColorItem
					{
						ClientName = title,
						HighlightColor = System.Windows.Media.Colors.Red
					});
				}
			}
		}

		public void RemovePerClientHighlightColor(PerClientHighlightColorItem item)
		{
			PerClientHighlightColors.Remove(item);
		}

		private string FormatPerClientHotkeys(Dictionary<string, string> hotkeys)
		{
			if (hotkeys == null || hotkeys.Count == 0)
				return string.Empty;

			// Format as "ClientName: Hotkey" one per line
			return string.Join(Environment.NewLine, hotkeys.Select(kvp => $"{kvp.Key}: {kvp.Value}"));
		}

		private Dictionary<string, string> ParsePerClientHotkeys(string input)
		{
			var result = new Dictionary<string, string>();

			if (string.IsNullOrWhiteSpace(input))
				return result;

			// Parse "ClientName: Hotkey" format
			var lines = input.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
			foreach (var line in lines)
			{
				var parts = line.Split(new[] { ':' }, 2);
				if (parts.Length == 2)
				{
					var clientName = parts[0].Trim();
					var hotkey = parts[1].Trim();
					if (!string.IsNullOrEmpty(clientName) && !string.IsNullOrEmpty(hotkey))
					{
						result[clientName] = hotkey;
					}
				}
			}

			return result;
		}
		#endregion
	}

	// Simple RelayCommand implementation
	public class RelayCommand : ICommand
	{
		private readonly Action<object> _execute;
		private readonly Func<object, bool> _canExecute;

		public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
		{
			_execute = execute ?? throw new ArgumentNullException(nameof(execute));
			_canExecute = canExecute;
		}

		public event EventHandler CanExecuteChanged
		{
			add { System.Windows.Input.CommandManager.RequerySuggested += value; }
			remove { System.Windows.Input.CommandManager.RequerySuggested -= value; }
		}

		public bool CanExecute(object parameter)
		{
			return _canExecute == null || _canExecute(parameter);
		}

		public void Execute(object parameter)
		{
			_execute(parameter);
		}
	}

	// Simple data class for per-client hotkey items
	public class PerClientHotkeyItem : INotifyPropertyChanged
	{
		private string _clientName;
		private string _hotkey;

		public string ClientName
		{
			get => _clientName;
			set
			{
				_clientName = value;
				OnPropertyChanged(nameof(ClientName));
			}
		}

		public string Hotkey
		{
			get => _hotkey;
			set
			{
				_hotkey = value;
				OnPropertyChanged(nameof(Hotkey));
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged(string propertyName)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}

	public class PerClientHighlightColorItem : INotifyPropertyChanged
	{
		private string _clientName;
		private System.Windows.Media.Color _highlightColor;

		public string ClientName
		{
			get => _clientName;
			set
			{
				_clientName = value;
				OnPropertyChanged(nameof(ClientName));
			}
		}

		public System.Windows.Media.Color HighlightColor
		{
			get => _highlightColor;
			set
			{
				_highlightColor = value;
				OnPropertyChanged(nameof(HighlightColor));
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged(string propertyName)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}

