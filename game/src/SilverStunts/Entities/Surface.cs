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

using Physics.Surfaces;

namespace SilverStunts.Entities
{
    public class Surface : Entity
    {
        public event EventHandler onContact
        {
            add { (_binder.source as AbstractSurface).Contact += value; }
            remove { (_binder.source as AbstractSurface).Contact -= value; }
        }

        public bool active
        {
            get { return (_binder.source as ISurface).Active; }
            set { (_binder.source as ISurface).Active = value; }
        }
    }
}
