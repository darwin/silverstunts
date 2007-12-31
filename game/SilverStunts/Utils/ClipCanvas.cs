using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;

namespace SilverStunts 
{
	public class ClipCanvas : Canvas 
	{
		private RectangleGeometry clipper;

		public ClipCanvas() 
		{
			this.clipper = new RectangleGeometry();
			this.Clip = this.clipper;
		}

		public void UpdateLayout() 
		{
			this.clipper.Rect = new Rect(0, 0, this.Width, this.Height);
		}
	}
}
