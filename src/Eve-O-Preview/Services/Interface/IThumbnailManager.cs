using System.Collections.Generic;
using EveOPreview.View;

namespace EveOPreview.Services
{
	public interface IThumbnailManager
	{
		void Start();
		void Stop();

		void UpdateThumbnailsSize();
		void UpdateThumbnailFrames();

		IThumbnailView GetClientByTitle(string title);
		IThumbnailView GetClientByPointer(System.IntPtr ptr);
		IThumbnailView GetActiveClient();
		IEnumerable<string> GetAllClientTitles();
	}
}