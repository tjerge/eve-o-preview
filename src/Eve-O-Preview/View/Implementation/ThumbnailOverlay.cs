using System;
using System.Windows.Forms;
using EveOPreview.Configuration;
using EveOPreview.Services;

namespace EveOPreview.View
{
	public partial class ThumbnailOverlay : Form
	{
		#region Private fields
		private readonly Action<object, MouseEventArgs> _areaClickAction;
		#endregion

		public ThumbnailOverlay(Form owner, Action<object, MouseEventArgs> areaClickAction)
		{
			this.Owner = owner;
			this._areaClickAction = areaClickAction;

			InitializeComponent();
		}

		private void OverlayArea_Click(object sender, MouseEventArgs e)
		{
			this._areaClickAction(this, e);
		}

	public void SetOverlayLabel(string characterName, string systemName, bool showSystemName, System.Drawing.Color characterColor, System.Drawing.Color systemColor, ZoomAnchor anchor)
	{
		this.OverlayLabel.Clear();
		
		// Set alignment for the entire control
		System.Windows.Forms.HorizontalAlignment alignment = GetAlignmentFromAnchor(anchor);
		
		// Add character name
		this.OverlayLabel.SelectionStart = 0;
		this.OverlayLabel.SelectionAlignment = alignment;
		this.OverlayLabel.SelectionColor = characterColor;
		this.OverlayLabel.AppendText(characterName);
		
		// Add system name if enabled
		if (showSystemName && !string.IsNullOrEmpty(systemName))
		{
			this.OverlayLabel.AppendText("\n");
			this.OverlayLabel.SelectionAlignment = alignment;
			this.OverlayLabel.SelectionColor = systemColor;
			this.OverlayLabel.AppendText(systemName);
		}
		
		// Reset selection
		this.OverlayLabel.SelectionStart = 0;
		this.OverlayLabel.SelectionLength = 0;
	}
	
	private System.Windows.Forms.HorizontalAlignment GetAlignmentFromAnchor(ZoomAnchor anchor)
	{
		switch (anchor)
		{
			case ZoomAnchor.NW:
			case ZoomAnchor.W:
			case ZoomAnchor.SW:
				return System.Windows.Forms.HorizontalAlignment.Left;
			case ZoomAnchor.N:
			case ZoomAnchor.C:
			case ZoomAnchor.S:
				return System.Windows.Forms.HorizontalAlignment.Center;
			case ZoomAnchor.NE:
			case ZoomAnchor.E:
			case ZoomAnchor.SE:
				return System.Windows.Forms.HorizontalAlignment.Right;
			default:
				return System.Windows.Forms.HorizontalAlignment.Left;
		}
	}		public void SetPropertiesOverlayLabel(int size, System.Drawing.Color c, ZoomAnchor anchor)
		{
			if (this.OverlayLabel.Font.Size != size)
			{
				this.OverlayLabel.Font = new System.Drawing.Font(this.OverlayLabel.Font.FontFamily, size);
			}

			int margin = 5;

			switch (anchor)
			{
				case ZoomAnchor.NW:
					this.OverlayLabel.Left = margin;
					this.OverlayLabel.Top = margin;
					break;
                case ZoomAnchor.N:
                    this.OverlayLabel.Left = (this.Width / 2) - (this.OverlayLabel.Width / 2);
                    this.OverlayLabel.Top = margin;
                    break;
                case ZoomAnchor.NE:
                    this.OverlayLabel.Left = this.Width - this.OverlayLabel.Width - margin;
                    this.OverlayLabel.Top = margin;
                    break;
                case ZoomAnchor.W:
                    this.OverlayLabel.Left = margin;
                    this.OverlayLabel.Top = (this.Height / 2) - (this.OverlayLabel.Height / 2);
                    break;
                case ZoomAnchor.C:
                    this.OverlayLabel.Left = (this.Width / 2) - (this.OverlayLabel.Width / 2);
                    this.OverlayLabel.Top = (this.Height / 2) - (this.OverlayLabel.Height / 2);
                    break;
                case ZoomAnchor.E:
                    this.OverlayLabel.Left = this.Width - this.OverlayLabel.Width - margin;
                    this.OverlayLabel.Top = (this.Height / 2) - (this.OverlayLabel.Height / 2);
                    break;
                case ZoomAnchor.SW:
                    this.OverlayLabel.Left = margin;
                    this.OverlayLabel.Top = this.Height - this.OverlayLabel.Height - margin;
                    break;
                case ZoomAnchor.S:
                    this.OverlayLabel.Left = (this.Width / 2) - (this.OverlayLabel.Width / 2);
                    this.OverlayLabel.Top = this.Height - this.OverlayLabel.Height - margin;
                    break;
                case ZoomAnchor.SE:
                    this.OverlayLabel.Left = this.Width - this.OverlayLabel.Width - margin;
                    this.OverlayLabel.Top = this.Height - this.OverlayLabel.Height - margin;
                    break;
            }
		}

		public void EnableOverlayLabel(bool enable)
		{
			this.OverlayLabel.Visible = enable;
		}

		protected override CreateParams CreateParams
		{
			get
			{
				var Params = base.CreateParams;
				Params.ExStyle |= (int)InteropConstants.WS_EX_TOOLWINDOW;
				return Params;
			}
		}
	}
}
