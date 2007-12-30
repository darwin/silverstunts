using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

using Physics;
using Physics.Surfaces;

namespace SilverStunts.Entities
{
    public class Line : Surface
    {
        public double x1
        {
            get { LineSurface line = _binder.source as LineSurface; return line.OP1.X; }
            set { LineSurface line = _binder.source as LineSurface; line.Init(value, y1, x2, y2); }
        }
        public double y1
        {
            get { LineSurface line = _binder.source as LineSurface; return line.OP1.Y; }
            set { LineSurface line = _binder.source as LineSurface; line.Init(x1, value, x2, y2); }
        }
        public double x2
        {
            get { LineSurface line = _binder.source as LineSurface; return line.OP2.X; }
            set { LineSurface line = _binder.source as LineSurface; line.Init(x1, y1, value, y2); }
        }
        public double y2
        {
            get { LineSurface line = _binder.source as LineSurface; return line.OP2.Y; }
            set { LineSurface line = _binder.source as LineSurface; line.Init(x1, y1, x2, value); }
        }
        
        public Line(double x1, double y1, double x2, double y2)
        {
            LineSurface line = new LineSurface(x1, y1, x2, y2);
            _binder = new Visual(line, Visual.Family.Line, null);
        
            Born();
        }
    }
}
