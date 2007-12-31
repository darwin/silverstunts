using Physics.Surfaces;

namespace SilverStunts.Entities
{
	public class Line : Surface
	{
		public double x1
		{
			get { LineSurface line = _visual.source as LineSurface; return line.OP1.X; }
			set { LineSurface line = _visual.source as LineSurface; line.Init(value, y1, x2, y2); }
		}
		public double y1
		{
			get { LineSurface line = _visual.source as LineSurface; return line.OP1.Y; }
			set { LineSurface line = _visual.source as LineSurface; line.Init(x1, value, x2, y2); }
		}
		public double x2
		{
			get { LineSurface line = _visual.source as LineSurface; return line.OP2.X; }
			set { LineSurface line = _visual.source as LineSurface; line.Init(x1, y1, value, y2); }
		}
		public double y2
		{
			get { LineSurface line = _visual.source as LineSurface; return line.OP2.Y; }
			set { LineSurface line = _visual.source as LineSurface; line.Init(x1, y1, x2, value); }
		}
        
		public Line(double x1, double y1, double x2, double y2)
		{
			LineSurface line = new LineSurface(x1, y1, x2, y2);
			_visual = new Visual(line, Visual.Family.Line, null);
        
			Born();
		}
	}
}
