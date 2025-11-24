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
		// Set character name label
		this.CharacterNameLabel.Text = characterName;
		this.CharacterNameLabel.ForeColor = characterColor;
		
		// Set system name label
		if (showSystemName && !string.IsNullOrEmpty(systemName))
		{
			this.SystemNameLabel.Text = systemName;
			this.SystemNameLabel.ForeColor = systemColor;
			this.SystemNameLabel.Visible = true;
		}
		else
		{
			this.SystemNameLabel.Visible = false;
		}
		
		// Update alignment based on anchor
		UpdateLabelAlignment(anchor);
	}
	
	private void UpdateLabelAlignment(ZoomAnchor anchor)
	{
		System.Drawing.ContentAlignment alignment = GetContentAlignmentFromAnchor(anchor);
		this.CharacterNameLabel.TextAlign = alignment;
		this.SystemNameLabel.TextAlign = alignment;
	}
	
	private System.Drawing.ContentAlignment GetContentAlignmentFromAnchor(ZoomAnchor anchor)
	{
		switch (anchor)
		{
			case ZoomAnchor.NW:
				return System.Drawing.ContentAlignment.TopLeft;
			case ZoomAnchor.N:
				return System.Drawing.ContentAlignment.TopCenter;
			case ZoomAnchor.NE:
				return System.Drawing.ContentAlignment.TopRight;
			case ZoomAnchor.W:
				return System.Drawing.ContentAlignment.MiddleLeft;
			case ZoomAnchor.C:
				return System.Drawing.ContentAlignment.MiddleCenter;
			case ZoomAnchor.E:
				return System.Drawing.ContentAlignment.MiddleRight;
			case ZoomAnchor.SW:
				return System.Drawing.ContentAlignment.BottomLeft;
			case ZoomAnchor.S:
				return System.Drawing.ContentAlignment.BottomCenter;
			case ZoomAnchor.SE:
				return System.Drawing.ContentAlignment.BottomRight;
			default:
				return System.Drawing.ContentAlignment.TopLeft;
		}
	}		public void SetPropertiesOverlayLabel(int size, System.Drawing.Color c, ZoomAnchor anchor)
	{
		// Update font size for both labels
		if (this.CharacterNameLabel.Font.Size != size)
		{
			this.CharacterNameLabel.Font = new System.Drawing.Font(this.CharacterNameLabel.Font.FontFamily, size, System.Drawing.FontStyle.Regular);
			this.SystemNameLabel.Font = new System.Drawing.Font(this.SystemNameLabel.Font.FontFamily, size, System.Drawing.FontStyle.Regular);
		}

		int margin = 5;
		int labelSpacing = 2; // Spacing between character name and system name

		// Calculate positions based on anchor
		switch (anchor)
		{
			case ZoomAnchor.NW:
				this.CharacterNameLabel.Left = margin;
				this.CharacterNameLabel.Top = margin;
				this.SystemNameLabel.Left = margin;
				this.SystemNameLabel.Top = this.CharacterNameLabel.Bottom + labelSpacing;
				break;
			case ZoomAnchor.N:
				this.CharacterNameLabel.Left = (this.Width / 2) - (this.CharacterNameLabel.Width / 2);
				this.CharacterNameLabel.Top = margin;
				this.SystemNameLabel.Left = (this.Width / 2) - (this.SystemNameLabel.Width / 2);
				this.SystemNameLabel.Top = this.CharacterNameLabel.Bottom + labelSpacing;
				break;
			case ZoomAnchor.NE:
				this.CharacterNameLabel.Left = this.Width - this.CharacterNameLabel.Width - margin;
				this.CharacterNameLabel.Top = margin;
				this.SystemNameLabel.Left = this.Width - this.SystemNameLabel.Width - margin;
				this.SystemNameLabel.Top = this.CharacterNameLabel.Bottom + labelSpacing;
				break;
			case ZoomAnchor.W:
				this.CharacterNameLabel.Left = margin;
				this.CharacterNameLabel.Top = (this.Height / 2) - (this.CharacterNameLabel.Height + this.SystemNameLabel.Height + labelSpacing) / 2;
				this.SystemNameLabel.Left = margin;
				this.SystemNameLabel.Top = this.CharacterNameLabel.Bottom + labelSpacing;
				break;
			case ZoomAnchor.C:
				this.CharacterNameLabel.Left = (this.Width / 2) - (this.CharacterNameLabel.Width / 2);
				this.CharacterNameLabel.Top = (this.Height / 2) - (this.CharacterNameLabel.Height + this.SystemNameLabel.Height + labelSpacing) / 2;
				this.SystemNameLabel.Left = (this.Width / 2) - (this.SystemNameLabel.Width / 2);
				this.SystemNameLabel.Top = this.CharacterNameLabel.Bottom + labelSpacing;
				break;
			case ZoomAnchor.E:
				this.CharacterNameLabel.Left = this.Width - this.CharacterNameLabel.Width - margin;
				this.CharacterNameLabel.Top = (this.Height / 2) - (this.CharacterNameLabel.Height + this.SystemNameLabel.Height + labelSpacing) / 2;
				this.SystemNameLabel.Left = this.Width - this.SystemNameLabel.Width - margin;
				this.SystemNameLabel.Top = this.CharacterNameLabel.Bottom + labelSpacing;
				break;
			case ZoomAnchor.SW:
				this.SystemNameLabel.Left = margin;
				this.SystemNameLabel.Top = this.Height - this.SystemNameLabel.Height - margin;
				this.CharacterNameLabel.Left = margin;
				this.CharacterNameLabel.Top = this.SystemNameLabel.Top - this.CharacterNameLabel.Height - labelSpacing;
				break;
			case ZoomAnchor.S:
				this.SystemNameLabel.Left = (this.Width / 2) - (this.SystemNameLabel.Width / 2);
				this.SystemNameLabel.Top = this.Height - this.SystemNameLabel.Height - margin;
				this.CharacterNameLabel.Left = (this.Width / 2) - (this.CharacterNameLabel.Width / 2);
				this.CharacterNameLabel.Top = this.SystemNameLabel.Top - this.CharacterNameLabel.Height - labelSpacing;
				break;
			case ZoomAnchor.SE:
				this.SystemNameLabel.Left = this.Width - this.SystemNameLabel.Width - margin;
				this.SystemNameLabel.Top = this.Height - this.SystemNameLabel.Height - margin;
				this.CharacterNameLabel.Left = this.Width - this.CharacterNameLabel.Width - margin;
				this.CharacterNameLabel.Top = this.SystemNameLabel.Top - this.CharacterNameLabel.Height - labelSpacing;
				break;
		}
	}

	public void EnableOverlayLabel(bool enable)
	{
		this.CharacterNameLabel.Visible = enable;
		this.SystemNameLabel.Visible = enable && !string.IsNullOrEmpty(this.SystemNameLabel.Text);
	}		protected override CreateParams CreateParams
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
