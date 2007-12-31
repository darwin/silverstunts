using Physics.Surfaces;

namespace SilverStunts.Entities
{
	public class Circle : Surface
	{
		public double x 
		{
			get { CircleSurface circle = _visual.source as CircleSurface; return circle.Center.X; }
			set { CircleSurface circle = _visual.source as CircleSurface; circle.Init(value, y, r); } 
		}

		public double y
		{
			get { CircleSurface circle = _visual.source as CircleSurface; return circle.Center.Y; }
			set { CircleSurface circle = _visual.source as CircleSurface; circle.Init(x, value, r); }
		}

		public double r
		{
			get { CircleSurface circle = _visual.source as CircleSurface; return circle.Radius; }
			set { CircleSurface circle = _visual.source as CircleSurface; circle.Init(x, y, value); }
		}

		public Circle(double x, double y, double r)
		{
			CircleSurface circle = new CircleSurface(x, y, r);
			_visual = new Visual(circle, Visual.Family.Circle, null);
			
			Born();
		}
	}
}
