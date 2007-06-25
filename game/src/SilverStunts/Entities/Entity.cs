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
    public class Entity
    {
        public Canvas xaml { get { return _binder.content; } }
        public string name { get { return _binder.source.name; } }
        public string type { 
            get {
                string s = GetType().FullName;
                return s.Substring(s.LastIndexOf('.')+1); 
            } 
        }

        protected Binder _binder;

        protected void Born()
        {
            Page.Current.Level.EntityCreated(this, _binder);
        }

        protected void Die()
        {
            Page.Current.Level.EntityDestroyed(_binder);
        }

        public void Destroy()
        {
            Die();
        }
    }
}
