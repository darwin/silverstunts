using Physics.Surfaces;

namespace SilverStunts.Entities
{
	public class Rectangle : Surface
	{
		public double x
		{
			get { RectangleSurface rectangle = _visual.source as RectangleSurface; return rectangle.Center.X; }
			set { RectangleSurface rectangle = _visual.source as RectangleSurface; rectangle.Init(value, y, w, h); }
		}
		public double y
		{
			get { RectangleSurface rectangle = _visual.source as RectangleSurface; return rectangle.Center.Y; }
			set { RectangleSurface rectangle = _visual.source as RectangleSurface; rectangle.Init(x, value, w, h); }
		}
		public double w
		{
			get { RectangleSurface rectangle = _visual.source as RectangleSurface; return rectangle.Width; }
			set { RectangleSurface rectangle = _visual.source as RectangleSurface; rectangle.Init(x, y, value, h); }
		}
		public double h
		{
			get { RectangleSurface rectangle = _visual.source as RectangleSurface; return rectangle.Height; }
			set { RectangleSurface rectangle = _visual.source as RectangleSurface; rectangle.Init(x, y, w, value); }
		}

		public Rectangle(double x, double y, double w, double h)
		{
			RectangleSurface rectangle = new RectangleSurface(x, y, w, h);
			_visual = new Visual(rectangle, Visual.Family.Rectangle, null);

			Born();
		}
	}
}
