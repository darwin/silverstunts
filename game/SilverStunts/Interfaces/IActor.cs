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
using Physics.Composites;
using Physics.Constraints;
using Physics.Primitives;
using Physics.Surfaces;
using Physics.Util;

namespace SilverStunts
{
    public interface IActor
    {
        void ProcessInputs(bool[] keys);
        void Destroy();
        void Move(double x, double y);

        bool IsDead();
        Vector GetPos();
    }
}
