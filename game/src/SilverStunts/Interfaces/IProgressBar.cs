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
    public interface IProgressBar
    {
        void Changed(double percentage);
        void Failed(double percentage);
        void Completed(double percentage);
    }
}
