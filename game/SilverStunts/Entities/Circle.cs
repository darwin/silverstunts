using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

using Physics.Surfaces;

namespace SilverStunts.Entities
{
    public class Circle : Surface
    {
        public double x 
        {
            get { CircleSurface circle = _binder.source as CircleSurface; return circle.Center.X; }
            set { CircleSurface circle = _binder.source as CircleSurface; circle.Init(value, y, r); } 
        }
        public double y
        {
            get { CircleSurface circle = _binder.source as CircleSurface; return circle.Center.Y; }
            set { CircleSurface circle = _binder.source as CircleSurface; circle.Init(x, value, r); }
        }
        public double r
        {
            get { CircleSurface circle = _binder.source as CircleSurface; return circle.Radius; }
            set { CircleSurface circle = _binder.source as CircleSurface; circle.Init(x, y, value); }
        }

        public Circle(double x, double y, double r)
        {
            CircleSurface circle = new CircleSurface(x, y, r);
            _binder = new Visual(circle, Visual.Family.Circle, null);

            Born();
        }
    }
}
