using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;

namespace SilverStunts 
{
	public class ClipCanvas : Canvas 
    {
		private RectangleGeometry clipGeometry;

		public ClipCanvas() 
        {
			this.clipGeometry = new RectangleGeometry();
			this.Clip = this.clipGeometry;
		}

		public void UpdateLayout() 
        {
			Rect newClip = new Rect(0, 0, this.Width, this.Height);
			this.clipGeometry.Rect = newClip;
		}
	}
}
