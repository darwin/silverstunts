using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace SilverStunts
{
    public class LevelDescriptor
    {
        public string Name { get; set; }
        public string Description { get; set; }

        public LevelDescriptor(string name)
        {
            this.Name = name;
        }
    }
}
