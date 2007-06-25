using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Physics.Base;

namespace SilverStunts.Entities
{
    public class Generic : Entity
    {
        public Binder binder { get { return _binder; } }

        public Generic(PhysicsObject source, Binder.Family family, string selector)
        {
            _binder = new Binder(source, family, selector);

            Born();
        }
    }
}
