using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.Generic;

using Physics;
using Physics.Composites;
using Physics.Constraints;
using Physics.Primitives;
using Physics.Surfaces;
using Physics.Util;
using Physics.Base;
using SilverStunts.Entities;

namespace SilverStunts
{
    public class DummyActor : IActor
    {
        public DummyActor()
        {
        }

        public bool IsDead()
        {
            return true;
        }

        public void Destroy()
        {
        }

        public void Move(double x, double y)
        {
        }

        public Vector GetPos()
        {
            return new Vector(0,0);
        }

        public void ProcessInputs(bool[] keys)
        {

        }
    }
}
