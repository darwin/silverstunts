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
    public class Layer : Control
    {
        public Layer(string source, string type)
        {
            try
            {
                this.InitializeFromXaml(source);
            }
            catch (Exception e)
            {
                Page.Current.PrintConsole("Bad "+type+" XAML: "+e.ToString()+"\n", Page.ConsoleOutputKind.Exception);
            }
        }
    }
}
